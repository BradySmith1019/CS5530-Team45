using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : CommonController
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query =
                from s in db.Students
                join e in db.Enrolled on s.UId equals e.UId
                join cl in db.Classes on e.ClassId equals cl.ClassId
                join co in db.Courses on cl.CourseId equals co.CourseId
                join d in db.Departments on co.Dept equals d.Subject
                where s.UId == uid
                select new
                {
                    subject = d.Subject,
                    number = co.Number,
                    name = co.Name,
                    season = cl.Season,
                    year = cl.Year,
                    grade = e.Grade
                };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query =
                from d in db.Departments
                join co in db.Courses on d.Subject equals co.Dept
                join cl in db.Classes on co.CourseId equals cl.ClassId
                join e in db.Enrolled on cl.ClassId equals e.ClassId
                join st in db.Students on e.UId equals st.UId
                join su in db.Submissions.DefaultIfEmpty() on st.UId equals su.UId
                join a in db.Assignments on su.AssignId equals a.AssignId
                join ac in db.AssignmentCategories on a.CatId equals ac.CatId
                where d.Subject == subject && co.Number == num && cl.Season == season && cl.Year == year && st.UId == uid
                select new
                {
                    aname = a.Name,
                    cname = ac.Name,
                    due = a.DueDateTime,
                    score = su.Score
                };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// Does *not* automatically reject late submissions.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}.</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var query =
                from d in db.Departments
                join co in db.Courses on d.Subject equals co.Dept
                join cl in db.Classes on co.CourseId equals cl.ClassId
                join e in db.Enrolled on cl.ClassId equals e.ClassId
                join st in db.Students on e.UId equals st.UId
                join su in db.Submissions.DefaultIfEmpty() on st.UId equals su.UId
                join a in db.Assignments on su.AssignId equals a.AssignId
                join ac in db.AssignmentCategories on a.CatId equals ac.CatId
                where d.Subject == subject && co.Number == num && cl.Season == season && cl.Year == year && ac.Name == category && a.Name == asgname
                select new
                {
                    assignID = a.AssignId,
                    catID = ac.CatId,
                    classID = cl.ClassId,
                    courseID = co.CourseId,
                    subject = d.Subject,
                    time = su.Time
                    
                };

            foreach (var x in query)
            {
                if (x.time == null)
                {
                    Submissions s = new Submissions();
                    s.Score = 0;
                    s.Contents = contents;
                    s.Time = DateTime.Now;
                    s.UId = uid;
                    s.Subject = subject;
                    s.AssignId = x.assignID;
                    s.CatId = x.catID;
                    s.ClassId = x.classID;
                    s.CourseId = x.courseID;
                    s.Subject = x.subject;

                    db.Submissions.Add(s);

                    db.SaveChanges();

                    return Json(new { success = true });

                }

                else
                {
                    Submissions s = new Submissions();
                    s.Score = 0;
                    s.Contents = contents;
                    s.Time = DateTime.Now;
                    s.UId = uid;
                    s.Subject = subject;
                    s.AssignId = x.assignID;
                    s.CatId = x.catID;
                    s.ClassId = x.classID;
                    s.CourseId = x.courseID;
                    s.Subject = x.subject;

                    db.Submissions.Add(s);

                    db.SaveChanges();

                    return Json(new { success = true });
                }
                


            }
            
            return Json(new { success = false });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false},
        /// false if the student is already enrolled in the Class.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            bool success = false;
            var query =
                from d in db.Departments
                join co in db.Courses on d.Subject equals co.Dept
                join cl in db.Classes on co.CourseId equals cl.CourseId
                join e in db.Enrolled on cl.ClassId equals e.ClassId
                where d.Subject == subject && co.Number == num && cl.Season == season && cl.Year == year && e.UId == uid
                select e.UId;

            if (query.Count() != 0)
            {
                success = false;
            }

            else
            {
                var query2 =
                    from de in db.Departments
                    join cou in db.Courses on de.Subject equals cou.Dept
                    join cla in db.Classes on cou.CourseId equals cla.CourseId
                    where de.Subject == subject && cou.Number == num && cla.Season == season && cla.Year == year
                    select new
                    {
                        cID = cla.ClassId,
                        coID = cou.CourseId,
                    };

                if (query2.Count() != 0)
                {
                    foreach (var x in query2)
                    {
                        Enrolled e = new Enrolled();
                        e.UId = uid;
                        e.ClassId = x.cID;
                        e.CourseId = x.coID;
                        e.Subject = subject;
                        e.Grade = "--";

                        db.Enrolled.Add(e);

                        success = true;
                    }
                }

                else
                {
                    success = false;

                }


            }

            if (success)
            {
                db.SaveChanges();

                return Json(new { success = true });
            }

            else
            {
                return Json(new { success = false });
            }
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student does not have any grades, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            double GPAcounter = 0.0;
            int numClasses = 0;
            double gpa = 0.0;

            var query =
                from e in db.Enrolled
                where e.UId == uid
                select e.Grade;

            if (query.Count() == 0)
            {
                return Json(new { gpa });

            }
            foreach (var x in query)
            {
                numClasses++;
                if (x.Equals("--"))
                    continue;

                switch(x)
                {
                    case "A":
                        GPAcounter += 4.0;
                        break;
                    case "A-":
                        GPAcounter += 3.7;
                        break;
                    case "B+":
                        GPAcounter += 3.3;
                        break;
                    case "B":
                        GPAcounter += 3.0;
                        break;
                    case "B-":
                        GPAcounter += 2.7;
                        break;
                    case "C+":
                        GPAcounter += 2.3;
                        break;
                    case "C":
                        GPAcounter += 2.0;
                        break;
                    case "C-":
                        GPAcounter += 1.7;
                        break;
                    case "D+":
                        GPAcounter += 1.3;
                        break;
                    case "D":
                        GPAcounter += 1.0;
                        break;
                    case "D-":
                        GPAcounter += 0.7;
                        break;
                    case "E":
                        GPAcounter += 0.0;
                        break;
                }
            }

            gpa = GPAcounter / numClasses;
            return Json(new { gpa });
        }

        /*******End code to modify********/

    }
}