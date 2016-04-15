using System;
using System.Linq;
using Marten;
using Marten.Linq;
using MartenPlayground.Domain;
using Serilog;
using Shouldly;

namespace MartenPlayground
{
    class QueryUsingPaging
    {
        public static void Run(DocumentStore store)
        {
            var searchTerm = Guid.NewGuid().ToString();

            using (var session = store.OpenSession())
            {
                var documents= Enumerable.Range(0, 10).Select(i => CreateDefaultDocumentV1.Run(searchTerm + i));
                session.StoreObjects(documents);
                session.SaveChanges();
            }

            Meassure.Run(() =>
            {
                using (var session = store.OpenSession())
                {
                    var query = session.Query<Document>().Skip(5).Take(5).Where(d => d.TopLevelProperty.Contains(searchTerm));
                    var command = query.ToCommand(FetchType.FetchMany);
                    Log.Debug("Query with pagging: {Query}.", command.CommandText);
                    query.ToArray().Length.ShouldBe(5);
                }
            });
        }
    }
}