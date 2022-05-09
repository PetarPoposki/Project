using Microsoft.AspNetCore.Mvc.Rendering;
using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels
{
    public class EnrollCoursesViaStudent
    {
        public Student student { get; set; }

        public IEnumerable<int>? SelectedCourses { get; set; }

        public IEnumerable<SelectListItem>? CoursesEnrolledList { get; set; }

        [Display(Name = "Year")]
        public int? Year { get; set; }

        [Display(Name = "Semester")]
        public string? Semester { get; set; }

        public string? ProfilePictureName { get; set; }
    }
}