using System;

namespace MartenPlayground.Domain.V2
{
    public class Document
    {
        public Document(Guid id, string topLevelPropertyV2, string duplicatedProperty, Child child)
        {
            Id = id;
            TopLevelPropertyV2 = topLevelPropertyV2;
            DuplicatedProperty = duplicatedProperty;
            Child = child;
        }

        public Guid Id { get; private set; }
        public string TopLevelPropertyV2 { get; private set; }
        public string DuplicatedProperty { get; private set; }
        public Child Child { get; private set; }

        public void SetTopLevelPropertyV2(string newValue)
        {
            TopLevelPropertyV2 = newValue;
        }
    }
}