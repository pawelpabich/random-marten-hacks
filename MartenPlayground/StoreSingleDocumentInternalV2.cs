using Marten;

namespace MartenPlayground
{
    class StoreSingleDocumentInternalV2
    {
        public static void Run(DocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                session.Store(CreateDefaultDocumentV2.Run());
                session.SaveChanges();
            }
        }
    }
}