using Marten;

namespace MartenPlayground.Config
{
    public class CustomRegistryV2 : MartenRegistry
    {
        public CustomRegistryV2()
        {
            For<Domain.V2.Document>().Searchable(x => x.DuplicatedProperty);
            For<Domain.V2.Document>().Searchable(x => x.Child.DuplicatedNestedProperty);
        }
    }
}