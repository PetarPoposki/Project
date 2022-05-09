using Microsoft.AspNetCore.Mvc.Rendering;
using Project.Models;

namespace Project.ViewModels
{
    public class EnrollStudentsViaCourse
    {
        public Course course { get; set; }

        public IEnumerable<long>? SelectedStudents { get; set; }

        public IEnumerable<SelectListItem>? StudentsEnrolledList { get; set; }

        public int? Year { get; set; }
    }
}
