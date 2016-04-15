using Marten;
using MartenPlayground.Domain;

namespace MartenPlayground.Config
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