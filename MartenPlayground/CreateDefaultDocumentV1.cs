using System;
using MartenPlayground.Domain;

namespace MartenPlayground
{
    class CreateDefaultDocumentV1
    {
        public static Document Run(string topLevelProperty = null)
        {
            topLevelProperty = topLevelProperty ?? Guid.NewGuid().ToString();
            var child = new Child(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            return new Document(Guid.NewGuid(), topLevelProperty, Guid.NewGuid().ToString(), child);
        }
    }
}