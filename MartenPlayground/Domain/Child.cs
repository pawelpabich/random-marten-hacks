namespace MartenPlayground.Domain
{
    public class Child
    {
        public Child(string nestedProperty, string duplicatedNestedProperty)
        {
            NestedProperty = nestedProperty;
            DuplicatedNestedProperty = duplicatedNestedProperty;
        }

        public string NestedProperty { get; private set; }
        public string DuplicatedNestedProperty { get; private set; }
    }
}