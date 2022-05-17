using Microsoft.EntityFrameworkCore;
using Project.Data;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.AspNetCore.Identity;
using Project.Areas.Identity.Data;

namespace Project.Models
{
    public class SeedData
    {
        public static async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ProjectUser>>();
            IdentityResult roleResult;
            //Add Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("Admin");
            if (!roleCheck) { roleResult = await RoleManager.CreateAsync(new IdentityRole("Admin")); }
            ProjectUser user = await UserManager.FindByEmailAsync("admin@project.com");
            if (user == null)
            {
                var User = new ProjectUser();
                User.Email = "admin@project.com";
                User.UserName = "admin@project.com";
                string userPWD = "Admin123";
                IdentityResult chkUser = await UserManager.CreateAsync(User, userPWD);
                //Add default User to Role Admin
                if (chkUser.Succeeded) { var result1 = await UserManager.AddToRoleAsync(User, "Admin"); }
            }
            //Add Teacher Role
            roleCheck = await RoleManager.RoleExistsAsync("Teacher");
            if (!roleCheck) { roleResult = await RoleManager.CreateAsync(new IdentityRole("Teacher")); }
            user = await UserManager.FindByEmailAsync("teacher@project.com");
            if (user == null)
            {
                var User = new ProjectUser();
                User.Email = "teacher@project.com";
                User.UserName = "teacher@project.com";
                string userPWD = "Teacher123";
                IdentityResult chkUser = await UserManager.CreateAsync(User, userPWD);
                //Add default User to Role Teacher
                if (chkUser.Succeeded) { var result1 = await UserManager.AddToRoleAsync(User, "Teacher"); }
            }
            //Add Student Role
            roleCheck = await RoleManager.RoleExistsAsync("Student");
            if (!roleCheck) { roleResult = await RoleManager.CreateAsync(new IdentityRole("Student")); }
            user = await UserManager.FindByEmailAsync("student@project.com");
            if (user == null)
            {
                var User = new ProjectUser();
                User.Email = "student@project.com";
                User.UserName = "student@project.com";
                string userPWD = "Student123";
                IdentityResult chkUser = await UserManager.CreateAsync(User, userPWD);
                //Add default User to Role Student
                if (chkUser.Succeeded) { var result1 = await UserManager.AddToRoleAsync(User, "Student"); }
            }
        }


        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ProjectContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ProjectContext>>()))
            {
                CreateUserRoles(serviceProvider).Wait();
                // Look for any movies.
                if (context.Course.Any() || context.Teacher.Any() || context.Student.Any())
                {
                    return;   // DB has been seeded
                }

                context.Teacher.AddRange(
                    new Teacher { /*Id = 1, */FirstName = "Rob", LastName = "Reiner", Degree = "Electrical Engineering", AcademicRank = "Bachelor", OfficeNumber = "A13", HireDate = DateTime.Parse("2003-3-6") },
                    new Teacher { /*Id = 2, */FirstName = "Ivan", LastName = "Reitman", ProjectUserId = context.Users.Single(x => x.Email == "teacher@project.com").Id },
                    new Teacher { /*Id = 3, */FirstName = "Howard", LastName = "Hawks" }
                );
                context.SaveChanges();

                context.Student.AddRange(
                    new Student { /*Id = 1, */StudentId = "ABC", FirstName = "Billy", LastName = "Crystal" },
                    new Student { /*Id = 2, */StudentId = "DEF", FirstName = "Meg", LastName = "Ryan" },
                    new Student { /*Id = 3, */StudentId = "GHI", FirstName = "Carrie", LastName = "Fisher" },
                    new Student { /*Id = 4, */StudentId = "HJK", FirstName = "Bill", LastName = "Murray" },
                    new Student { /*Id = 5, */StudentId = "LFS", FirstName = "Dan", LastName = "Aykroyd", ProjectUserId= context.Users.Single(x => x.Email == "student@project.com").Id },
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
