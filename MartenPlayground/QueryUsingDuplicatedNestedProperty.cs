using System.Linq;
using Marten;
using Shouldly;

namespace MartenPlayground
{
    class QueryUsingDuplicatedNestedProperty
    {
        public static void Run(DocumentStore store)
        {
            var document = StoreSingleDocumentInternalV1.Run(store);
            Meassure.Run(() =>
            {
                using (var session = store.OpenSession())
                {
                    var result = session.Query<Domain.Document>().Single(d => d.Child.DuplicatedNestedProperty == document.Child.DuplicatedNestedProperty);
                    result.Child.DuplicatedNestedProperty.ShouldBe(document.Child.DuplicatedNestedProperty);
                }
            });
        }
    }
}