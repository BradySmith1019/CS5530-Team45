﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryWebServer;
using LibraryWebServer.Models;

namespace LibraryWebServer.Controllers
{
    public class HomeController : Controller
    {
        // WARNING:
        // This very simple web server is designed to be as tiny and simple as possible
        // This is NOT the way to save user data.
        // This will only allow one user of the web server at a time (aside from major security concerns).
        private static string user = "";
        private static int card = -1;

        /// <summary>
        /// Given a Patron name and CardNum, verify that they exist and match in the database.
        /// If the login is successful, sets the global variables "user" and "card"
        /// </summary>
        /// <param name="name">The Patron's name</param>
        /// <param name="cardnum">The Patron's card number</param>
        /// <returns>A JSON object with a single field: "success" with a boolean value:
        /// true if the login is accepted, false otherwise.
        /// </returns>
        [HttpPost]
        public IActionResult CheckLogin(string name, int cardnum)
        {
            bool loginSuccessful = false;
            using (Team45LibraryContext db = new Team45LibraryContext())
            {
                // Performs a query that checks if the given name and cardnum are in the Patrons table
                var query =
                    from p in db.Patrons
                    where p.Name == name &&
                    p.CardNum == cardnum
                    select p;

                // Checks if any rows were returned, indicating the given user is a Patron
                if (query.Any())
                {
                    loginSuccessful = true;
                }
            }

            if (!loginSuccessful)
            {
                return Json(new { success = false });
            }
            else
            {
                user = name;
                card = cardnum;
                return Json(new { success = true });
            }
        }


        /// <summary>
        /// Logs a user out. This is implemented for you.
        /// </summary>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult LogOut()
        {
            user = "";
            card = -1;
            return Json(new { success = true });
        }

        /// <summary>
        /// Returns a JSON array representing all known books.
        /// Each book should contain the following fields:
        /// {"isbn" (string), "title" (string), "author" (string), "serial" (uint?), "name" (string)}
        /// Every object in the list should have isbn, title, and author.
        /// Books that are not in the Library's inventory (such as Dune) should have a null serial.
        /// The "name" field is the name of the Patron who currently has the book checked out (if any)
        /// Books that are not checked out should have an empty string "" for name.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult AllTitles()
        {
            using (Team45LibraryContext db = new Team45LibraryContext())
            {
                var query =
                    from t in db.Titles
                    join i in db.Inventory on t.Isbn equals i.Isbn
                    into join1
                    from j1 in join1.DefaultIfEmpty()
                    join c in db.CheckedOut on j1.Serial equals c.Serial
                    into join2
                    from j2 in join2.DefaultIfEmpty()
                    join p in db.Patrons on j2.CardNum equals p.CardNum
                    into join3
                    from j3 in join3.DefaultIfEmpty()
                    select new
                    {
                        isbn = t.Isbn,
                        title = t.Title,
                        author = t.Author,
                        serial = j1 == null ? null : (uint?)j1.Serial,
                        name = j3 == null ? "" : j3.Name
                    };
                return Json(query.ToArray());
            }
        }

        /// <summary>
        /// Returns a JSON array representing all books checked out by the logged in user 
        /// The logged in user is tracked by the global variable "card".
        /// Every object in the array should contain the following fields:
        /// {"title" (string), "author" (string), "serial" (uint) (note this is not a nullable uint) }
        /// Every object in the list should have a valid (non-null) value for each field.
        /// </summary>
        /// <returns>The JSON representation of the books</returns>
        [HttpPost]
        public ActionResult ListMyBooks()
        {
            using (Team45LibraryContext db = new Team45LibraryContext())
            {
                var query =
                    from t in db.Titles
                    join i in db.Inventory on t.Isbn equals i.Isbn                    
                    join c in db.CheckedOut on i.Serial equals c.Serial                    
                    join p in db.Patrons on c.CardNum equals p.CardNum
                    where p.CardNum == card
                    select new
                    {
                        isbn = t.Isbn,
                        title = t.Title,
                        author = t.Author,
                        serial = i.Serial
                    };
                return Json(query.ToArray());
            }
        }


        /// <summary>
        /// Updates the database to represent that
        /// the given book is checked out by the logged in user (global variable "card").
        /// In other words, insert a row into the CheckedOut table.
        /// You can assume that the book is not currently checked out by anyone.
        /// </summary>
        /// <param name="serial">The serial number of the book to check out</param>
        /// <returns>success</returns>
        [HttpPost]
        public ActionResult CheckOutBook(int serial)
        {
            using (Team45LibraryContext db = new Team45LibraryContext())
            {
                // Create a new CheckedOut object and fill in the cardnum and serial with the given serial
                // and global cardnum
                CheckedOut newCheckOut = new CheckedOut();
                newCheckOut.CardNum = (uint)card;
                newCheckOut.Serial = (uint)serial;

                // Adds the new CheckedOut object to the CheckedOut table
                db.CheckedOut.Add(newCheckOut);
                db.SaveChanges();
            }

                return Json(new { success = true });
        }


        /// <summary>
        /// Returns a book currently checked out by the logged in user (global variable "card").
        /// In other words, removes a row from the CheckedOut table.
        /// You can assume the book is checked out by the user.
        /// </summary>
        /// <param name="serial">The serial number of the book to return</param>
        /// <returns>Success</returns>
        [HttpPost]
        public ActionResult ReturnBook(int serial)
        {
            using (Team45LibraryContext db = new Team45LibraryContext())
            {
                // Finds the row in the CheckedOut table corresponding to the global cardnum
                // and the given serial number
                var query = from c in db.CheckedOut
                            where c.CardNum == (uint)card &&
                            c.Serial == (uint)serial
                            select c;

                // Removes the returned row from the CheckedOut table
                db.CheckedOut.RemoveRange(query);
                db.SaveChanges();
            }

                return Json(new { success = true });
        }

        /*******************************************/
        /****** Do not modify below this line ******/
        /*******************************************/

        /// <summary>
        /// Return the home page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            if (user == "" && card == -1)
                return View("Login");

            return View();
        }

        /// <summary>
        /// Return the MyBooks page.
        /// </summary>
        /// <returns></returns>
        public IActionResult MyBooks()
        {
            if (user == "" && card == -1)
                return View("Login");

            return View();
        }

        /// <summary>
        /// Return the About page.
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        /// <summary>
        /// Return the Login page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Login()
        {
            user = "";
            card = -1;

            ViewData["Message"] = "Please login.";

            return View();
        }


        /// <summary>
        /// Return the Contact page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Return the Error page.
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

