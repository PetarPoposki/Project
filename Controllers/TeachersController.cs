#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Project.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Project.Areas.Identity.Data;

namespace Project.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ProjectContext _context;
        private UserManager<ProjectUser> _userManager;
        public TeachersController(ProjectContext context, UserManager<ProjectUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Roles = "Admin")]
        // GET: Teachers
        public async Task<IActionResult> Index(string FullName, string AcademicRank, string Degree)
        {
            IQueryable<Teacher> teachersQuery = _context.Teacher.AsQueryable();
            IQueryable<string> academicRanksQuery = _context.Teacher.OrderBy(m => m.AcademicRank).Select(m => m.AcademicRank).Distinct();
            IQueryable<string> degreesQuery = _context.Teacher.OrderBy(m => m.Degree).Select(m => m.Degree).Distinct();
            if (!string.IsNullOrEmpty(FullName))
            {
                if (FullName.Contains(" "))
                {
                    string[] names = FullName.Split(" ");
                    teachersQuery = teachersQuery.Where(x => x.FirstName.Contains(names[0]) || x.LastName.Contains(names[1]) ||
                    x.FirstName.Contains(names[1]) || x.LastName.Contains(names[0]));
                }
                else
                {
                    teachersQuery = teachersQuery.Where(x => x.FirstName.Contains(FullName) || x.LastName.Contains(FullName));
                }
            }
            if (!string.IsNullOrEmpty(AcademicRank))
            {
                teachersQuery = teachersQuery.Where(x => x.AcademicRank.Contains(AcademicRank));
            }
            if (!string.IsNullOrEmpty(Degree))
            {
                teachersQuery = teachersQuery.Where(x => x.Degree.Contains(Degree));
            }
            var TeacherFilter = new TeacherQuery
            {
                Teachers = await teachersQuery.ToListAsync(),
                AcademicRanks = new SelectList(await academicRanksQuery.ToListAsync()),
                Degrees = new SelectList(await degreesQuery.ToListAsync())
            };

            return View(TeacherFilter);
        }
        [Authorize(Roles = "Admin")]
        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            TeacherPicture viewmodel = new TeacherPicture
            {
                teacher = teacher,
                ProfilePictureName = teacher.ProfilePicture
            };

            return View(viewmodel);
        }

        // GET: Teachers/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                var User = new ProjectUser();
                User.Email = teacher.FirstName.ToLower() + "." + teacher.LastName.ToLower() + "@project.com";
                User.UserName = teacher.FirstName.ToLower() + "." + teacher.LastName.ToLower() + "@project.com";
                string userPWD = "Teacher123";
                IdentityResult chkUser = await _userManager.CreateAsync(User, userPWD);
                //Add default User to Role Admin
                if (chkUser.Succeeded) { var result1 = await _userManager.AddToRoleAsync(User, "Teacher"); }
                teacher.ProjectUserId = User.Id;
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
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
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teacher.FindAsync(id);
            IQueryable<Course> courses = _context.Course.AsQueryable();
            IQueryable<Course> courses1 = courses.Where(x => x.FirstTeacherId == teacher.Id);
            IQueryable<Course> courses2 = courses.Where(x => x.SecondTeacherId == teacher.Id);
            foreach (var course in courses1)
            {
                course.FirstTeacherId = null;
            }
            foreach (var course in courses2)
            {
                course.SecondTeacherId = null;
            }
            _context.Teacher.Remove(teacher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teacher.Any(e => e.Id == id);
        }
        // GET: Teachers/EditPicture/5
        public async Task<IActionResult> EditPicture(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = _context.Teacher.Where(x => x.Id == id).First();
            if (teacher == null)
            {
                return NotFound();
            }

            TeacherPicture viewmodel = new TeacherPicture
            {
                teacher = teacher,
                ProfilePictureName = teacher.ProfilePicture
            };

            return View(viewmodel);
        }
        // POST: Teachers/EditPicture/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPicture(long id, TeacherPicture viewmodel)
        {
            if (id != viewmodel.teacher.Id)
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
                        viewmodel.teacher.ProfilePicture = uniqueFileName;
                    }
                    else
                    {
                        viewmodel.teacher.ProfilePicture = viewmodel.ProfilePictureName;
                    }

                    _context.Update(viewmodel.teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(viewmodel.teacher.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = viewmodel.teacher.Id });
            }
            return View(viewmodel);
        }
        private string UploadedFile(TeacherPicture viewmodel)
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
