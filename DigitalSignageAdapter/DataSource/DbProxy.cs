using AdapterDb;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalSignageAdapter.DataSource
{
    public class DbProxy
    {
        public static int? GetLastQueueId()
        {
            var lastStoredItem = Database.Last<DataItem, int, Models.Shared.DataItem>(di => di.QueueId, (dataItem) =>
            {
                if (dataItem == null)
                    return null;

                var dataItem2 = Mapper.Map<DataItem, Models.Shared.DataItem>(dataItem);
                return dataItem2;
            });

            var lastQueueId = (lastStoredItem != null) ? lastStoredItem.QueueId : null;

            return lastQueueId;
        }
    }
}