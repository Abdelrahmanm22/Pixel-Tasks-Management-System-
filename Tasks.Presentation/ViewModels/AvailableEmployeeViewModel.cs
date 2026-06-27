namespace Tasks.Presentation.ViewModels
{
    public class AvailableEmployeeViewModel
    {
        public string Id         { get; set; } = string.Empty;
        public string FullName   { get; set; } = string.Empty;
        public string? Email     { get; set; }
        public bool IsSelected   { get; set; }
    }
}
