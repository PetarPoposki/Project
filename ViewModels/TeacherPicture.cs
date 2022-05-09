using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels
{
    public class TeacherPicture
    {
        public Teacher? teacher { get; set; }

        [Display(Name = "Upload picture")]
        public IFormFile? ProfilePictureFile { get; set; }

        [Display(Name = "Picture name")]
        public string? ProfilePictureName { get; set; }
    }
}
