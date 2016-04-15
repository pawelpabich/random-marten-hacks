using System;
using Marten;
using MartenPlayground.Domain;
using Shouldly;

namespace MartenPlayground
{
    class UpdateDocument
    {
        public static void Run(DocumentStore store)
        {
            var document = MartenPlayground.StoreSingleDocumentInternalV1.Run(store);

            var newValue = Guid.NewGuid().ToString();
            MartenPlayground.Meassure.Run(() =>
            {
                using (var session = store.OpenSession())
                {
                    var copy = session.Load<Document>(document.Id);
                    copy.SetTopLevelProperty(newValue);
                    session.Store(copy);
                    session.SaveChanges();
                }
            });

            using (var session = store.OpenSession())
            {
                var copy = session.Load<Document>(document.Id);
                copy.TopLevelProperty.ShouldBe(newValue);
            }
        }
    }
}