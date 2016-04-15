using Marten;
using Marten.Schema;
using MartenPlayground.Config;
using MartenPlayground.Domain.V2;

namespace MartenPlayground
{
    class CreateStore
    {
        private const string ConnectionString = "host = localhost; database = marten; password = password; username = martenuser;Maximum Pool Size = 50;Minimum Pool Size = 50";

        public static DocumentStore V1()
        {
            return DocumentStore.For(config =>
            {
                config.Connection(ConnectionString);
                config.Schema.Include<CustomRegistry>();
                config.Schema.For<Domain.Document>().DocumentAlias("document");
                config.UpsertType = PostgresUpsertType.Standard;
            });
        }

        public static DocumentStore V2()
        {
            return DocumentStore.For(config =>
            {
                config.Connection(ConnectionString);
                config.Schema.Include<CustomRegistryV2>();
                config.Schema.For<Document>().DocumentAlias("document");
                config.UpsertType = PostgresUpsertType.Standard;
            });
        }
    }
}