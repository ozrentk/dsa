DROP TYPE [dbo].[DataItem2];
GO

ALTER TABLE [SEC].[User] DROP COLUMN [LastTicketNumber];
ALTER TABLE [SEC].[User]
    ADD [IsActive] BIT CONSTRAINT [DF_User_IsActive] DEFAULT ((1)) NOT NULL;
GO

CREATE TABLE [SEC].[Activity] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Controller]   NVARCHAR (256) NOT NULL,
    [Action]       NVARCHAR (256) NOT NULL,
    [Method]       NVARCHAR (16)  NULL,
    [PermissionId] INT            NOT NULL,
    CONSTRAINT [PK_Activity] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [SEC].[Permission] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Description] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_Permission] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE TABLE [SEC].[RolePermission] (
    [PermissionId] INT NOT NULL,
    [RoleId]       INT NOT NULL,
    CONSTRAINT [PK_RolePermission] PRIMARY KEY CLUSTERED ([PermissionId] ASC, [RoleId] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_DataItem_CalledById_Entered]
    ON [dbo].[DataItem]([CalledById] ASC, [Entered] ASC, [BusinessId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_DataItem_Entered]
    ON [dbo].[DataItem]([Entered] ASC)
    INCLUDE([Id], [BusinessId], [LineId]);
GO
GO
CREATE NONCLUSTERED INDEX [IX_DataItem_QueueId]
    ON [dbo].[DataItem]([QueueId] ASC);
GO

ALTER TABLE [SEC].[Activity] WITH NOCHECK
    ADD CONSTRAINT [FK_Activity_Permission] FOREIGN KEY ([PermissionId]) REFERENCES [SEC].[Permission] ([Id]);
GO

ALTER TABLE [SEC].[RolePermission] WITH NOCHECK
    ADD CONSTRAINT [FK_RolePermission_Roles] FOREIGN KEY ([RoleId]) REFERENCES [SEC].[Roles] ([Id]);
GO
ALTER TABLE [SEC].[RolePermission] WITH NOCHECK
    ADD CONSTRAINT [FK_RolePermission_Permission] FOREIGN KEY ([PermissionId]) REFERENCES [SEC].[Permission] ([Id]);
GO

CREATE VIEW [dbo].[DataItemCalculation]
AS
	SELECT
		Id,
		WaitTime = 1.0 * isnull(datediff(ss, di.Entered, di.Called), 0),
		ServiceTime = 1.0 * isnull(datediff(ss, di.Called, di.Serviced), 0),
		IsWaiting = CASE WHEN Entered IS NOT NULL AND di.Called IS NULL THEN 1 ELSE 0 END,
		IsBeingServiced = CASE WHEN Called IS NOT NULL AND di.Serviced IS NULL THEN 1 ELSE 0 END,
		IsServiced = CASE WHEN di.Serviced IS NOT NULL THEN 1 ELSE 0 END,
		IsCustomer = CASE WHEN di.Id IS NOT NULL THEN 1 ELSE 0 END
	FROM
		DataItem di
GO

CREATE VIEW SEC.[AllowedActivities]
AS
SELECT DISTINCT
	UserId = u.Id, 
	UserName = u.UserName, 
	ActivityId = a.Id,
	Controller = a.Controller, 
	[Action] = a.[Action],
	Method = a.Method
FROM
	SEC.[User] u
	JOIN SEC.UserRoles ur ON u.Id = ur.UserId 
	JOIN SEC.Roles r ON ur.RoleId = r.Id 
	JOIN SEC.RolePermission rp ON r.Id = rp.RoleId 
	JOIN SEC.Permission p ON rp.PermissionId = p.Id 
	JOIN SEC.Activity a ON p.Id = a.PermissionId
GO

ALTER PROCEDURE [dbo].[GetAggregatedData]
	@timeFrom datetime,
	@timeTo datetime,
	@businessIds Integers READONLY,
	@lineIds Integers READONLY
AS
	IF(@timeFrom IS NULL) THROW 51000, '@timeFrom cannot be empty', 1;
	ELSE IF(@timeFrom IS NULL) THROW 51000, '@timeTo cannot be empty', 1;

	DECLARE @allLines bit = 0
	SET @allLines = CASE WHEN NOT EXISTS(SELECT * FROM @lineIds) THEN 1 ELSE 0 END

	SELECT
		AverageWaitTime = isnull(cast(round(avg(dic.WaitTime), 0) as int), 0),
		AverageServiceTime = isnull(cast(round(avg(dic.ServiceTime), 0) as int), 0),
		CustomersWaitingCount = isnull(sum(dic.IsWaiting), 0),
		CustomersBeingServicedCount = isnull(sum(dic.IsBeingServiced), 0),
		CustomersServicedCount = isnull(sum(dic.IsServiced), 0),
		CustomersCount = isnull(sum(dic.IsCustomer), 0)
	FROM 
		Business b
		JOIN @businessIds bids
			ON bids.Value = b.Id
		JOIN Line l
			ON l.BusinessId = b.Id
		JOIN DataItem di
			ON b.Id = di.BusinessId
			AND l.Id = di.LineId
		JOIN DataItemCalculation dic
			ON di.Id = dic.Id
	WHERE 
		Entered BETWEEN @timeFrom AND @timeTo
		AND (@allLines = 1 OR (@allLines = 0 AND l.Id IN(SELECT Value FROM @lineIds)))
GO

ALTER PROCEDURE [dbo].[MergeStats]
AS
	DECLARE @refs TABLE (
		BusinessId int,
		LineId int,
		EmployeeId int,
		EnteredYear int,
		EnteredMonth int)

	MERGE EmployeeStats AS target
	USING (
		SELECT
			BusinessId,
			LineId,
			EmployeeId = CalledById,
			EnteredYear = year(Entered),
			EnteredMonth = month(Entered),
			WaitTimeSec = AVG(WaitTimeSec),
			ServiceTimeSec = AVG(ServiceTimeSec)
		FROM DataItem
		WHERE 
			CalledById IS NOT NULL -- skip unidentified records
			AND WaitTimeSec IS NOT NULL -- business-procesed items only
			AND ServiceTimeSec IS NOT NULL -- business-procesed items only
		GROUP BY
			BusinessId,
			LineId,
			CalledById,
			year(Entered),
			month(Entered)) AS source
	ON (
		target.BusinessId = source.BusinessId
		AND target.LineId = source.LineId
		AND target.EmployeeId = source.EmployeeId
		AND target.EnteredYear = source.EnteredYear
		AND target.EnteredMonth = source.EnteredMonth)  
	WHEN NOT MATCHED BY target THEN 
		INSERT (
			BusinessId,
			LineId,
			EmployeeId,
			EnteredYear,
			EnteredMonth,
			WaitTimeSec,
			ServiceTimeSec)
		VALUES (
			BusinessId,
			LineId,
			EmployeeId,
			EnteredYear,
			EnteredMonth,
			WaitTimeSec,
			ServiceTimeSec)
	WHEN MATCHED THEN
		UPDATE SET
			WaitTimeSec = source.WaitTimeSec,
			ServiceTimeSec = source.ServiceTimeSec
	OUTPUT 
		inserted.BusinessId, 
		inserted.LineId,
		inserted.EmployeeId,
		inserted.EnteredYear,
		inserted.EnteredMonth INTO @refs;

	-- Clean up: delete stats where no input data exists
	DELETE FROM EmployeeStats
	FROM 
		EmployeeStats es
		LEFT JOIN @refs r
			ON r.BusinessId = es.BusinessId
			AND r.LineId = es.LineId
			AND r.EmployeeId = es.EmployeeId
			AND r.EnteredYear = es.EnteredYear
			AND r.EnteredMonth = es.EnteredMonth
	WHERE r.EmployeeId IS NULL
GO

CREATE PROCEDURE [dbo].[GetAggregatedEmployeeData]
	@timeFrom datetime,
	@timeTo datetime,
	@businessIds Integers READONLY,
	@lineIds Integers READONLY,
	@employeeId int
AS
	IF(@timeFrom IS NULL) THROW 51000, '@timeFrom cannot be empty', 1;
	ELSE IF(@timeFrom IS NULL) THROW 51000, '@timeTo cannot be empty', 1;
	ELSE IF(@employeeId IS NULL) THROW 51000, '@employeeId cannot be empty', 1;
	
	DECLARE @allLines bit = 0
	SET @allLines = CASE WHEN NOT EXISTS(SELECT * FROM @lineIds) THEN 1 ELSE 0 END

	SELECT
		AverageWaitTime = isnull(cast(round(avg(dic.WaitTime), 0) as int), 0),
		AverageServiceTime = isnull(cast(round(avg(dic.ServiceTime), 0) as int), 0),
		CustomersWaitingCount = isnull(sum(dic.IsWaiting), 0),
		CustomersBeingServicedCount = isnull(sum(dic.IsBeingServiced), 0),
		CustomersServicedCount = isnull(sum(dic.IsServiced), 0),
		CustomersCount = isnull(sum(dic.IsCustomer), 0)
	FROM 
		Business b
		JOIN @businessIds bids
			ON bids.Value = b.Id
		JOIN Line l
			ON l.BusinessId = b.Id
		JOIN EmployeeLine el
			ON el.LineId = l.Id
		JOIN Employee e
			ON e.Id = el.EmployeeId
		JOIN DataItem di
			ON b.Id = di.BusinessId
			AND l.Id = di.LineId
			AND e.Id = di.CalledById
		JOIN DataItemCalculation dic
			ON di.Id = dic.Id
	WHERE 
		Entered BETWEEN @timeFrom AND @timeTo
		AND (@allLines = 1 OR (@allLines = 0 AND l.Id IN(SELECT Value FROM @lineIds)))
		AND e.Id = @employeeId
GO

CREATE PROCEDURE [dbo].[GetCalledEmployees]
	@timeFrom datetime,
	@timeTo datetime,
	@businessIds Integers READONLY
AS
	IF(@timeFrom IS NULL) THROW 51000, '@timeFrom cannot be empty', 1;
	ELSE IF(@timeFrom IS NULL) THROW 51000, '@timeTo cannot be empty', 1;
	
	;WITH rawItems AS (
		SELECT *
		FROM 
			DataItem di
			JOIN @businessIds bids
				ON di.BusinessId = bids.Value
		WHERE 
			Entered BETWEEN @timeFrom AND @timeTo
	)
	SELECT e.*
	FROM
		Employee e
		CROSS APPLY (
			SELECT TOP 1 *
			FROM rawItems ri
			WHERE ri.CalledById = e.Id) ext
GO

CREATE PROCEDURE [dbo].[GetDataAggregatedByBusiness]
	@timeFrom datetime,
	@timeTo datetime,
	@businessIds Integers READONLY,
	@lineIds Integers READONLY
AS
	IF(@timeFrom IS NULL) THROW 51000, '@timeFrom cannot be empty', 1;
	ELSE IF(@timeFrom IS NULL) THROW 51000, '@timeTo cannot be empty', 1;

	DECLARE @allLines bit = 0
	SET @allLines = CASE WHEN NOT EXISTS(SELECT * FROM @lineIds) THEN 1 ELSE 0 END

	IF OBJECT_ID('tempdb..#GroupedItems') IS NOT NULL
		DROP TABLE #GroupedItems

	SELECT
		BusinessId = b.Id,
		BusinessName = b.Name,
		AverageWaitTime = isnull(cast(round(avg(dic.WaitTime), 0) as int), 0),
		AverageServiceTime = isnull(cast(round(avg(dic.ServiceTime), 0) as int), 0),
		CustomersWaitingCount = isnull(sum(dic.IsWaiting), 0),
		CustomersBeingServicedCount = isnull(sum(dic.IsBeingServiced), 0),
		CustomersServicedCount = isnull(sum(dic.IsServiced), 0),
		CustomersCount = isnull(sum(dic.IsCustomer), 0)
	INTO #GroupedItems
	FROM 
		Business b
		JOIN @businessIds bids
			ON bids.Value = b.Id
		JOIN Line l
			ON l.BusinessId = b.Id
		JOIN DataItem di
			ON b.Id = di.BusinessId
			AND l.Id = di.LineId
		JOIN DataItemCalculation dic
			ON di.Id = dic.Id
	WHERE 
		Entered BETWEEN @timeFrom AND @timeTo
		AND (@allLines = 1 OR (@allLines = 0 AND l.Id IN(SELECT Value FROM @lineIds)))
	GROUP BY
		b.Id,
		b.Name

	SELECT
		BusinessId = b.Id,
		BusinessName = b.Name,
		AverageWaitTime = isnull(AverageWaitTime, 0),
		AverageServiceTime = isnull(AverageServiceTime, 0),
		CustomersWaitingCount = isnull(CustomersWaitingCount, 0),
		CustomersBeingServicedCount = isnull(CustomersBeingServicedCount, 0),
		CustomersServicedCount = isnull(CustomersServicedCount, 0),
		CustomersCount = isnull(CustomersCount, 0)
	FROM 
		Business b
		JOIN @businessIds bids
			ON bids.Value = b.Id
		LEFT JOIN #GroupedItems gi
			ON b.Id = gi.BusinessId
	ORDER BY
		b.Name
GO

CREATE PROCEDURE [dbo].[GetDataAggregatedByLine]
	@timeFrom datetime,
	@timeTo datetime,
	@businessIds Integers READONLY,
	@lineIds Integers READONLY
AS
	IF(@timeFrom IS NULL) THROW 51000, '@timeFrom cannot be empty', 1;
	ELSE IF(@timeFrom IS NULL) THROW 51000, '@timeTo cannot be empty', 1;

	DECLARE @allLines bit = 0
	SET @allLines = CASE WHEN NOT EXISTS(SELECT * FROM @lineIds) THEN 1 ELSE 0 END

	IF OBJECT_ID('tempdb..#GroupedItems') IS NOT NULL
		DROP TABLE #GroupedItems

	SELECT
		BusinessId = b.Id,
		BusinessName = b.Name,
		LineId = l.Id,
		LineName = l.Name,
		AverageWaitTime = isnull(cast(round(avg(dic.WaitTime), 0) as int), 0),
		AverageServiceTime = isnull(cast(round(avg(dic.ServiceTime), 0) as int), 0),
		CustomersWaitingCount = isnull(sum(dic.IsWaiting), 0),
		CustomersBeingServicedCount = isnull(sum(dic.IsBeingServiced), 0),
		CustomersServicedCount = isnull(sum(dic.IsServiced), 0),
		CustomersCount = isnull(sum(dic.IsCustomer), 0)
	INTO #GroupedItems
	FROM 
		Business b
		JOIN @businessIds bids
			ON bids.Value = b.Id
		JOIN Line l
			ON l.BusinessId = b.Id
		JOIN DataItem di
			ON b.Id = di.BusinessId
			AND l.Id = di.LineId
		JOIN DataItemCalculation dic
			ON di.Id = dic.Id
	WHERE 
		Entered BETWEEN @timeFrom AND @timeTo
		AND (@allLines = 1 OR (@allLines = 0 AND l.Id IN(SELECT Value FROM @lineIds)))
	GROUP BY
		b.Id,
		b.Name,
		l.Id,
		l.Name

	SELECT
		BusinessId = b.Id,
		BusinessName = b.Name,
		LineId = l.Id,
		LineName = l.Name,
		AverageWaitTime = isnull(AverageWaitTime, 0),
		AverageServiceTime = isnull(AverageServiceTime, 0),
		CustomersWaitingCount = isnull(CustomersWaitingCount, 0),
		CustomersBeingServicedCount = isnull(CustomersBeingServicedCount, 0),
		CustomersServicedCount = isnull(CustomersServicedCount, 0),
		CustomersCount = isnull(CustomersCount, 0)
	FROM 
		Business b
		JOIN @businessIds bids
			ON bids.Value = b.Id
		JOIN Line l
			ON l.BusinessId = b.Id
		LEFT JOIN #GroupedItems gi
			ON b.Id = gi.BusinessId
			AND l.Id = gi.LineId
	ORDER BY
		b.Name,
		l.Name
GO

CREATE PROCEDURE [dbo].[GetDataAggregatedByLineName]
	@timeFrom datetime,
	@timeTo datetime,
	@businessIds Integers READONLY,
	@lineIds Integers READONLY
AS
	IF(@timeFrom IS NULL) THROW 51000, '@timeFrom cannot be empty', 1;
	ELSE IF(@timeFrom IS NULL) THROW 51000, '@timeTo cannot be empty', 1;

	DECLARE @allLines bit = 0
	SET @allLines = CASE WHEN NOT EXISTS(SELECT * FROM @lineIds) THEN 1 ELSE 0 END

	IF OBJECT_ID('tempdb..#GroupedItems') IS NOT NULL
		DROP TABLE #GroupedItems

	SELECT
		LineName = l.Name,
		AverageWaitTime = isnull(cast(round(avg(dic.WaitTime), 0) as int), 0),
		AverageServiceTime = isnull(cast(round(avg(dic.ServiceTime), 0) as int), 0),
		CustomersWaitingCount = isnull(sum(dic.IsWaiting), 0),
		CustomersBeingServicedCount = isnull(sum(dic.IsBeingServiced), 0),
		CustomersServicedCount = isnull(sum(dic.IsServiced), 0),
		CustomersCount = isnull(sum(dic.IsCustomer), 0)
	INTO #GroupedItems
	FROM 
		Business b
		JOIN @businessIds bids
			ON bids.Value = b.Id
		JOIN Line l
			ON l.BusinessId = b.Id
		JOIN DataItem di
			ON b.Id = di.BusinessId
			AND l.Id = di.LineId
		JOIN DataItemCalculation dic
			ON di.Id = dic.Id
	WHERE 
		Entered BETWEEN @timeFrom AND @timeTo
		AND (@allLines = 1 OR (@allLines = 0 AND l.Id IN(SELECT Value FROM @lineIds)))
	GROUP BY
		l.Name

	;WITH lineName AS (
		SELECT DISTINCT Name
		FROM Line
	)
	SELECT
		LineName = ln.Name,
		AverageWaitTime = isnull(AverageWaitTime, 0),
		AverageServiceTime = isnull(AverageServiceTime, 0),
		CustomersWaitingCount = isnull(CustomersWaitingCount, 0),
		CustomersBeingServicedCount = isnull(CustomersBeingServicedCount, 0),
		CustomersServicedCount = isnull(CustomersServicedCount, 0),
		CustomersCount = isnull(CustomersCount, 0)
	FROM 
		lineName ln
		LEFT JOIN #GroupedItems gi
			ON ln.Name = gi.LineName
	ORDER BY
		ln.Name
GO

CREATE PROCEDURE [dbo].[GetEmployeeMonthlyStats]
	@month int,
	@employeeIds Integers READONLY
AS
	IF(@month IS NULL) THROW 51000, '@month cannot be empty', 1;
	
	SELECT
		EmployeeId = e.Id,
		EmployeeName = e.Name,
		AverageServiceTime = AVG(es.ServiceTimeSec)
	FROM 
		Employee e
		JOIN @employeeIds eids
			ON eids.Value = e.Id
		JOIN EmployeeStats es
			ON es.EmployeeId = e.Id
	WHERE 
		es.EnteredMonth = @month
	GROUP BY
		e.Id,
		e.Name
GO

CREATE PROCEDURE [dbo].[GetEmployeeStats]
	@timeFrom datetime,
	@timeTo datetime,
	@employeeIds Integers READONLY
AS
	IF(@timeFrom IS NULL) THROW 51000, '@timeFrom cannot be empty', 1;
	ELSE IF(@timeFrom IS NULL) THROW 51000, '@timeTo cannot be empty', 1;

	DECLARE @month int = month(@timeFrom)
	DECLARE @year int = month(@timeFrom)
	
	;WITH monthly AS (
		SELECT
			EmployeeId = eids.Value,
			ServiceTimeSec = AVG(es.ServiceTimeSec)
		FROM 
			@employeeIds eids
			JOIN EmployeeStats es
				ON es.EmployeeId = eids.Value
		WHERE es.EnteredMonth = @month
		GROUP BY eids.Value)
	,yearly AS (
		SELECT
			EmployeeId = eids.Value,
			ServiceTimeSec = AVG(es.ServiceTimeSec)
		FROM 
			@employeeIds eids
			JOIN EmployeeStats es
				ON es.EmployeeId = eids.Value
		WHERE es.EnteredYear = @year
		GROUP BY eids.Value)
	,daily AS (
		SELECT 
			EmployeeId = eids.Value,
			--di.BusinessId,
			--di.LineId,
			--WaitTimeSec = isnull(cast(round(avg(dic.WaitTime), 0) as int), 0),
			ServiceTimeSec = isnull(cast(round(avg(dic.ServiceTime), 0) as int), 0)
		FROM 
			@employeeIds eids
			JOIN DataItem di
				ON di.CalledById = eids.Value
			JOIN DataItemCalculation dic
				ON di.Id = dic.Id
		WHERE 
			di.Entered BETWEEN @timeFrom AND @timeTo
		GROUP BY
			eids.Value,
			di.BusinessId,
			di.LineId)
	SELECT
		EmployeeId = e.Id,
		EmployeeName = e.Name,
		DailyServiceTimeSec = d.ServiceTimeSec,
		MonthlyServiceTimeSec = m.ServiceTimeSec,
		YearlyServiceTimeSec = y.ServiceTimeSec
	FROM
		Employee e
		JOIN daily d
			ON d.EmployeeId = e.Id
		JOIN monthly m
			ON m.EmployeeId = e.Id
		JOIN yearly y
			ON y.EmployeeId = e.Id
GO

CREATE PROCEDURE [dbo].[GetEmployeeTimes]
	@timeFrom datetime,
	@timeTo datetime,
	@employeeIds Integers READONLY
AS

	IF(@timeFrom IS NULL) THROW 51000, '@timeFrom cannot be empty', 1;
	ELSE IF(@timeFrom IS NULL) THROW 51000, '@timeTo cannot be empty', 1;

	DECLARE @month int = month(@timeFrom)
	DECLARE @year int = year(@timeFrom)
	
	;WITH monthly AS (
		SELECT
			EmployeeId = eids.Value,
			ServiceTimeSec = AVG(es.ServiceTimeSec)
		FROM 
			@employeeIds eids
			JOIN EmployeeStats es
				ON es.EmployeeId = eids.Value
		WHERE es.EnteredMonth = @month
		GROUP BY eids.Value)
	,yearly AS (
		SELECT
			EmployeeId = eids.Value,
			ServiceTimeSec = AVG(es.ServiceTimeSec)
		FROM 
			@employeeIds eids
			JOIN EmployeeStats es
				ON es.EmployeeId = eids.Value
		WHERE es.EnteredYear = @year
		GROUP BY eids.Value)
	,daily AS (
		SELECT 
			EmployeeId = eids.Value,
			--di.BusinessId,
			--di.LineId,
			--WaitTimeSec = isnull(cast(round(avg(dic.WaitTime), 0) as int), 0),
			ServiceTimeSec = isnull(cast(round(avg(dic.ServiceTime), 0) as int), 0)
		FROM 
			@employeeIds eids
			JOIN DataItem di
				ON di.CalledById = eids.Value
			JOIN DataItemCalculation dic
				ON di.Id = dic.Id
		WHERE 
			di.Entered BETWEEN @timeFrom AND @timeTo
		GROUP BY
			eids.Value)
	SELECT
		EmployeeId = e.Id,
		EmployeeName = e.Name,
		DailyServiceTimeSec = d.ServiceTimeSec,
		MonthlyServiceTimeSec = m.ServiceTimeSec,
		YearlyServiceTimeSec = y.ServiceTimeSec
	FROM
		Employee e
		JOIN @employeeIds eids
			ON e.Id = eids.Value
		LEFT JOIN daily d
			ON d.EmployeeId = e.Id
		LEFT JOIN monthly m
			ON m.EmployeeId = e.Id
		LEFT JOIN yearly y
			ON y.EmployeeId = e.Id
	ORDER BY d.ServiceTimeSec
GO

CREATE PROCEDURE [dbo].[GetEmployeeYearlyStats]
	@year int,
	@employeeIds Integers READONLY
AS
	IF(@year IS NULL) THROW 51000, '@year cannot be empty', 1;
	
	SELECT
		EmployeeId = e.Id,
		EmployeeName = e.Name,
		AverageServiceTime = AVG(es.ServiceTimeSec)
	FROM 
		Employee e
		JOIN @employeeIds eids
			ON eids.Value = e.Id
		JOIN EmployeeStats es
			ON es.EmployeeId = e.Id
	WHERE 
		es.EnteredYear = @year
	GROUP BY
		e.Id,
		e.Name
GO

CREATE PROCEDURE [SEC].[GetAllowedActivitiesChecksum]
AS
BEGIN
	DECLARE 
		@cs1 int = (SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM SEC.[User] WITH (NOLOCK)),
		@cs2 int = (SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM SEC.UserRoles WITH (NOLOCK)),
		@cs3 int = (SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM SEC.Roles WITH (NOLOCK)),
		@cs4 int = (SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM SEC.RolePermission WITH (NOLOCK)),
		@cs5 int = (SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM SEC.Permission WITH (NOLOCK)),
		@cs6 int = (SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM SEC.Activity WITH (NOLOCK))

	PRINT @cs1
	PRINT @cs2
	PRINT @cs3
	PRINT @cs4
	PRINT @cs5
	PRINT @cs6

	SELECT
		CAST(@cs1 AS nvarchar(11)) + '/' +
		CAST(@cs2 AS nvarchar(11)) + '/' +
		CAST(@cs3 AS nvarchar(11)) + '/' +
		CAST(@cs4 AS nvarchar(11)) + '/' +
		CAST(@cs5 AS nvarchar(11)) + '/' +
		CAST(@cs6 AS nvarchar(11))
END
GO

ALTER TABLE [SEC].[Activity] WITH CHECK CHECK CONSTRAINT [FK_Activity_Permission];
ALTER TABLE [SEC].[RolePermission] WITH CHECK CHECK CONSTRAINT [FK_RolePermission_Roles];
ALTER TABLE [SEC].[RolePermission] WITH CHECK CHECK CONSTRAINT [FK_RolePermission_Permission];
GO