using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Student
    {
        public long Id { get; set; }
        [Required]
        [StringLength(10)]
        public string StudentId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Enrollment Date")]
        [DataType(DataType.Date)]
        public DateTime? EnrollmentDate { get; set; }
        public int? AcquiredCredits { get; set; }
        public int? CurrentSemester { get; set; }

        [StringLength(25)]
        public string? EducationLevel { get; set; }
        public string FullName
        {
            get { return String.Format("{0} {1}", FirstName, LastName); }
        }
        public string? ProfilePicture { get; set; }
        public ICollection<Enrollment>? Courses { get; set; }
    }
}
