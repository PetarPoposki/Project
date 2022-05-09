using Microsoft.AspNetCore.Mvc.Rendering;
using Project.Models;

namespace Project.ViewModels
{
    public class EnrollmentQuery
    {
        public IList<Enrollment> Enrollments { get; set; }

        public SelectList YearsList { get; set; }
        public int Year { get; set; }
    }
}