using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Administrator")]
  public class AdministratorController : CommonController
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Department(string subject)
    {
      ViewData["subject"] = subject;
      return View();
    }

    public IActionResult Course(string subject, string num)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      return View();
    }

    /*******Begin code to modify********/

    /// <summary>
    /// Returns a JSON array of all the courses in the given department.
    /// Each object in the array should have the following fields:
    /// "number" - The course number (as in 5530)
    /// "name" - The course name (as in "Database Systems")
    /// </summary>
    /// <param name="subject">The department subject abbreviation (as in "CS")</param>
    /// <returns>The JSON result</returns>
    public IActionResult GetCourses(string subject)
    {
            using (Team45LMSContext db = new Team45LMSContext())
            {
                //find all courses where the Subject matches and return their number and name
                var query =
                    from c in db.Courses
                    join d in db.Departments on c.Dept equals d.Subject
                    where d.Subject == subject
                    select new
                    {
                        number = c.Number,
                        name = c.Name
                    };
                return Json(query.ToArray());
            }
    }


    /// <summary>
    /// Returns a JSON array of all the professors working in a given department.
    /// Each object in the array should have the following fields:
    /// "lname" - The professor's last name
    /// "fname" - The professor's first name
    /// "uid" - The professor's uid
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <returns>The JSON result</returns>
    public IActionResult GetProfessors(string subject)
    {
            using (Team45LMSContext db = new Team45LMSContext())
            {
                //get all professors who work for the department with the given subject
                var query =
                    from p in db.Professors                   
                    where p.Subject == subject
                    select new
                    {
                        lname = p.LName,
                        fname = p.FName,
                        uid = p.UId
                    };
                //return the Json formatted result of the query
                return Json(query.ToArray());
            }
        }



    /// <summary>
    /// Creates a course.
    /// A course is uniquely identified by its number + the subject to which it belongs
    /// </summary>
    /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
    /// <param name="number">The course number</param>
    /// <param name="name">The course name</param>
    /// <returns>A JSON object containing {success = true/false},
	/// false if the Course already exists.</returns>
    public IActionResult CreateCourse(string subject, int number, string name)
    {
            //local variable to choose Json result
            bool Success = false;
            using (Team45LMSContext db = new Team45LMSContext())
            {
                //check if the course already exists
                var query =
                    from c in db.Courses
                    where c.Name == name && c.Number == number && c.Dept == subject
                    select c.CourseId;
                // if it doesn't exist add it to Courses
                if(query.Count()==0)
                {
                    Success = true;
                    //Create a new Course with the given Dept, Number, and Name
                    Courses newCourse = new Courses();
                    newCourse.Dept = subject;
                    newCourse.Number = (uint)number;
                    newCourse.Name = name;

                    // Adds the new Course object to the Courses table
                    db.Courses.Add(newCourse);
                    db.SaveChanges();
                }
            }
            //choose Json result
            if(Success == false)
            { 
                return Json(new { success = false });
            }
            else
            {
                return Json(new { success = true });
            }
    }



    /// <summary>
    /// Creates a class offering of a given course.
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <param name="number">The course number</param>
    /// <param name="season">The season part of the semester</param>
    /// <param name="year">The year part of the semester</param>
    /// <param name="start">The start time</param>
    /// <param name="end">The end time</param>
    /// <param name="location">The location</param>
    /// <param name="instructor">The uid of the professor</param>
    /// <returns>A JSON object containing {success = true/false}. 
    /// false if another class occupies the same location during any time 
    /// within the start-end range in the same semester, or if there is already
    /// a Class offering of the same Course in the same Semester.</returns>
    public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
    {
            //local variable to choose Json result
            bool Success = false;
            using (Team45LMSContext db = new Team45LMSContext())
            {
                //check if the class already exists with the above conditions
                var q1 =
                    from c in db.Classes
                    join d in db.Courses on c.CourseId equals d.CourseId
                    where d.Dept == subject && d.Number == number && c.Season == season && c.Year == year 
                    select c.CourseId;
                var q2 =
                    from c in db.Classes
                    where c.Location == location && ((Convert.ToDateTime(c.Start) >= start && Convert.ToDateTime(c.Start) <= end) || (Convert.ToDateTime(c.End) <= end && Convert.ToDateTime(c.End) >= end))
                    select c.CourseId;
                //get the course id
                var q3 =
                    from d in db.Courses
                    where d.Dept == subject && d.Number == number
                    select d.CourseId;
                // if it doesn't exist add it to Classes
                if (q1.Count() == 0 && q2.Count() ==0)
                {
                    Success = true;
                    //Create a new Class with the found CourseId and the given Season, Year, Start, End, Location and Professor
                    Classes newClass = new Classes();
                    newClass.CourseId = q3.ToArray()[0];
                    newClass.Season= season;
                    newClass.Year = (uint)year;
                    newClass.Start = start.TimeOfDay;
                    newClass.End = end.TimeOfDay;
                    newClass.Location = location;
                    newClass.Professor = instructor;

                    // Adds the new Class object to the Classes table
                    db.Classes.Add(newClass);
                    db.SaveChanges();
                }
            }
            //choose Json result
            if (Success == false)
            {
                return Json(new { success = false });
            }
            else
            {
                return Json(new { success = true });
            }
        }


    /*******End code to modify********/

  }
}