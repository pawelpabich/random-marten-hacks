namespace MartenPlayground.Domain
{
    public class Child
    {
        public Child(string nestedProperty)
        {
            NestedProperty = nestedProperty;
        }

        public string NestedProperty { get; private set; }
    }
}