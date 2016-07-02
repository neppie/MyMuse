using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyMuse.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<Models.Album> albums = helper.AlbumInit("xmlAlbum.xml");
            return View(albums);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        [Authorize]
        public ActionResult Chat()
        {
            ViewBag.Message = "Chat";
            TempData["ReqAuth"] = "Chat";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}