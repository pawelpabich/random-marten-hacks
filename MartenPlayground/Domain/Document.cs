using System;

namespace MartenPlayground.Domain
{
    public class Document
    {
        public Document(Guid id, string topLevelProperty, string duplicatedProperty, Child child)
        {
            Id = id;
            TopLevelProperty = topLevelProperty;
            DuplicatedProperty = duplicatedProperty;
            Child = child;
        }

        public Guid Id { get; private set; }
        public string TopLevelProperty { get; private set; }
        public string DuplicatedProperty { get; private set; }
        public Child Child { get; private set; }
    }
}