using Marten;

namespace MartenPlayground
{
    class StoreSingleDocument
    {
        public static void Run(DocumentStore store)
        {
            Meassure.Run(() => StoreSingleDocumentInternalV1.Run(store));
        }
    }
}