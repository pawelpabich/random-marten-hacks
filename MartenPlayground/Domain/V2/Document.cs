using System;

namespace MartenPlayground.Domain.V2
{
    public class Document
    {
        public Document(Guid id, string topLevelProperty, string duplicatedProperty, long dateTimeAsUnixTime, Child child)
        {
            Id = id;
            TopLevelProperty = topLevelProperty;
            DuplicatedProperty = duplicatedProperty;
            Child = child;
            DateTimeAsUnixTime = dateTimeAsUnixTime;
        }

        public Guid Id { get; private set; }
        public string TopLevelProperty { get; private set; }
        public string DuplicatedProperty { get; private set; }
        public long DateTimeAsUnixTime { get; private set; }


        public Child Child { get; private set; }
        
    }
}