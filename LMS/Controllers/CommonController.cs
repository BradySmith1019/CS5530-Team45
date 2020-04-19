using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CommonController : Controller
    {

        /*******Begin code to modify********/

        protected Team45LMSContext db;

        public CommonController()
        {
            db = new Team45LMSContext();
        }


        /*
         * WARNING: This is the quick and easy way to make the controller
         *          use a different LibraryContext - good enough for our purposes.
         *          The "right" way is through Dependency Injection via the constructor 
         *          (look this up if interested).
        */

        public void UseLMSContext(Team45LMSContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            // Retrieves all the departments from the departments table and returns them in a JSON array
            var query =
                from d in db.Departments
                select new
                {
                    name = d.Name,
                    subject = d.Subject
                };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query =
                from d in db.Departments
                select new
                {
                    subject = d.Subject,
                    dname = d.Name,
                    courses = from c in db.Courses
                              where c.Dept == d.Subject
                              select new
                              {
                                  number = c.Number,
                                  cname = c.Name
                              }
                };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var query =
                from cl in db.Classes
                join co in db.Courses on cl.CourseId equals co.CourseId
                join d in db.Departments on co.Dept equals d.Subject
                join p in db.Professors on d.Subject equals p.Subject
                where d.Subject == subject && co.Number == number
                select new
                {
                    season = cl.Season,
                    year = cl.Year,
                    location = cl.Location,
                    start = cl.Start,
                    end = cl.End,
                    fname = p.FName,
                    lname = p.LName
                };
            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var query =
                from a in db.Assignments
                join ac in db.AssignmentCategories on a.CatId equals ac.CatId
                join c in db.Classes on ac.ClassId equals c.ClassId
                join d in db.Courses on c.CourseId equals d.CourseId
                where d.Dept == subject && d.Number == num && c.Season == season && c.Year == year && ac.Name == category && a.Name == asgname
                select a.Contents;
            return Content(query.ToString());
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var query =
                from st in db.Students
                join su in db.Submissions on st.UId equals su.UId
                join a in db.Assignments on su.AssignId equals a.AssignId
                join ac in db.AssignmentCategories on a.CatId equals ac.CatId
                join cl in db.Classes on ac.ClassId equals cl.ClassId
                join co in db.Courses on cl.CourseId equals co.CourseId
                join d in db.Departments on co.Dept equals d.Subject
                where d.Subject == subject && co.Number == num && cl.Season == season && cl.Year == year && ac.Name == category && a.Name == asgname && st.UId == uid
                select su.Contents;

            if (!query.Any())
            {
                return Content("");
            }
            else
            {
                return Content(query.ToString());

            }
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var studQuery =
                from s in db.Students
                join d in db.Departments on s.Subject equals d.Subject
                where s.UId == uid
                select new
                {
                    fname = s.FName,
                    lname = s.LName,
                    uid = s.UId,
                    department = d.Name
                };

            if (!studQuery.Any())
            {
                var profQuery =
                from p in db.Professors
                join d in db.Departments on p.Subject equals d.Subject
                where p.UId == uid
                select new
                {
                    fname = p.FName,
                    lname = p.LName,
                    uid = p.UId,
                    department = d.Name
                };

                if (!profQuery.Any())
                {
                    var adminQuery =
                        from a in db.Administrators
                        where a.UId == uid
                        select new
                        {
                            fname = a.FName,
                            lname = a.LName,
                            uid = a.UId
                        };

                    if (!adminQuery.Any())
                    {
                        return Json(new { success = false });
                    }

                    else
                    {
                        return Json(new { adminQuery.First().fname, adminQuery.First().lname, adminQuery.First().uid });
                    }
                }

                else
                {
                    return Json(new { profQuery.First().fname, profQuery.First().lname, profQuery.First().uid, profQuery.First().department });
                }
            }

            else
            {
                return Json(new { studQuery.First().fname, studQuery.First().lname, studQuery.First().uid, studQuery.First().department });
            }

            

        }


        /*******End code to modify********/

    }
}