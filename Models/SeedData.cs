using Microsoft.EntityFrameworkCore;
using Project.Data;
namespace Project.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ProjectContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ProjectContext>>()))
            {
                // Look for any movies.
                if (context.Course.Any() || context.Teacher.Any() || context.Student.Any())
                {
                    return;   // DB has been seeded
                }

                context.Teacher.AddRange(
                    new Teacher { /*Id = 1, */FirstName = "Rob", LastName = "Reiner", Degree = "Electrical Engineering", AcademicRank = "Bachelor", OfficeNumber = "A13", HireDate = DateTime.Parse("2003-3-6") },
                    new Teacher { /*Id = 2, */FirstName = "Ivan", LastName = "Reitman" },
                    new Teacher { /*Id = 3, */FirstName = "Howard", LastName = "Hawks" }
                );
                context.SaveChanges();

                context.Student.AddRange(
                    new Student { /*Id = 1, */StudentId = "ABC", FirstName = "Billy", LastName = "Crystal" },
                    new Student { /*Id = 2, */StudentId = "DEF", FirstName = "Meg", LastName = "Ryan" },
                    new Student { /*Id = 3, */StudentId = "GHI", FirstName = "Carrie", LastName = "Fisher" },
                    new Student { /*Id = 4, */StudentId = "HJK", FirstName = "Bill", LastName = "Murray" },
                    new Student { /*Id = 5, */StudentId = "LFS", FirstName = "Dan", LastName = "Aykroyd" },
                    new Student { /*Id = 6, */StudentId = "RYS", FirstName = "Sigourney", LastName = "Weaver" },
                    new Student { /*Id = 7, */StudentId = "HKA", FirstName = "John", LastName = "Wayne" },
                    new Student { /*Id = 8, */StudentId = "DRR", FirstName = "Dean", LastName = "Martin" }
                );
                context.SaveChanges();

                context.Course.AddRange(
                    new Course
                    {
                        //Id = 1,
                        Title = "Math",
                        Credits = 6,
                        Semester = 4,
                        FirstTeacherId = 1,
                        SecondTeacherId = 2
                    },
                    new Course
                    {
                        //Id = 2,
                        Title = "Physics",
                        Credits = 6,
                        Semester = 3,
                        FirstTeacherId = 2,
                        SecondTeacherId = 1
                    },
                    new Course
                    {
                        //Id = 3,
                        Title = "English",
                        Credits = 5,
                        Semester = 2,
                        FirstTeacherId = 1,
                        SecondTeacherId = 3
                    },
                    new Course
                    {
                        //Id = 4,
                        Title = "Sports",
                        Credits = 3,
                        Semester = 1,
                        FirstTeacherId = 3,
                        SecondTeacherId = 2
                    }
                );
                context.SaveChanges();

                context.Enrollment.AddRange(
                    new Enrollment { StudentId = 1, CourseId = 1 },
                    new Enrollment { StudentId = 2, CourseId = 1 },
                    new Enrollment { StudentId = 3, CourseId = 1 },
                    new Enrollment { StudentId = 4, CourseId = 2 },
                    new Enrollment { StudentId = 5, CourseId = 2 },
                    new Enrollment { StudentId = 6, CourseId = 2 },
                    new Enrollment { StudentId = 4, CourseId = 3 },
                    new Enrollment { StudentId = 5, CourseId = 3 },
                    new Enrollment { StudentId = 6, CourseId = 3 },
                    new Enrollment { StudentId = 7, CourseId = 4 },
                    new Enrollment { StudentId = 8, CourseId = 4 }
                );
                context.SaveChanges();
            }
        }
    }
}
