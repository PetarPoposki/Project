#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Project.ViewModels;

namespace Project.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ProjectContext _context;

        public StudentsController(ProjectContext context)
        {
            _context = context;
        }

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

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemester,EducationLevel")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemester,EducationLevel")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
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
            return View(student);
        }

        // GET: Students/Delete/5
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
        // GET: Students/StudentsFilter/5
        public async Task<IActionResult> StudentsFind(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .FirstOrDefaultAsync(m => m.Id == id);
            IQueryable<Student> studentsQuery = _context.Enrollment.Where(x => x.Id == id).Select(x => x.Student);
            await _context.SaveChangesAsync();
            
            if (course == null)
            {
                return NotFound();
            }
            var StudentFilter = new StudentQuery
            {
                Students = await studentsQuery.ToListAsync(),
            };

            return View(StudentFilter);
        }
        /*  public async Task<IActionResult> StudentsEnrolled(int? id)
          {
              if (id == null)
              {
                  return NotFound();
              }

              var course = await _context.Course
                  .FirstOrDefaultAsync(m => m.Id == id);
              IQueryable<Student> studentQuery = _context.Enrollment.Where(x => x.Id == id).Select(x => x.Student);
              await _context.SaveChangesAsync();
              if (course == null)
              {
                  return NotFound();
              }
              var studentFilterVM = new StudentQuery
              {
                  Students = await studentQuery.ToListAsync(),
              };

              return View(studentFilterVM);
         }*/
    }
}
