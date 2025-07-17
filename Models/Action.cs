namespace InfoTask.Models
{
    public class WorkflowAction
    {
        public required string  Id { get; set; }
        public required string  Name { get; set; }
        public bool Enabled { get; set; } = true;
        public List<string> FromStates { get; set; } = [];
        public required string  ToState { get; set; }
    }
}
