using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Marten;
using MartenPlayground.Domain;
using Serilog;
using Shouldly;

namespace MartenPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            //Slow startu up
            //Migraations
            //Read http://jasperfx.github.io/marten/documentation/documents/saved_queries/
            //Server status to monitor questions
            //How to setup database
            try
            {
                ConfigureLogging();
                Log.Information("Starting execution ...");

                var store = CreateStore();

                StoreSingleDocument(store);
                StoreSingleDocument(store);

                QueryUsingRawSql(store);
                QueryUsingRawSql(store);

                QueryUsingNestedProperty(store);
                QueryUsingNestedProperty(store);

                QueryUsingDuplicatedNestedProperty(store);
                QueryUsingDuplicatedNestedProperty(store);

                QueryUsingPaging(store);
                QueryUsingPaging(store);
            }
            catch (Exception e)
            {
                Log.Error(e, "Something went terribly wrong :(");                
            }

            Console.ReadLine();
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
                    var results = session.Query<Document>().Skip(5).Take(5).Where(d => d.TopLevelProperty.Contains(searchTerm)).ToArray();
                    results.Length.ShouldBe(5);
                }
            });
        }

        private static void QueryUsingDuplicatedNestedProperty(DocumentStore store)
        {
            var document = StoreSingleDocument(store);
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
            var document = StoreSingleDocument(store);
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
            var document = StoreSingleDocument(store);
            Meassure(() =>
            {
                using (var session = store.OpenSession())
                {
                    var result = session.Query<Document>($"where data->> 'TopLevelProperty' = '{document.TopLevelProperty}'").Single();
                    result.TopLevelProperty.ShouldBe(document.TopLevelProperty);
                }
            });
        }

        private static DocumentStore CreateStore()
        {
            return DocumentStore.For(config =>
            {
                config.Connection("host = localhost; database = marten; password = password; username = martenuser");
                config.Schema.Include<CustomRegistry>();                
            });
        }

        private static Document StoreSingleDocument(DocumentStore store)
        {
            return Meassure(() =>
            {
                using (var session = store.OpenSession())
                {
                    var document = CreateDefaultDocument();
                    session.Store(document);
                    session.SaveChanges();

                    return document;
                }
            });
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
