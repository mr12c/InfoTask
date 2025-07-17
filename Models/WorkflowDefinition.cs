namespace InfoTask.Models
{
    public class WorkflowDefinition
    {
        public required string Id { get; set; }
        public required string Description { get; set; }
        public List<State> States { get; set; } = [];
        public List<WorkflowAction> Actions { get; set; } = [];
    }
}
