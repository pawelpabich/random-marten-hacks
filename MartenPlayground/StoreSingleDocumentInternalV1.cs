using Marten;
using MartenPlayground.Domain;

namespace MartenPlayground
{
    class StoreSingleDocumentInternalV1
    {
        public static Document Run(DocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                var document = CreateDefaultDocumentV1.Run();
                session.Store(document);
                session.SaveChanges();

                return document;
            }
        }
    }
}