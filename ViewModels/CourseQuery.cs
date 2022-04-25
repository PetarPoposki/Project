using Project.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Project.ViewModels
{
    public class CourseQuery
    {
        public IList<Course> Courses { get; set; }

        public SelectList Programmes { get; set; }

        public SelectList Semesters { get; set; }

        public string Programme { get; set; }

        public string Title { get; set; }

        public int Semester { get; set; }
    }
}
