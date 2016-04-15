using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Marten;
using Marten.Linq;
using MartenPlayground.Domain;
using Npgsql;
using Serilog;
using Shouldly;

namespace MartenPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Marten can take up to 6 seconds to start up. Details here: https://github.com/JasperFx/marten/issues/289
                ConfigureLogging();
                Log.Information("Starting execution ...");

                var storeV1 = CreateStoreV1();

                StoreSingleDocument(storeV1);
                StoreSingleDocument(storeV1);

                QueryUsingRawSql(storeV1);
                QueryUsingRawSql(storeV1);

                QueryUsingNestedProperty(storeV1);
                QueryUsingNestedProperty(storeV1);

                QueryUsingDuplicatedNestedProperty(storeV1);
                QueryUsingDuplicatedNestedProperty(storeV1);

                QueryUsingPaging(storeV1);
                QueryUsingPaging(storeV1);

                UpdateDocument(storeV1);
                UpdateDocument(storeV1);

                var storeV2 = CreateStoreV2();

                MigrateToDocumentV2(storeV1, storeV2);

                QueryUsingDateRange(storeV2);
                QueryUsingDateRange(storeV2);

            }
            catch (Exception e)
            {
                Log.Error(e, "Something went terribly wrong :(");                
            }

            Console.ReadLine();
        }

        private static void QueryUsingDateRange(DocumentStore storeV2)
        {
            var dateTime = DateTimeOffset.UtcNow;
            var searchText = Guid.NewGuid().ToString();

            using (var session = storeV2.OpenSession())
            {
                var documents = Enumerable.Range(0, 10).Select(i => CreateDefaultDocumentV2(dateTime.AddDays(i), searchText));
                session.StoreObjects(documents);
                session.SaveChanges();
            }

            Meassure(() =>
            {
                using (var session = storeV2.OpenSession())
                {
                    var future = dateTime.AddDays(4.5).ToUnixTimeMilliseconds();
                    var results = session.Query<Domain.V2.Document>().Where(d => d.DateTimeAsUnixTime > future && d.TopLevelProperty == searchText).ToArray();
                    results.Length.ShouldBe(5);
                }
            });
        }

        private static void MigrateToDocumentV2(DocumentStore storeV1, DocumentStore storeV2)
        {
            var documentId = StoreSingleDocumentInternal(storeV1).Id;
            var nowAsUnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Meassure(() =>
            {
                using (var session = storeV2.OpenSession())
                {
                    var documentV2 = session.Load<Domain.V2.Document>(documentId);
                    documentV2.DateTimeAsUnixTime.ShouldBe(0);

                    var removeProperty = new NpgsqlCommand("Update mt_doc_document SET data = data - 'TopLevelProperty'", session.Connection);
                    removeProperty.ExecuteNonQuery();

                    
                    var addCommand = "Update mt_doc_document SET data = jsonb_set(data, '{DateTimeAsUnixTime}', '" + nowAsUnixTime + "', true)";
                    var addProperty = new NpgsqlCommand(addCommand, session.Connection);
                    addProperty.ExecuteNonQuery();

                    session.SaveChanges();
                }
            });

            using (var session = storeV2.OpenSession())
            {
                var documentV2 = session.Load<Domain.V2.Document>(documentId);
                documentV2.DateTimeAsUnixTime.ShouldBe(nowAsUnixTime);
            }
        }

        private static void UpdateDocument(DocumentStore store)
        {
            var document = StoreSingleDocumentInternal(store);

            var newValue = Guid.NewGuid().ToString();
            Meassure(() =>
            {
                using (var session = store.OpenSession())
                {
                    var copy = session.Load<Document>(document.Id);
                    copy.SetTopLevelProperty(newValue);
                    session.Store(copy);
                    session.SaveChanges();
                }
            });

            using (var session = store.OpenSession())
            {
                var copy = session.Load<Document>(document.Id);
                copy.TopLevelProperty.ShouldBe(newValue);
            }
        }

        private static void QueryUsingPaging(DocumentStore store)
        {
            var searchTerm = Guid.NewGuid().ToString();

            using (var session = store.OpenSession())
            {
                var documents= Enumerable.Range(0, 10).Select(i => CreateDefaultDocumentV1(searchTerm + i));
                session.StoreObjects(documents);
                session.SaveChanges();
            }

            Meassure(() =>
            {
                using (var session = store.OpenSession())
                {
                    var query = session.Query<Document>().Skip(5).Take(5).Where(d => d.TopLevelProperty.Contains(searchTerm));
                    var text = query.ToCommand(FetchType.FetchMany);
                    Log.Debug("Query with pagging: {Query}.", text);
                    query.ToArray().Length.ShouldBe(5);
                }
            });
        }

        private static void QueryUsingDuplicatedNestedProperty(DocumentStore store)
        {
            var document = StoreSingleDocumentInternal(store);
            Meassure(() =>
            {
                using (var session = store.OpenSession())
                {
                    var result = session.Query<Document>().Single(d => d.Child.DuplicatedNestedProperty == document.Child.DuplicatedNestedProperty);
                    result.Child.DuplicatedNestedProperty.ShouldBe(document.Child.DuplicatedNestedProperty);
                }
            });
        }

        private static void QueryUsingNestedProperty(DocumentStore store)
        {
            var document = StoreSingleDocumentInternal(store);
            Meassure(() =>
            {
                using (var session = store.OpenSession())
                {
                    var result = session.Query<Document>().Single(d => d.Child.NestedProperty == document.Child.NestedProperty);
                    result.Child.NestedProperty.ShouldBe(document.Child.NestedProperty);
                }
            });
        }

        private static void QueryUsingRawSql(DocumentStore store)
        {
            var document = StoreSingleDocumentInternal(store);
            Meassure(() =>
            {
                using (var session = store.OpenSession())
                {
                    var result = session.Query<Document>($"where data->> 'TopLevelProperty' = '{document.TopLevelProperty}'").Single();
                    result.TopLevelProperty.ShouldBe(document.TopLevelProperty);
                }
            });
        }

        private static DocumentStore CreateStoreV1()
        {
            return DocumentStore.For(config =>
            {
                config.Connection("host = localhost; database = marten; password = password; username = martenuser");
                config.Schema.Include<CustomRegistry>();
                config.Schema.For<Document>().DocumentAlias("document");
            });
        }

        private static DocumentStore CreateStoreV2()
        {
            return DocumentStore.For(config =>
            {
                config.Connection("host = localhost; database = marten; password = password; username = martenuser");
                config.Schema.Include<CustomRegistryV2>();
                config.Schema.For<Domain.V2.Document>().DocumentAlias("document");
            });
        }

        private static void StoreSingleDocument(DocumentStore store)
        {
            Meassure(() => StoreSingleDocumentInternal(store));
        }

        private static Document StoreSingleDocumentInternal(DocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                var document = CreateDefaultDocumentV1();
                session.Store(document);
                session.SaveChanges();

                return document;
            }
        }

        public static Document CreateDefaultDocumentV1(string topLevelProperty = null)
        {
            topLevelProperty = topLevelProperty ?? Guid.NewGuid().ToString();
            var child = new Child(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            return new Document(Guid.NewGuid(), topLevelProperty, Guid.NewGuid().ToString(), child);
        }

        public static Domain.V2.Document CreateDefaultDocumentV2(DateTimeOffset? dateTime = null, string topLevelProperty = null)
        {           
            dateTime = dateTime ?? DateTimeOffset.UtcNow;
            topLevelProperty = topLevelProperty ?? Guid.NewGuid().ToString();

            var child = new Child(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            return new Domain.V2.Document(Guid.NewGuid(), topLevelProperty, Guid.NewGuid().ToString(),
                                          dateTime.Value.ToUnixTimeMilliseconds(), child);
        }

        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.LiterateConsole()
                                                .CreateLogger();
        }

        private static void Meassure(Action action, [CallerMemberName] string memberName = "")
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            var message = $"{memberName} took: {stopwatch.Elapsed.TotalMilliseconds} ms.";
            if (stopwatch.Elapsed < TimeSpan.FromSeconds(1))
            {
                Log.Information(message);
            }
            else
            {
                Log.Warning(message);
            }
        }

        private static TResult Meassure<TResult>(Func<TResult> func, [CallerMemberName] string memberName = "")
        {
            var result = default(TResult);
            Action action = () =>
            {
                result = func();
            };

            Meassure(action, memberName);
            return result;            
        }
    }
}
