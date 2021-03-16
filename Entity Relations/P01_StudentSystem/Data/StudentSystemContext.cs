using Microsoft.EntityFrameworkCore;
using P01_StudentSystem.Data.Models;

namespace P01_StudentSystem.Data
{
    public class StudentSystemContext : DbContext
    {
        // create constructors
        // db sets
        // connect sql server
        // fluent API relations - many to many

        public StudentSystemContext()
        {

        }

        public StudentSystemContext(DbContextOptions options) 
            : base(options)
        {

        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Homework> HomeworkSubmissions { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Проверка дали е конфигурирано за да не го прави всеки път.            
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=T450\SQLEXPRESS;Database=StudentSystem;Integrated Security=True");
            }
            
            
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentCourse>(x =>
            {
                x.HasKey(x => new { x.CourseId, x.StudentId });
            });
            
            base.OnModelCreating(modelBuilder);
        }


    }
}
