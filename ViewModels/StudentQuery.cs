using Project.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Project.ViewModels
{
    public class StudentQuery
    {
        public IList<Student> Students { get; set; }

        public string FullName { get; set; }

        public string StudentId { get; set; }
    }
}
