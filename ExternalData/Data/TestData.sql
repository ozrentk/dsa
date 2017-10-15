declare @id int
select @id = max(queueid) from DataItem

IF OBJECT_ID('tempdb..#testdataitem') IS NOT NULL DROP TABLE #testdataitem

select top 1000
	BusinessCode = b.Code,
	Line = l.Code,
	di.ServiceId,
	AnalyticId = -1,
	QueueId = @id + ROW_NUMBER() over (order by QueueId),
	di.Name,
	di.Verification,
	Serviced = t2,
	di.ServicedByName,
	Called = t1,
	di.CalledByName,
	Entered = dateadd(ss, -6942, t0) 
into #testdataitem
from 
	DataItem di
	JOIN Business b ON b.Id = di.BusinessId
	JOIN Line l ON l.Id = di.LineId
	CROSS APPLY(
		select t0 = dateadd(ss, -(ABS(CHECKSUM(NewId())) % 86400), getutcdate())
	) ext0
	CROSS APPLY(
		select t1 = dateadd(ss, (ABS(CHECKSUM(NewId())) % 900), t0)
	) ext1
	CROSS APPLY(
		select t2 = dateadd(ss, (ABS(CHECKSUM(NewId())) % 900), t1)
	) ext2
order by newid()

declare @dataitem DataItem

INSERT INTO @dataitem
select * from #testdataitem

begin tran
exec MergeItems @dataitem
rollback

/*
CREATE NONCLUSTERED INDEX [IX_DataItem_QueueId]
ON [dbo].[DataItem] ([QueueId])
*/