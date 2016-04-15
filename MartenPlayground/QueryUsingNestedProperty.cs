using System.Linq;
using Marten;
using Shouldly;

namespace MartenPlayground
{
    class QueryUsingNestedProperty
    {
        public static void Run(DocumentStore store)
        {
            var document = StoreSingleDocumentInternalV1.Run(store);
            Meassure.Run(() =>
            {
                using (var session = store.OpenSession())
                {
                    var result = session.Query<Domain.Document>().Single(d => d.Child.NestedProperty == document.Child.NestedProperty);
                    result.Child.NestedProperty.ShouldBe(document.Child.NestedProperty);
                }
            });
        }
    }
}