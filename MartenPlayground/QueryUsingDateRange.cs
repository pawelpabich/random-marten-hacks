using System;
using System.Linq;
using Marten;
using Shouldly;

namespace MartenPlayground
{
    class QueryUsingDateRange
    {
        public static void Run(DocumentStore storeV2)
        {
            var dateTime = DateTimeOffset.UtcNow;
            var searchText = Guid.NewGuid().ToString();

            using (var session = storeV2.OpenSession())
            {
                var documents = Enumerable.Range(0, 10).Select(i => CreateDefaultDocumentV2.Run(dateTime.AddDays(i), searchText));
                session.StoreObjects(documents);
                session.SaveChanges();
            }

            Meassure.Run(() =>
            {
                using (var session = storeV2.OpenSession())
                {
                    var future = dateTime.AddDays(4.5).ToUnixTimeMilliseconds();
                    var results = session.Query<Domain.V2.Document>().Where(d => d.DateTimeAsUnixTime > future && d.TopLevelProperty == searchText).ToArray();
                    results.Length.ShouldBe(5);
                }
            });
        }
    }
}