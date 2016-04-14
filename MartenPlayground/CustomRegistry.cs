using Marten;
using MartenPlayground.Domain;

namespace MartenPlayground
{
    public class CustomRegistry : MartenRegistry
    {
        public CustomRegistry()
        {
            For<Document>().Searchable(x => x.DuplicatedProperty);
            For<Document>().Searchable(x => x.Child.DuplicatedNestedProperty);
        }
    }
}