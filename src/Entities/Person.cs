namespace ChatAgentic.Entities
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public List<PersonMetadataItem> Metadata { get; set; } = default!;

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = default!;

        public List<Contact> Contacts { get; set; } = [];
    }

    public class PersonMetadataItem
    {
        public string Name { get; set; } = default!;
        public string Value { get; set; } = default!;
    }
}