using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels
{
    public class EditAsStudent
    {
        public Enrollment enrollment { get; set; }

        [Display(Name = "Seminal File")]
        public IFormFile? SeminalUrlFile { get; set; }

        public string? SeminalUrlName { get; set; }
    }
}