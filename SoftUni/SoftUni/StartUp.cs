using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var softUniContext = new SoftUniContext();
            var result = AddNewAddressToEmployee(softUniContext);

            Console.WriteLine(result);

        }

        public static string RemoveTown(SoftUniContext context)
        {
            var allEmployees = context.Employees.ToList();

            allEmployees.ForEach(x => x.AddressId = null);

            var town = context.Towns.First(x => x.Name == "Seattle");

            var addresses = context.Addresses.Where(x => x.Town == town).ToList();

            context.Addresses.RemoveRange(addresses);

            context.Towns.Remove(town);

            context.SaveChanges();

            return $"{addresses.Count} addresses in Seattle were deleted";
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var employeeProject = context.EmployeesProjects.Where(x => x.ProjectId == 2).ToList();

            context.EmployeesProjects.RemoveRange(employeeProject);

            var project = context.Projects.Find(2);

            context.Projects.Remove(project);

            context.SaveChanges();

            StringBuilder sb = new StringBuilder();

            foreach (var p in context.Projects.Take(10))
            {
                sb.AppendLine(p.Name);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projectsInfo = context.Projects
                .OrderByDescending(x => x.StartDate)
                .Take(10)
                .Select(x => new
                {
                    x.Name,
                    x.Description,
                    StartDate = x.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                })
                .ToList()
                .OrderBy(x => x.Name);

            StringBuilder sb = new StringBuilder();

            foreach (var projectInfo in projectsInfo)
            {
                sb.AppendLine(projectInfo.Name);
                sb.AppendLine(projectInfo.Description);
                sb.AppendLine(projectInfo.StartDate);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var emploees = context.Employees
                .Where(x => x.FirstName.StartsWith("Sa"))
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.JobTitle,
                    x.Salary
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var emploee in emploees)
            {
                sb.AppendLine($"{emploee.FirstName} {emploee.LastName} - {emploee.JobTitle} - (${emploee.Salary:F2})");
            }

            return sb.ToString().TrimEnd();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            var departments = new string[]
            {
                "Engineering", "Tool Design", "Marketing", "Information Services"
            };

            var emploees = context.Employees
                .Where(x => departments.Contains(x.Department.Name))
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            foreach (var emploee in emploees)
            {
                emploee.Salary *= 1.12m;
            }

            context.SaveChanges();

            StringBuilder sb = new StringBuilder();

            foreach (var emploee in emploees)
            {
                sb.AppendLine($"{emploee.FirstName} {emploee.LastName} (${emploee.Salary:F2})");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
                .Where(x => x.Employees.Count > 5)
                .OrderBy(x => x.Employees.Count)
                .ThenBy(x => x.Name)
                .Select(x => new
                {
                    x.Name,
                    x.Manager.FirstName,
                    x.Manager.LastName,
                    Emploees = x.Employees.Select(e => new 
                    {
                        e.FirstName,
                        e.LastName,
                        e.JobTitle
                    })
                    .OrderBy(x => x.FirstName)
                    .ThenBy(x => x.LastName)
                    .ToList()
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var department in departments)
            {
                sb.AppendLine($"{department.Name} – {department.FirstName} {department.LastName}");

                foreach (var empInDep in department.Emploees)
                 {
                    sb.AppendLine($"{empInDep.FirstName} {empInDep.LastName} - {empInDep.JobTitle}");
                }
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .Select(x => new
                {
                    x.AddressText,
                    x.Town.Name,
                    x.Employees.Count
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .ThenBy(x => x.AddressText)
                .Take(10)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var address in addresses)
            {
                sb.AppendLine($"{address.AddressText}, {address.Name} - {address.Count} employees");
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var employee147 = context.Employees
                .Select(x => new Employee
                {
                    EmployeeId = x.EmployeeId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    JobTitle = x.JobTitle,
                    EmployeesProjects = x.EmployeesProjects.Select(p => new EmployeeProject
                    {
                        Project = p.Project
                    })
                    .OrderBy(x => x.Project.Name)
                    .ToList()

                })
                .FirstOrDefault(x => x.EmployeeId == 147);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{employee147.FirstName} {employee147.LastName} - {employee147.JobTitle}");

            foreach (var project in employee147.EmployeesProjects)
            {
                sb.AppendLine(project.Project.Name);
            }
            
            return sb.ToString().TrimEnd();
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            //Find the first 10 employees who have projects started in the period 2001 - 2003 (inclusive). 

            var emploees = context.Employees
                .Include(x => x.EmployeesProjects)
                .ThenInclude(x => x.Project)
                .Where(x => x.EmployeesProjects.Any(p => p.Project.StartDate.Year >= 2001 && p.Project.StartDate.Year <= 2003))
                .Select(x => new
                {
                    EmploeeFirstName = x.FirstName,
                    EmployeeLastName  = x.LastName,
                    MenagerFirstName = x.Manager.FirstName,
                    MenagerLastName = x.Manager.LastName,
                    Projects = x.EmployeesProjects
                    .Select( p=> new { 
                    ProjectName = p.Project.Name,
                    StartDate = p.Project.StartDate,
                    EndDate = p.Project.EndDate
                    })
                })
                .Take(10)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var emp in emploees)
            {
                sb.AppendLine($"{emp.EmploeeFirstName} {emp.EmployeeLastName} - Manager: {emp.MenagerFirstName} {emp.MenagerLastName}");

                foreach (var project in emp.Projects)
                {
                    var endDate = project.EndDate.HasValue
                        ? project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                        : "not finished";
                    sb.AppendLine($"--{project.ProjectName} - {project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)} - {endDate}");
                    
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            Address newAddress = new Address
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            context.Addresses.Add(newAddress);

            var employeeNakov = context.Employees.FirstOrDefault(x => x.LastName == "Nakov");

            employeeNakov.Address = newAddress;

            context.SaveChanges();

            var allEmployeesInfo = context.Employees.OrderByDescending(x => x.AddressId).Take(10).Select(x => x.Address.AddressText);

            StringBuilder sb = new StringBuilder();

            foreach (var employeeInfo in allEmployeesInfo)
            {
                sb.AppendLine(employeeInfo);
            }

            return sb.ToString().TrimEnd();

        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(x => new
                {
                    x.EmployeeId,
                    x.FirstName,
                    x.LastName,
                    x.MiddleName,
                    x.JobTitle,
                    x.Salary
                })
                .OrderBy(x => x.EmployeeId)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:F2}");
            }

            var result = sb.ToString().TrimEnd();
            return result;
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.Salary > 50000)
                .Select(x => new
                {
                    x.FirstName,
                    x.Salary
                })
                .OrderBy(x => x.FirstName)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} - {employee.Salary:F2}");
            }

            var result = sb.ToString().TrimEnd();
            return result;
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.Department.Name == "Research and Development")
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    DepartmentName = x.Department.Name,
                    x.Salary,
                })
                .OrderBy(x => x.Salary)
                .ThenByDescending(x => x.FirstName)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} from {employee.DepartmentName} - ${employee.Salary:F2}");
            }

            var result = sb.ToString().TrimEnd();
            return result;
        }
    }


}
