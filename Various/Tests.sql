DECLARE @dateFrom datetime = '2017-09-13 00:00'
DECLARE @dateTo datetime = '2017-09-13 16:00'
--SELECT * FROM SEC.[User]
DECLARE @userName nvarchar(256) = 'ozren.krznaric@gmail.com' --'kris_11005@yahoo.com'


DECLARE @userId int
DECLARE @admin bit
SELECT 
	@userId = u.ID,
	@admin = CASE WHEN EXISTS (SELECT * FROM SEC.UserRoles ur WHERE ur.RoleId = 1 AND ur.UserId = u.Id) THEN 1 ELSE 0 END
FROM SEC.[User] u 
WHERE UserName = @userName

SELECT
	AvgWaitTime = CONVERT(varchar, DATEADD(ss, AVG(dic.WaitTime), 0), 114),
	AvgServiceTime = CONVERT(varchar, DATEADD(ss, AVG(dic.ServiceTime), 0), 114),
	TotalCustomersWaiting = SUM(CASE WHEN di.Called IS NULL THEN 1 ELSE 0 END),
	TotalCustomersBeingServiced = SUM(CASE WHEN di.Called IS NOT NULL AND di.Serviced IS NULL THEN 1 ELSE 0 END),
	TotalCustomers = COUNT(*)
FROM 
	DataItem di
	JOIN DataItemCalculation dic
		ON dic.Id = di.Id
	JOIN Business b
		ON b.Id = di.BusinessId
WHERE 
	Entered BETWEEN @dateFrom AND @dateTo
	AND (
		@admin = 0 AND di.BusinessId IN (SELECT BusinessId FROM BusinessMember WHERE UserId = @userId)
		OR
		@admin = 1 AND di.BusinessId IN (SELECT BusinessId FROM BusinessMember)
	)

SELECT
	--di.BusinessId,
	BusinessName = b.Name,
	AvgWaitTime = CONVERT(varchar, DATEADD(ss, AVG(dic.WaitTime), 0), 114),
	AvgServiceTime = CONVERT(varchar, DATEADD(ss, AVG(dic.ServiceTime), 0), 114),
	TotalCustomersWaiting = SUM(CASE WHEN di.Called IS NULL THEN 1 ELSE 0 END),
	TotalCustomersBeingServiced = SUM(CASE WHEN di.Called IS NOT NULL AND di.Serviced IS NULL THEN 1 ELSE 0 END),
	TotalCustomers = COUNT(*)
FROM 
	DataItem di
	JOIN DataItemCalculation dic
		ON dic.Id = di.Id
	JOIN Business b
		ON b.Id = di.BusinessId
WHERE 
	Entered BETWEEN @dateFrom AND @dateTo
	AND (
		@admin = 0 AND di.BusinessId IN (SELECT BusinessId FROM BusinessMember WHERE UserId = @userId)
		OR
		@admin = 1 AND di.BusinessId IN (SELECT BusinessId FROM BusinessMember)
	)
GROUP BY b.Name, di.BusinessId
ORDER BY di.BusinessId

SELECT
	--di.BusinessId,
	BusinessName = b.Name,
	--di.LineId,
	LineName = l.Name,
	AvgWaitTime = CONVERT(varchar, DATEADD(ss, AVG(dic.WaitTime), 0), 114),
	AvgServiceTime = CONVERT(varchar, DATEADD(ss, AVG(dic.ServiceTime), 0), 114),
	TotalCustomersWaiting = SUM(CASE WHEN di.Called IS NULL THEN 1 ELSE 0 END),
	TotalCustomersBeingServiced = SUM(CASE WHEN di.Called IS NOT NULL AND di.Serviced IS NULL THEN 1 ELSE 0 END),
	TotalCustomers = COUNT(*)
FROM 
	DataItem di
	JOIN DataItemCalculation dic
		ON dic.Id = di.Id
	JOIN Business b
		ON b.Id = di.BusinessId
	JOIN Line l
		ON l.Id = di.LineId
WHERE 
	Entered BETWEEN @dateFrom AND @dateTo
	AND (
		@admin = 0 AND di.BusinessId IN (SELECT BusinessId FROM BusinessMember WHERE UserId = @userId)
		OR
		@admin = 1 AND di.BusinessId IN (SELECT BusinessId FROM BusinessMember)
	)
GROUP BY b.Name, di.BusinessId, l.Name, di.LineId
ORDER BY di.BusinessId, di.LineId