#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Data
{
    public class ProjectContext : DbContext
    {
        public ProjectContext (DbContextOptions<ProjectContext> options)
            : base(options)
        {
        }

        public DbSet<Project.Models.Course> Course { get; set; }

        public DbSet<Project.Models.Student> Student { get; set; }

        public DbSet<Project.Models.Teacher> Teacher { get; set; }

        public DbSet<Project.Models.Enrollment> Enrollment { get; set; }
    }
}
