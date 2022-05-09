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
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;

namespace Project.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly ProjectContext _context;

        public EnrollmentsController(ProjectContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var projectContext = _context.Enrollment.Include(e => e.Course).Include(e => e.Student);
            return View(await projectContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title");
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if(ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
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
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var enrollment = await _context.Enrollment.FindAsync(id);
            _context.Enrollment.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(long id)
        {
            return _context.Enrollment.Any(e => e.Id == id);
        }
        // GET: Enrollments/StudentsEnrolledAtCourse/5/MarijaStefanoska
        public async Task<IActionResult> StudentsEnrolledAtCourse(int? id, string teacher, int year)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .FirstOrDefaultAsync(m => m.Id == id);

            string[] names = teacher.Split(" ");
            var teacherModel = await _context.Teacher.FirstOrDefaultAsync(m => m.FirstName == names[0] && m.LastName == names[1]);
            ViewBag.teacher = teacher;
            ViewBag.course = course.Title;
            var enrollment = _context.Enrollment.Where(x => x.CourseId == id && (x.Course.FirstTeacherId == teacherModel.Id || x.Course.SecondTeacherId == teacherModel.Id))
            .Include(e => e.Course)
            .Include(e => e.Student);
            await _context.SaveChangesAsync();
            IQueryable<int?> yearsQuery = _context.Enrollment.OrderBy(m => m.Year).Select(m => m.Year).Distinct();
            IQueryable<Enrollment> enrollmentQuery = enrollment.AsQueryable();
            if (year != null && year != 0)
            {
                enrollmentQuery = enrollmentQuery.Where(x => x.Year == year);
            }
            else
            {
                enrollmentQuery = enrollmentQuery.Where(x => x.Year == yearsQuery.Max());
            }

            if (enrollment == null)
            {
                return NotFound();
            }

            EnrollmentQuery viewmodel = new EnrollmentQuery
            {
                Enrollments = await enrollmentQuery.ToListAsync(),
                YearsList = new SelectList(await yearsQuery.ToListAsync())
            };

            return View(viewmodel);
        }
        // GET: Enrollments/EditAsTeacher/5
        public async Task<IActionResult> EditAsTeacher(long? id, string teacher)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewBag.teacher = teacher;
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }
        // POST: Enrollments/EditAsTeacher/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsTeacher(long id, string teacher, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return NotFound();
            }
            string temp = teacher;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("StudentsEnrolledAtCourse", new { id = enrollment.CourseId, teacher = temp, year = enrollment.Year });
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "CourseId", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(enrollment);
        }
        // GET: Enrollments/StudentCourses/5
        public async Task<IActionResult> StudentCourses(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id);

            ViewBag.student = student.FullName;

            IQueryable<Enrollment> enrollment = _context.Enrollment.Where(x => x.StudentId == id)
            .Include(e => e.Course)
            .Include(e => e.Student);
            await _context.SaveChangesAsync();

            if (enrollment == null)
            {
                return NotFound();
            }

            return View(await enrollment.ToListAsync());
        }
        // GET: Enrollments/EditAsStudent/5
        public async Task<IActionResult> EditAsStudent(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = _context.Enrollment.Where(m => m.Id == id).Include(x => x.Student).Include(x => x.Course).First();
            IQueryable<Enrollment> enrollmentQuery = _context.Enrollment.AsQueryable();
            enrollmentQuery = enrollmentQuery.Where(m => m.Id == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            EditAsStudent viewmodel = new EditAsStudent
            {
                enrollment = await enrollmentQuery.Include(x => x.Student).Include(x => x.Course).FirstAsync(),
                SeminalUrlName = enrollment.SeminalUrl
            };
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Title", enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", enrollment.StudentId);
            return View(viewmodel);
        }
        // POST: Enrollments/EditAsStudent/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsStudent(long id, EditAsStudent viewmodel)
        {
            if (id != viewmodel.enrollment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    if (viewmodel.SeminalUrlFile != null)
                    {
                        string uniqueFileName = UploadedFile(viewmodel);
                        viewmodel.enrollment.SeminalUrl = uniqueFileName;
                    }
                    else
                    {
                        viewmodel.enrollment.SeminalUrl = viewmodel.SeminalUrlName;
                    }

                    _context.Update(viewmodel.enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(viewmodel.enrollment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("StudentCourses", new { id = viewmodel.enrollment.StudentId });
            }

            ViewData["CourseId"] = new SelectList(_context.Course, "CourseId", "Title", viewmodel.enrollment.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Student, "Id", "FirstName", viewmodel.enrollment.StudentId);
            return View(viewmodel);
        }
        private string UploadedFile(EditAsStudent viewmodel)
        {
            string uniqueFileName = null;

            if (viewmodel.SeminalUrlFile != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/seminals");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(viewmodel.SeminalUrlFile.FileName);
                string fileNameWithPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                {
                    viewmodel.SeminalUrlFile.CopyTo(stream);
                }
            }
            return uniqueFileName;
        }
    }
}
