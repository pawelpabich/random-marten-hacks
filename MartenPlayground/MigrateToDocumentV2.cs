using System;
using Marten;
using MartenPlayground.Domain.V2;
using Npgsql;
using Shouldly;

namespace MartenPlayground
{
    class MigrateToDocumentV2
    {
        public static void Run(DocumentStore storeV1, DocumentStore storeV2)
        {
            var documentId = StoreSingleDocumentInternalV1.Run(storeV1).Id;
            var nowAsUnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Meassure.Run(() =>
            {
                using (var session = storeV2.OpenSession())
                {
                    var documentV2 = session.Load<Document>(documentId);
                    documentV2.DateTimeAsUnixTime.ShouldBe(0);

                    var removeProperty = new NpgsqlCommand("Update mt_doc_document SET data = data - 'TopLevelProperty'", session.Connection);
                    removeProperty.ExecuteNonQuery();
                    
                    var addCommand = "Update mt_doc_document SET data = jsonb_set(data, '{DateTimeAsUnixTime}', '" + nowAsUnixTime + "', true)";
                    var addProperty = new NpgsqlCommand(addCommand, session.Connection);
                    addProperty.ExecuteNonQuery();

                    session.SaveChanges();
                }
            });

            using (var session = storeV2.OpenSession())
            {
                var documentV2 = session.Load<Document>(documentId);
                documentV2.DateTimeAsUnixTime.ShouldBe(nowAsUnixTime);
            }
        }
    }
}