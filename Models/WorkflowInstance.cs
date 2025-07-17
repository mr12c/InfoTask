namespace InfoTask.Models
{
    public class WorkflowInstance
    {
        public required string Id { get; set; }
        public  required string DefinitionId { get; set; }
        public  required string CurrentStateId { get; set; }
        public List<WorkflowHistoryEntry> History { get; set; } = [];
    }

}
