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
            //Paging
            //Serilog
            //Slow startu up
            //Add Seperate properties
            //Migraations
            //Read http://jasperfx.github.io/marten/documentation/documents/saved_queries/
            //Server status to monitor questions
            //How to setup database
            try
            {
                ConfigureLogging();
                Log.Information("Starting execution ...");

                var store = CreateStore();

                DoNothing(store);
                StoreSingleDocument(store);
                StoreSingleDocument(store);
                QueryUsingRawSql(store);
            }
            catch (Exception e)
            {
                Log.Error(e, "Something went terribly wrong :(");                
            }

            Console.ReadLine();
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

        public static Document CreateDefaultDocument()
        {
            return new Document(Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), new Child(Guid.NewGuid().ToString()));
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
