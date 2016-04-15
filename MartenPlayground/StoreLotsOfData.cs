using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Serilog;

namespace MartenPlayground
{
    class StoreLotsOfData
    {
        public static void Run(DocumentStore storeV2, int users, int batchSize)
        {
            var everyNTh = users / 10;
            Meassure.Run(() =>
            {
                var tasks = Enumerable.Range(0, users).Select(i =>
                {
                    return Task.Run(async () =>
                    {
                        if (i % everyNTh == 0) Log.Debug("{I}th customer about to be processed.", i);
                        using (var session = storeV2.OpenSession(isolationLevel:IsolationLevel.ReadCommitted))
                        {
                            var documents = Enumerable.Range(0, batchSize).Select(_ => CreateDefaultDocumentV2.Run()).ToArray();
                            session.StoreObjects(documents);
                            await session.SaveChangesAsync();                            
                        }

                        if (i % everyNTh == 0) Log.Debug("{I}th customer done.", i);
                    });
                }).ToArray();

                Log.Information("All tasks created: {Number}.", tasks.Length);

                Task.WaitAll(tasks);
            });      
        }
    }
}