
namespace InfoTask.Models
{
    public class DefinitionBuildRequest
    {
        public string Id { get; set; } = default!;
        public string Description { get; set; } = default!;
        public List<string> StateIds { get; set; } = new();
        public List<string> ActionIds { get; set; } = new();
    }
}
