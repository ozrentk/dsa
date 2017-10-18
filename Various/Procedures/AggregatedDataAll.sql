/*
DECLARE @RC int
DECLARE @ids [dbo].[Integers]
INSERT INTO @ids (Value)
SELECT DISTINCT lineid from dataitem

EXECUTE @RC = [dbo].[AggregatedDataAll] 
   @ids
*/
ALTER PROCEDURE AggregatedDataAll
	@businessIds Integers READONLY
AS
	SELECT
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
