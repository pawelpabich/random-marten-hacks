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
            //Slow startu up
            //Migrations
            //Dates
            //Read http://jasperfx.github.io/marten/documentation/documents/saved_queries/
            //Server status to monitor questions  store.Diagnostics.CommandFor(q
            //How to setup database
            //Fix command generation ToCommand http://jasperfx.github.io/marten/documentation/documents/diagnostics/#sec3
            try
            {
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
            }
            catch (Exception e)
            {
                Log.Error(e, "Something went terribly wrong :(");                
            }

            Console.ReadLine();
        }

        private static void MigrateToDocumentV2(DocumentStore storeV1, DocumentStore storeV2)
        {
            var documentId = StoreSingleDocumentInternal(storeV1).Id;

            Meassure(() =>
            {
                using (var session = storeV2.OpenSession())
                {
                    var documentV2 = session.Load<Domain.V2.Document>(documentId);
                    documentV2.TopLevelPropertyV2.ShouldBe(null);

                    var removeProperty = new NpgsqlCommand("Update mt_doc_document SET data = data - 'TopLevelProperty'", session.Connection);
                    removeProperty.ExecuteNonQuery();

                    var addProperty = new NpgsqlCommand("Update mt_doc_document SET data = jsonb_set(data, '{TopLevelPropertyV2}', '\"TopLevelPropertyV2\"', true)", session.Connection);
                    addProperty.ExecuteNonQuery();

                    session.SaveChanges();
                }
            });

            using (var session = storeV2.OpenSession())
            {
                var documentV2 = session.Load<Domain.V2.Document>(documentId);
                documentV2.TopLevelPropertyV2.ShouldBe("TopLevelPropertyV2");
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
                var documents= Enumerable.Range(0, 10).Select(i => CreateDefaultDocument(searchTerm + i));
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

        private static Document StoreSingleDocument(DocumentStore store)
        {
            return Meassure(() => StoreSingleDocumentInternal(store));
        }

        private static Document StoreSingleDocumentInternal(DocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                var document = CreateDefaultDocument();
                session.Store(document);
                session.SaveChanges();

                return document;
            }
        }

        public static Document CreateDefaultDocument(string topLevelProperty = null)
        {
            topLevelProperty = topLevelProperty ?? Guid.NewGuid().ToString();
            var child = new Child(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            return new Document(Guid.NewGuid(), topLevelProperty, Guid.NewGuid().ToString(), child);
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
