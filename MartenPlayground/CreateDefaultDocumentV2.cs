using System;
using MartenPlayground.Domain;
using Document = MartenPlayground.Domain.V2.Document;

namespace MartenPlayground
{
    class CreateDefaultDocumentV2
    {
        public static Document Run(DateTimeOffset? dateTime = null, string topLevelProperty = null)
        {           
            dateTime = dateTime ?? DateTimeOffset.UtcNow;
            topLevelProperty = topLevelProperty ?? Guid.NewGuid().ToString();

            var child = new Child(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            return new Document(Guid.NewGuid(), topLevelProperty, Guid.NewGuid().ToString(),
                dateTime.Value.ToUnixTimeMilliseconds(), child);
        }
    }
}