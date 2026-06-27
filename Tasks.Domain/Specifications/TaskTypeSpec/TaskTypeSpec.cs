using Tasks.Domain.Models;

namespace Tasks.Domain.Specifications.TaskTypeSpec
{
    public class TaskTypeSpec : BaseSpecifications<TaskType>
    {
        public TaskTypeSpec() : base()
        { }

        public TaskTypeSpec(int id) : base(t => t.Id == id)
        { }
    }
}
