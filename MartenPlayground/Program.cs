using System;
using Serilog;

namespace MartenPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Marten can take up to 6 seconds to start up. Details here: https://github.com/JasperFx/marten/issues/289
                ConfigureLogging.Run();
                Log.Information("Starting execution ...");

                var storeV1 = CreateStore.V1();

                StoreSingleDocument.Run(storeV1);
                StoreSingleDocument.Run(storeV1);

                QueryUsingRawSql.Run(storeV1);
                QueryUsingRawSql.Run(storeV1);

                QueryUsingNestedProperty.Run(storeV1);
                QueryUsingNestedProperty.Run(storeV1);

                QueryUsingDuplicatedNestedProperty.Run(storeV1);
                QueryUsingDuplicatedNestedProperty.Run(storeV1);

                QueryUsingPaging.Run(storeV1);
                QueryUsingPaging.Run(storeV1);

                UpdateDocument.Run(storeV1);
                UpdateDocument.Run(storeV1);

                var storeV2 = CreateStore.V2();

                MigrateToDocumentV2.Run(storeV1, storeV2);

                QueryUsingDateRange.Run(storeV2);
                QueryUsingDateRange.Run(storeV2);

                StoreSingleDocumentInternalV2.Run(storeV2);
                StoreLotsOfData.Run(storeV2, 10, 1);
                StoreLotsOfData.Run(storeV2, 1000, 1);

                QueryLotsOfData.Run(storeV2, 10);
                QueryLotsOfData.Run(storeV2, 1000);
            }
            catch (Exception e)
            {
                Log.Error(e, "Something went terribly wrong :(.");                
            }

            Console.ReadLine();
        }
    }
}
