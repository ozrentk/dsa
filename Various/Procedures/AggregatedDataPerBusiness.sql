/*
DECLARE @RC int
DECLARE @ids [dbo].[Integers]
INSERT INTO @ids (Value)
SELECT DISTINCT businessid from dataitem

EXECUTE @RC = [dbo].[AggregatedDataPerBusiness] 
   @ids
*/
ALTER PROCEDURE AggregatedDataPerBusiness
	@businessIds Integers READONLY
AS
	WITH rawData AS (
		SELECT
			BusinessId,
			AverageWaitTime = AVG(WaitTimeSec),
			AverageServiceTime = AVG(ServiceTimeSec),
			CustomersWaitingCount = COUNT(CASE WHEN Entered IS NOT NULL AND Called IS NULL THEN 1 ELSE NULL END),
			CustomersBeingServicedCount = COUNT(CASE WHEN Called IS NOT NULL AND Serviced IS NULL THEN 1 ELSE NULL END),
			CustomersServicedCount = COUNT(CASE WHEN Serviced IS NOT NULL THEN 1 ELSE NULL END),
			CustomersCount = COUNT(*)
		FROM 
			DataItem item
			JOIN @businessIds id
				ON id.Value = item.BusinessId
		GROUP BY
			item.BusinessId)
	SELECT
		BusinessId = b.Id,
		BusinessName = b.Name,
		rd.AverageWaitTime,
		rd.AverageServiceTime,
		rd.CustomersWaitingCount,
		rd.CustomersBeingServicedCount,
		rd.CustomersServicedCount,
		rd.CustomersCount
	FROM
		Business b
		JOIN rawData rd
			ON rd.BusinessId = b.Id
	ORDER BY
		b.id