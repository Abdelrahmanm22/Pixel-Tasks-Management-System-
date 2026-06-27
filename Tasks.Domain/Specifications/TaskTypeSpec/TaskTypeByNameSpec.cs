using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.TaskTypeSpec
{
    public class TaskTypeByNameSpec : BaseSpecifications<TaskType>
    {
        public TaskTypeByNameSpec(string name)
            : base(t => t.Name.ToLower() == name.ToLower())
        { }
    }
}
