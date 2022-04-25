using Project.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Project.ViewModels
{
    public class TeacherQuery
    {
        public IList<Teacher> Teachers { get; set; }

        public SelectList AcademicRanks { get; set; }

        public SelectList Degrees { get; set; }

        public string FullName { get; set; }

        public string AcademicRank { get; set; }

        public string Degree { get; set; }
    }
}
