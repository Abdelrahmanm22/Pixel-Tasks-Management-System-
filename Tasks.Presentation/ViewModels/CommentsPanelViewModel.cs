namespace Tasks.Presentation.ViewModels
{
    // Model for the shared _CommentsChat partial (used by admin Details and employee Work views).
    public class CommentsPanelViewModel
    {
        public int WorkTaskId { get; set; }
        public int TaskAssignmentId { get; set; }
        public List<TaskCommentViewModel> Comments { get; set; } = new();

        // Admin-only: show assignee dropdown and all threads
        public bool IsAdminView { get; set; }
        public List<AssigneeDropdownItem> Assignees { get; set; } = new();
        public string ReturnUrl { get; set; } = string.Empty;
    }

    public class AssigneeDropdownItem
    {
        public int AssignmentId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
