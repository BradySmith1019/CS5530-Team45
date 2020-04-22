using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Professor")]
  public class ProfessorController : CommonController
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Students(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
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

    public IActionResult Categories(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
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

    public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }

    public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      ViewData["uid"] = uid;
      return View();
    }

    /*******Begin code to modify********/
    /// <summary>
    /// Returns a JSON array of all the students in a class.
    /// Each object in the array should have the following fields:
    /// "fname" - first name
    /// "lname" - last name
    /// "uid" - user ID
    /// "dob" - date of birth
    /// "grade" - the student's grade in this class
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
    {
        using (Team45LMSContext db = new Team45LMSContext())
        {
            //select the first name, last name, uid, dob, and grade of the students in the
            //specified class
            var query =
                from s in db.Students
                join e in db.Enrolled on s.UId equals e.UId
                join c in db.Classes on e.ClassId equals c.ClassId                
                join r in db.Courses on c.CourseId equals r.CourseId
                where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year
                select new
                {
                    fname = s.FName,
                    lname = s.LName,
                    uid= s.UId,
                    dob = s.Dob,
                    grade = e.Grade
                };
            //return the json of the result of the query
            return Json(query.ToArray());
        };
    }

    /// <summary>
    /// Returns a JSON array with all the assignments in an assignment category for a class.
    /// If the "category" parameter is null, return all assignments in the class.
    /// Each object in the array should have the following fields:
    /// "aname" - The assignment name
    /// "cname" - The assignment category name.
    /// "due" - The due DateTime
    /// "submissions" - The number of submissions to the assignment
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class, 
    /// or null to return assignments from all categories</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
    {
        using (Team45LMSContext db = new Team45LMSContext())
        {
            if(category != null)
            { 
                //select assignment name, category name, due date, and amount of submissions for a given assignment category in a given class
                var query =
                    from a in db.Assignments
                    join ac in db.AssignmentCategories on a.CatId equals ac.CatId
                    join c in db.Classes on ac.ClassId equals c.ClassId
                    join r in db.Courses on c.CourseId equals r.CourseId
                    where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year && ac.Name == category
                    select new
                    {
                        aname = a.Name,
                        cname = ac.Name,
                        due = a.DueDateTime,
                        submissions = a.Submissions.Count()
                    };
                //return the json version of the result of the query
                return Json(query.ToArray());
            }
            else
            {
                //select the assignment name, category name, due date and amount of submission for all assignments in a given class
                var query =
                        from a in db.Assignments
                        join ac in db.AssignmentCategories on a.CatId equals ac.CatId
                        join c in db.Classes on ac.ClassId equals c.ClassId
                        join r in db.Courses on c.CourseId equals r.CourseId
                        where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.DueDateTime,
                            submissions = a.Submissions.Count()
                        };
                //return the json version of the result of the query
                return Json(query.ToArray());
            }
        };
    }

    /// <summary>
    /// Returns a JSON array of the assignment categories for a certain class.
    /// Each object in the array should have the following fields:
    /// "name" - The category name
    /// "weight" - The category weight
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
    {
        using (Team45LMSContext db = new Team45LMSContext())
        {
            //select the name and weight of the assignment categories in the given class
            var query =
                from a in db.AssignmentCategories
                join c in db.Classes on a.ClassId equals c.ClassId
                join r in db.Courses on c.CourseId equals r.CourseId
                where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year
                select new
                {
                    name = a.Name,
                    weight = a.Weight
                };
            //return the json of the result of the query
            return Json(query.ToArray());
        };
    }

    /// <summary>
    /// Creates a new assignment category for the specified class.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The new category name</param>
    /// <param name="catweight">The new category weight</param>
    /// <returns>A JSON object containing {success = true/false},
    ///	false if an assignment category with the same name already exists in the same class.</returns>
    public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
    {
        //local variable to choose Json result
        bool Success = false;
        using (Team45LMSContext db = new Team45LMSContext())
        {
            //check if the Assignment Category already exists
            var query =
                from a in db.AssignmentCategories
                join c in db.Classes on a.ClassId equals c.ClassId
                join r in db.Courses on c.CourseId equals r.CourseId
                where a.Name == category && r.Dept == subject && r.Number == num && c.Season == season && c.Year == year
                select a.CatId;
            // if it doesn't exist add it to AssignmentCategories
            if (query.Count() == 0)
            {
                var query2 =
                from c in db.Classes
                join r in db.Courses on c.CourseId equals r.CourseId
                where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year
                select c.ClassId;
                    Success = true;
                //Create a new AssignmentCategory with the given Name and Weight
                AssignmentCategories newCat = new AssignmentCategories();
                newCat.Name = category;
                newCat.Weight = (uint)catweight;
                newCat.ClassId = query2.First();

                // Adds the new AssignmentCategory object to the AssignmentCategory table
                db.AssignmentCategories.Add(newCat);
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

    /// <summary>
    /// Creates a new assignment for the given class and category.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The new assignment name</param>
    /// <param name="asgpoints">The max point value for the new assignment</param>
    /// <param name="asgdue">The due DateTime for the new assignment</param>
    /// <param name="asgcontents">The contents of the new assignment</param>
    /// <returns>A JSON object containing success = true/false,
	/// false if an assignment with the same name already exists in the same assignment category.</returns>
    public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
    {
        //local variable to choose Json result
        bool Success = false;
        using (Team45LMSContext db = new Team45LMSContext())
        {
            //check if the Assignment already exists
            var query1 =
                from a in db.Assignments
                join ac in db.AssignmentCategories on a.CatId equals ac.CatId
                join c in db.Classes on ac.ClassId equals c.ClassId
                join r in db.Courses on c.CourseId equals r.CourseId
                where a.Name == asgname && r.Dept == subject && r.Number == num && c.Season == season && c.Year == year && ac.Name == category
                select a.AssignId;
            // if it doesn't exist add it to Assignments
            if (query1.Count() == 0)
            {
                var query2 =
                from ac in db.AssignmentCategories
                join c in db.Classes on ac.ClassId equals c.ClassId
                join r in db.Courses on c.CourseId equals r.CourseId
                where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year && ac.Name == category
                select ac.CatId;
                Success = true;
                //Create a new Assignments with the given Name, Max Points, Due Date and Contents
                Assignments newAsgn = new Assignments();
                newAsgn.Name = asgname;
                newAsgn.MaxPoints = (uint)asgpoints;
                newAsgn.DueDateTime = asgdue;
                newAsgn.Contents = asgcontents;
                newAsgn.CatId = query2.First();

                // Adds the new Assignments object to the Assignments table
                db.Assignments.Add(newAsgn);
                db.SaveChanges();

                    //Get students in class
                    var query3 = from s in db.Students
                                 join e in db.Enrolled on s.UId equals e.UId
                                 join c in db.Classes on e.ClassId equals c.ClassId
                                 join r in db.Courses on c.CourseId equals r.CourseId
                                 where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year
                                 select s.UId;
                    //Auto-update the grades of the students in the class
                    //Get classId
                    var query4 =
                        from c in db.Classes
                        join r in db.Courses on c.CourseId equals r.CourseId
                        where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year
                        select c.ClassId;
                    updateStudentsGrade(query3.ToArray(), query4.First(), category, asgname);
                   
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

        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
    public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
    {
        using (Team45LMSContext db = new Team45LMSContext())
        {
            //select the first name, last name, uid, submission time and score of the submissions in the given assignment
            var query =
                from s in db.Students
                join e in db.Submissions on s.UId equals e.UId
                join c in db.Classes on e.ClassId equals c.ClassId
                join r in db.Courses on c.CourseId equals r.CourseId
                join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                join a in db.Assignments on ac.CatId equals a.CatId
                where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year && ac.Name == category && a.Name == asgname
                select new
                {
                    fname = s.FName,
                    lname = s.LName,
                    uid = s.UId,
                    time = e.Time,
                    score = e.Score
                };
            //return the json of the result of the query
            return Json(query.ToArray());
        };
    }


    /// <summary>
    /// Set the score of an assignment submission
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <param name="uid">The uid of the student who's submission is being graded</param>
    /// <param name="score">The new score for the submission</param>
    /// <returns>A JSON object containing success = true/false</returns>
    public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
    {
            using (Team45LMSContext db = new Team45LMSContext())
            {
                //get the submission
                var query1 =
                    from s in db.Submissions
                    join p in db.Students on s.UId equals p.UId
                    join c in db.Classes on s.ClassId equals c.ClassId
                    join r in db.Courses on c.CourseId equals r.CourseId
                    join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                    join a in db.Assignments on ac.CatId equals a.CatId
                    where p.UId == uid && a.Name == asgname && ac.Name == category && r.Dept == subject && r.Number == num && c.Season == season && c.Year == year
                    select s;
                query1.First().Score = (uint)score;
                db.SaveChanges();
                //Get classId
                var query2 =
                    from c in db.Classes
                    join r in db.Courses on c.CourseId equals r.CourseId
                    where r.Dept == subject && r.Number == num && c.Season == season && c.Year == year
                    select c.ClassId;
                updateStudentsGrade(new string[] { uid }, query2.First(),category, asgname);
                
            }
                return Json(new { success = true });
        }


    /// <summary>
    /// Returns a JSON array of the classes taught by the specified professor
    /// Each object in the array should have the following fields:
    /// "subject" - The subject abbreviation of the class (such as "CS")
    /// "number" - The course number (such as 5530)
    /// "name" - The course name
    /// "season" - The season part of the semester in which the class is taught
    /// "year" - The year part of the semester in which the class is taught
    /// </summary>
    /// <param name="uid">The professor's uid</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {
        using (Team45LMSContext db = new Team45LMSContext())
        {
            //select the subject, number, name, season, and year of the classes taught by the given professor
            var query =
                from c in db.Classes                
                join r in db.Courses on c.CourseId equals r.CourseId
                where c.Professor == uid
                select new
                {
                    subject = r.Dept,
                    number = r.Number,
                    name = r.Name,
                    season = c.Season,
                    year = c.Year
                };
            //return the json of the result of the query
            return Json(query.ToArray());
        };
    }

    //helper methods
    /// <summary>
    /// Update the grades of a list of students in a specified class.
    /// This function will run when an assignment is created and when a student's submission is graded.
    /// </summary>
    /// <param name="uids">the string[] of student uids whose grade needs to be updated</param>
    /// <param name="subject">the subject of the class</param>
    /// <param name="num">the number of the class</param>
    /// <param name="season">the season of the class</param>
    /// <param name="year">the year of the class</param>
    /// <param name="category">the name of the assignment category</param>
    /// <param name="asgname">the name of the assignment</param>
    private void updateStudentsGrade(string[] uids, uint classId, string category, string asgname)
        {
            //get the enrollment rows of the given students
            var query1 = from e in db.Enrolled
                         join c in db.Classes on e.ClassId equals c.ClassId
                         join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                         join a in db.Assignments on ac.CatId equals a.CatId
                         where e.UId == Array.Find(uids, value => e.UId == value) && c.ClassId == classId && ac.Name == category && a.Name == asgname
                         select e;
            //get non-empty assignment categories for the given class
            var query3 = from ac in db.AssignmentCategories
                         join a in db.Assignments on ac.CatId equals a.CatId
                         where ac.ClassId == classId
                         select ac;
            double categoryPercent = 0;
            double categoryWeights = 0;
            foreach (AssignmentCategories ac in query3)
            {
                //get all assignments in the given category
                var query4 = from c in db.AssignmentCategories
                             join a in db.Assignments on c.CatId equals a.CatId
                             where ac.CatId == c.CatId
                             select a;
                double maxPoints = 0;
                double totalScore = 0;
                foreach (Assignments a in query4)
                {
                    //get score for the given assignment
                    var query5 = from an in db.Assignments
                                 join s in db.Submissions on an.AssignId equals s.AssignId
                                 where a.Name == an.Name
                                 select s.Score;
                    //add points earned if any to tally
                    if (query5.Count() != 0)
                    {
                        totalScore += query5.First();
                    }
                    //add max possible points to tally
                    maxPoints += a.MaxPoints;
                }

                //calculate score and weight for each assignment category
                try { 
                categoryPercent += (totalScore/maxPoints) * ac.Weight;
                }
                catch (DivideByZeroException e)
                {
                    categoryPercent += 0;
                }
                categoryWeights += ac.Weight;
            }
            //calculate grade percentage
            double totalPercent = categoryPercent * (100 / categoryWeights);
            string classGrade = convertPercentToLetter(totalPercent);

            foreach (Enrolled student in query1.ToArray())
            {
                student.Grade = classGrade;
            }
            db.SaveChanges();
        }
    
    /// <summary>
    /// A helper method to convert a percentage grade to a letter grade based on the CS 5530 syllabus
    /// </summary>
    /// <param name="totalPercent">The percentage grade to convert</param>
    /// <returns>The resulting letter grade</returns>
    private string convertPercentToLetter(double totalPercent)
        {
            if (totalPercent >= 93)
            {
                return "A";
            }
            else if (totalPercent >= 90)
            {
                return "A-";
            }
            else if (totalPercent >= 87)
            {
                return "B+";
            }
            else if (totalPercent >= 83)
            {
                return "B";
            }
            else if (totalPercent >= 80)
            {
                return "B-";
            }
            else if (totalPercent >= 77)
            {
                return "C+";
            }
            if (totalPercent >= 73)
            {
                return "C";
            }
            if (totalPercent >= 70)
            {
                return "C-";
            }
            if (totalPercent >= 67)
            {
                return "D+";
            }
            if (totalPercent >= 63)
            {
                return "D";
            }
            if (totalPercent >= 60)
            {
                return "D-";
            }
            else
            {
                return "E";
            }
        }


        /*******End code to modify********/

    }
}