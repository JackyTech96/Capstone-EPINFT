using Capstone.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Capstone.Controllers
{   
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private ModelDbContext db = new ModelDbContext();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
    }
}