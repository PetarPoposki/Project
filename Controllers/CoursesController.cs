#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Project.ViewModels;

namespace Project.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ProjectContext _context;

        public CoursesController(ProjectContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        // GET: Courses
        public async Task<IActionResult> Index(string title, int semester, string programme)
        {
            IQueryable<Course> coursesQuery = _context.Course.AsQueryable();
            IQueryable<int> semestersQuery = _context.Course.OrderBy(m => m.Semester).Select(m => m.Semester).Distinct();
            IQueryable<string> programmesQuery = _context.Course.OrderBy(m => m.Programme).Select(m => m.Programme).Distinct();
            if (!string.IsNullOrEmpty(title))
            {
                coursesQuery = coursesQuery.Where(x => x.Title.Contains(title));
            }
            if (semester != null && semester != 0)
            {
                coursesQuery = coursesQuery.Where(s => s.Semester == semester);
            }
            if (!string.IsNullOrEmpty(programme))
            {
                coursesQuery = coursesQuery.Where(p => p.Programme == programme);
            }
            var CoursefilterVM = new CourseQuery
            {
                Courses = await coursesQuery.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher).Include(c => c.Students).ThenInclude(c => c.Student).ToListAsync(),
                Programmes = new SelectList(await programmesQuery.ToListAsync()),
                Semesters = new SelectList(await semestersQuery.ToListAsync())
            };

            return View(CoursefilterVM);
        }
        [Authorize(Roles = "Admin")]
        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Teachers"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName");
            ViewData["Students"] = new SelectList(_context.Set<Student>(), "Id", "FullName");
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,FirstTeacher,SecondTeacherId,SecondTeacher,Students")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Teachers"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName");
            ViewData["Students"] = new SelectList(_context.Set<Student>(), "Id", "FullName");
            return View(course);
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = _context.Course.Where(m => m.Id == id).Include(x => x.Students).First();
            IQueryable<Course> coursesQuery = _context.Course.AsQueryable();
            coursesQuery = coursesQuery.Where(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            var students = _context.Student.AsEnumerable();
            students = students.OrderBy(s => s.FullName);

            EnrollStudentsViaCourse viewmodel = new EnrollStudentsViaCourse
            {
                course = await coursesQuery.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher).FirstAsync(),
                StudentsEnrolledList = new MultiSelectList(students, "Id", "FullName"),
                SelectedStudents = course.Students.Select(sa => sa.Id)
            };

            ViewData["Teachers"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName");
            //ViewData["Students"] = new SelectList(_context.Set<Student>(), "Id", "FullName", course.Students);
            return View(viewmodel);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnrollStudentsViaCourse viewmodel)
        {
            if (id != viewmodel.course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(viewmodel.course);
                    await _context.SaveChangesAsync();

                    var course = _context.Course.Where(m => m.Id == id).First();
                    string semester;
                    if (course.Semester % 2 == 0)
                    {
                        semester = "leten";
                    }
                    else
                    {
                        semester = "zimski";
                    }
                    IEnumerable<long> selectedStudents = viewmodel.SelectedStudents;
                    if (selectedStudents != null)
                    {
                        IQueryable<Enrollment> toBeRemoved = _context.Enrollment.Where(s => !selectedStudents.Contains(s.StudentId) && s.CourseId == id);
                        _context.Enrollment.RemoveRange(toBeRemoved);

                        IEnumerable<long> existEnrollments = _context.Enrollment.Where(s => selectedStudents.Contains(s.StudentId) && s.CourseId == id).Select(s => s.StudentId);
                        IEnumerable<long> newEnrollments = selectedStudents.Where(s => !existEnrollments.Contains(s));

                        foreach (int studentId in newEnrollments)
                            _context.Enrollment.Add(new Enrollment { StudentId = studentId, CourseId = id, Semester = semester, Year = viewmodel.Year });

                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        IQueryable<Enrollment> toBeRemoved = _context.Enrollment.Where(s => s.CourseId == id);
                        _context.Enrollment.RemoveRange(toBeRemoved);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(viewmodel.course.Id))
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

        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.FirstTeacher)
                .Include(c => c.SecondTeacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }
        // GET: Courses/CoursesFind/5
        public async Task<IActionResult> CoursesFind(int? id, string title, int semester, string programme)
        {
            if (id == null)
            {
                return NotFound();
            }
           
            var userLoggedInId = HttpContext.Session.GetString("UserLoggedIn");
            if (userLoggedInId != id.ToString() && userLoggedInId != "Admin")
            {
                return Forbid();
            }
            var teacher = await _context.Teacher
                .FirstOrDefaultAsync(m => m.Id == id);
            IQueryable<Course> coursesQuery = _context.Course.Where(m => m.FirstTeacherId == id || m.SecondTeacherId == id);
            await _context.SaveChangesAsync();
            ViewBag.Message = teacher.FullName;
            if (teacher == null)
            {
                return NotFound();
            }
            IQueryable<int> semestersQuery = _context.Course.OrderBy(m => m.Semester).Select(m => m.Semester).Distinct();
            IQueryable<string> programmesQuery = _context.Course.OrderBy(m => m.Programme).Select(m => m.Programme).Distinct();
            if (!string.IsNullOrEmpty(title))
            {
                coursesQuery = coursesQuery.Where(x => x.Title.Contains(title));
            }
            if (semester != null && semester != 0)
            {
                coursesQuery = coursesQuery.Where(s => s.Semester == semester);
            }
            if (!string.IsNullOrEmpty(programme))
            {
                coursesQuery = coursesQuery.Where(p => p.Programme == programme);
            }
            var CourseTitle = new CourseQuery
            {
                Courses = await coursesQuery.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher).ToListAsync(),
                Programmes = new SelectList(await programmesQuery.ToListAsync()),
                Semesters = new SelectList(await semestersQuery.ToListAsync())
            };

            return View(CourseTitle);
        }
    }
}
