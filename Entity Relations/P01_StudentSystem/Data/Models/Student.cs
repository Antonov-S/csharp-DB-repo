using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace P01_StudentSystem.Data.Models
{
    public class Student
    {
        public Student()
        {
            this.HomeworkSubmissions = new HashSet<Homework>();
            this.CourseEnrollments = new HashSet<StudentCourse>();
        }
        public int StudentId { get; set; }

        [Required]
        [MaxLength(100)]
        // Сам по себе си стринга е уникод.
        public string Name { get; set; }

        [Column(TypeName = "char(10)")]
        public string PhoneNumber { get; set; }

        public DateTime RegisteredOn { get; set; }

        public DateTime? Birthday { get; set; }

        public ICollection<Homework> HomeworkSubmissions { get; set; }

        public ICollection<StudentCourse> CourseEnrollments { get; set; }
    }
}

//o StudentId
//o Name(up to 100 characters, unicode)
//o PhoneNumber(exactly 10 characters, not unicode, not required)
//o RegisteredOn
//o Birthday(not required)

