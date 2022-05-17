#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Areas.Identity.Data;
using Project.Data;
using Project.Models;
using Project.ViewModels;

namespace Project.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ProjectContext _context;
        private UserManager<ProjectUser> _userManager;
        public StudentsController(ProjectContext context, UserManager<ProjectUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Roles = "Admin")]
        // GET: Students
        public async Task<IActionResult> Index(string FullName, string StudentId)
        {
            IQueryable<Student> studentsQuery = _context.Student.AsQueryable();
            if (!string.IsNullOrEmpty(FullName))
            {
                if (FullName.Contains(" "))
                {
                    string[] names = FullName.Split(" ");
                    studentsQuery = studentsQuery.Where(x => x.FirstName.Contains(names[0]) || x.LastName.Contains(names[1]) ||
                    x.FirstName.Contains(names[1]) || x.LastName.Contains(names[0])); ;
                }
                else
                {
                    studentsQuery = studentsQuery.Where(x => x.FirstName.Contains(FullName) || x.LastName.Contains(FullName));
                }
            }
            if (!string.IsNullOrEmpty(StudentId))
            {
                studentsQuery = studentsQuery.Where(x => x.StudentId.Contains(StudentId));
            }
            var StudentFilter = new StudentQuery
            {
                Students = await studentsQuery.ToListAsync()
            };

            return View(StudentFilter);
        }
        [Authorize(Roles = "Admin")]
        // GET: Students/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            StudentPicture viewmodel = new StudentPicture
            {
                student = student,
                ProfilePictureName = student.ProfilePicture
            };

            return View(viewmodel);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Courses"] = new SelectList(_context.Set<Course>(), "CourseId", "Title");
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemester,EducationLevel")] Student student)
        {
            if (ModelState.IsValid)
            {
                var User = new ProjectUser();
                User.Email = student.FirstName.ToLower() + "." + student.LastName.ToLower() + "@project.com";
                User.UserName = student.FirstName.ToLower() + "." + student.LastName.ToLower() + "@project.com";
                string userPWD = "Student123";
                IdentityResult chkUser = await _userManager.CreateAsync(User, userPWD);
                //Add default User to Role Admin
                if (chkUser.Succeeded) { var result1 = await _userManager.AddToRoleAsync(User, "Student"); }
                student.ProjectUserId = User.Id;
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = _context.Student.Where(x => x.Id == id).Include(x => x.Courses).First();
            if (student == null)
            {
                return NotFound();
            }

            var courses = _context.Course.AsEnumerable();
            courses = courses.OrderBy(s => s.Title);

            EnrollCoursesViaStudent viewmodel = new EnrollCoursesViaStudent
            {
                student = student,
                CoursesEnrolledList = new MultiSelectList(courses, "Id", "Title"),
                SelectedCourses = student.Courses.Select(x => x.CourseId)
            };

            ViewData["Courses"] = new SelectList(_context.Set<Course>(), "Id", "Title");
            return View(viewmodel);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, EnrollCoursesViaStudent viewmodel)
        {
            if (id != viewmodel.student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewmodel.student);
                    await _context.SaveChangesAsync();

                    var student = _context.Student.Where(x => x.Id == id).First();

                    IEnumerable<int> selectedCourses = viewmodel.SelectedCourses;
                    if (selectedCourses != null)
                    {
                        IQueryable<Enrollment> toBeRemoved = _context.Enrollment.Where(s => !selectedCourses.Contains(s.CourseId) && s.StudentId == id);
                        _context.Enrollment.RemoveRange(toBeRemoved);

                        IEnumerable<int> existEnrollments = _context.Enrollment.Where(s => selectedCourses.Contains(s.CourseId) && s.StudentId == id).Select(s => s.CourseId);
                        IEnumerable<int> newEnrollments = selectedCourses.Where(s => !existEnrollments.Contains(s));

                        foreach (int courseId in newEnrollments)
                            _context.Enrollment.Add(new Enrollment { StudentId = id, CourseId = courseId, Semester = viewmodel.Semester, Year = viewmodel.Year });

                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        IQueryable<Enrollment> toBeRemoved = _context.Enrollment.Where(s => s.StudentId == id);
                        _context.Enrollment.RemoveRange(toBeRemoved);
                        await _context.SaveChangesAsync();
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(viewmodel.student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewmodel);
        }

        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Student.FindAsync(id);
            _context.Student.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
        // GET: Students/StudentsFind/5
        public async Task<IActionResult> StudentsFind(int? id, string? fullName, string? studentId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .FirstOrDefaultAsync(m => m.Id == id);
            ViewBag.Message = course.Title;
            IQueryable<Student> studentsQuery = _context.Enrollment.Where(x => x.CourseId == id).Select(x => x.Student);
            await _context.SaveChangesAsync();
            if (course == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(fullName))
            {
                if (fullName.Contains(" "))
                {
                    string[] names = fullName.Split(" ");
                    studentsQuery = studentsQuery.Where(x => x.FirstName.Contains(names[0]) || x.LastName.Contains(names[1]) ||
                    x.FirstName.Contains(names[1]) || x.LastName.Contains(names[0])); ;
                }
                else
                {
                    studentsQuery = studentsQuery.Where(x => x.FirstName.Contains(fullName) || x.LastName.Contains(fullName));
                }
            }
            if (!string.IsNullOrEmpty(studentId))
            {
                studentsQuery = studentsQuery.Where(x => x.StudentId.Contains(studentId));
            }

            var studentFilterVM = new StudentQuery
            {
                Students = await studentsQuery.ToListAsync(),
            };

            return View(studentFilterVM);

        }
        // GET: Students/EditPicture/5
        public async Task<IActionResult> EditPicture(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = _context.Student.Where(x => x.Id == id).Include(x => x.Courses).First();
            if (student == null)
            {
                return NotFound();
            }

            var courses = _context.Course.AsEnumerable();
            courses = courses.OrderBy(s => s.Title);

            StudentPicture viewmodel = new StudentPicture
            {
                student = student,
                ProfilePictureName = student.ProfilePicture
            };

            return View(viewmodel);
        }
        // POST: Students/EditPicture/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPicture(long id, StudentPicture viewmodel)
        {
            if (id != viewmodel.student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (viewmodel.ProfilePictureFile != null)
                    {
                        string uniqueFileName = UploadedFile(viewmodel);
                        viewmodel.student.ProfilePicture = uniqueFileName;
                    }
                    else
                    {
                        viewmodel.student.ProfilePicture = viewmodel.ProfilePictureName;
                    }

                    _context.Update(viewmodel.student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(viewmodel.student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = viewmodel.student.Id });
            }
            return View(viewmodel);
        }
        private string UploadedFile(StudentPicture viewmodel)
        {
            string uniqueFileName = null;

            if (viewmodel.ProfilePictureFile != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profilePictures");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(viewmodel.ProfilePictureFile.FileName);
                string fileNameWithPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                {
                    viewmodel.ProfilePictureFile.CopyTo(stream);
                }
            }
            return uniqueFileName;
        }
    }
}
