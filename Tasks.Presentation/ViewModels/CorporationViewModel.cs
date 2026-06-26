using System.ComponentModel.DataAnnotations;

namespace Tasks.Presentation.ViewModels
{
    public class CorporationViewModel
    {
        public int Id { get; set; }
        
        public string Code { get; set; }
        [Required(ErrorMessage = "Name is Requred ")]
        [MaxLength(50, ErrorMessage = "Max Length is 50 chars")]
        [MinLength(2, ErrorMessage = "Min Length is 5 chars")]
        public string Name { get; set; }
        public string? NameAr { get; set; }
        [MaxLength(5000, ErrorMessage = "Max Length is 50 chars")]
        public string? Notes { get; set; }

    }
}
