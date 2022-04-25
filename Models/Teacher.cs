using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [StringLength(50)]
        public string? Degree { get; set; }
        [StringLength(25)]
        public string? AcademicRank { get; set; }
        [StringLength(10)]
        public string? OfficeNumber { get; set; }
        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; }
        public string FullName
        {
            get { return String.Format("{0} {1}", FirstName, LastName); }
        }
    }
}
