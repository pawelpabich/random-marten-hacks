using System.Linq;
using Marten;
using Shouldly;

namespace MartenPlayground
{
    class QueryUsingRawSql
    {
        public static void Run(DocumentStore store)
        {
            var document = StoreSingleDocumentInternalV1.Run(store);
            Meassure.Run(() =>
            {
                using (var session = store.OpenSession())
                {
                    var result = session.Query<Domain.Document>($"where data->> 'TopLevelProperty' = '{document.TopLevelProperty}'").Single();
                    result.TopLevelProperty.ShouldBe(document.TopLevelProperty);
                }
            });
        }
    }
}