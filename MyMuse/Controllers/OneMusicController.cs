using MyMuse.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
//using System.Threading;
//using System.Net;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Drive.v2;
//using Google.Apis.Auth.OAuth2.Mvc;
//using Google.Apis.Services;

namespace MyMuse.Controllers
{
    public class OneMusicController : Controller
    {
        private MyMuseContext db = new MyMuseContext();

        // MAIN VIEWS        
        public ActionResult Index()
        { // MP3 REFRESH    

            var APPD = Server.MapPath("~/App_Data/");
            string xmlAlbum = APPD + "xmlAlbum.xml";
     
                if (System.IO.File.Exists(xmlAlbum)) //&& Monitor.TryEnter(helper._dirlock))
                {
                    Task background = Task.Run(() =>
                    {
                        helper.Deserialize(xmlAlbum);
                    });                    
                }
                else
                {
                        Task background = Task.Run(() => helper.DirScrape(APPD));;
                }
                       
      //        ProgressHub.Disable();

                var goroundalbums = db.Albums
                    .GroupBy(a => a.AlbumGuid).Select(a => a.FirstOrDefault()).OrderBy(a => a.AlbumName);
                return View(goroundalbums.ToList());         
        }

            // SORTED BY ALBUM NAME
            public ActionResult idxAlbum(string xfilter, int? page)
        {
            ViewBag.Message = "Sorted by Album";
            ViewBag.Sort = "idxAlbum";

            int pageNumber = (page ?? 1);
            int pageSize = 16;

            var albums = db.Albums.GroupBy(a => a.AlbumGuid).Select(a => a.FirstOrDefault());
            if (!String.IsNullOrEmpty(xfilter))
                albums = albums.OrderBy(a => a.AlbumName).Where(a => a.AlbumName.ToUpper().Contains(xfilter.ToUpper()));
            else
                albums = albums.OrderBy(a => a.AlbumName);

            return View(albums.ToPagedList(pageNumber, pageSize));
        }

            // SORTED BY ARTIST
            public ActionResult idxArtist(string xfilter, int? page)
            {
                ViewBag.Message = "Sorted by Artist";
                ViewBag.Sort = "idxArtist";
                int pageNumber = (page ?? 1);
                int pageSize = 1;

                var albums = db.Albums.GroupBy(a => a.AlbumGuid).Select(a => a.FirstOrDefault());
                if (!String.IsNullOrEmpty(xfilter))               
                    albums = albums.OrderBy(a => a.Artist).Where(a => a.Artist.ToUpper().Contains(xfilter.ToUpper()));
                else 
                    albums = albums.OrderBy(a => a.Artist).Where(a => a.Artist != null);

                List<Models.Artist> Artworks = new List<Models.Artist>();
                Models.Artist tempartist = new Models.Artist();
                foreach (var arty in albums)
                {
                    if (tempartist.ArtArtist != arty.Artist)
                    {
                        if (tempartist.ArtArtist != null)
                            Artworks.Add(tempartist);
                        tempartist = new Models.Artist();
                        tempartist.ArtArtist = arty.Artist;
                        tempartist.ArtAlbum.Add(arty);
                    }
                    else tempartist.ArtAlbum.Add(arty);                                               
                }
                if (tempartist.ArtArtist != null)
                    Artworks.Add(tempartist);    
          
                return View(Artworks.ToPagedList(pageNumber, pageSize));               
            }

        public ActionResult oneAlbum(Guid ID)
        {
            var unoAlbum = db.Albums.Select(a => a).Where(a => a.AlbumGuid == ID).ToList();
  
            AlbumView viewalbum = new AlbumView();
            foreach (var album in unoAlbum)
            {
                if (viewalbum.AlbumArtUrl == null || viewalbum.AlbumArtUrl == "")
                { // Just populate once
                viewalbum.AlbumId = album.AlbumId;
                viewalbum.AlbumArtUrl = album.AlbumArtUrl;
                viewalbum.AlbumName = album.AlbumName;
                viewalbum.Artist = album.Artist;
                viewalbum.AlbumGuid= album.AlbumGuid;
                viewalbum.GenreId = album.GenreId;
                viewalbum.Year = album.Year;
                }
                viewalbum.Title.Add(album.Title);
                viewalbum.FileURI.Add(album.FileURI);
            }
            ViewBag.tossme = viewalbum;
            return View(viewalbum);  
            
            //INTENTIONALLY UNREACHABLE NOW      
            var message = new HttpRequestMessage();
            var content = new MultipartFormDataContent();

            foreach (var album in unoAlbum)  // THIS IS FOR LATER AUTO-POSTING FORM FOR UPLOAD
            {
                var filestream = new FileStream(album.FileURI, FileMode.Open);
                var fileName = System.IO.Path.GetFileName(album.FileURI);
                content.Add(new StreamContent(filestream), "file", fileName);
            }      
            message.Method = HttpMethod.Post;
            // 3128, 8081,2807
            message.Content = content;
            message.RequestUri = new Uri("http://localhost:2807/api/fileupload/");
            var client = new HttpClient();
            client.SendAsync(message).ContinueWith(task =>
            {
                if (task.Result.IsSuccessStatusCode)
                { 
                    //do something with response
                }
            });
        }

       // IMAGE HELPER ACTIONS
 
        public ActionResult Thumbnail (string original, int ht, int wd)
        {
            if (original.Substring(0,1) == "~")
               original=Server.MapPath(original);

             System.Drawing.Image iDrawn = System.Drawing.Image.FromFile(original);
             System.Drawing.Image thumb = new Bitmap(iDrawn.GetThumbnailImage(ht, wd, null, System.IntPtr.Zero));
             var stream = new MemoryStream();
             thumb.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
             return File(stream.ToArray(), "image/jpeg");
        }

       // RETURN FILE AS STATIC STRING IMAGE FOR HTML
            public ActionResult WMPImage(string fn)
            {
                return File(fn, "image/jpeg");
            }

           public ActionResult BMPImage(Byte[] bmp)
            {
                return new FileContentResult(bmp, "image/jpeg");
            }

         //[Authorize]
         //public async Task<ActionResult> UpGoogle(string UploadFileName, CancellationToken cancellationToken)
         //{
             //ClientSecrets secrets =
             //    new ClientSecrets
             //    {
             //        ClientId = "148431152862-6l8g2ebnjmngequndsfd4hju8kthln7d.apps.googleusercontent.com",
             //        ClientSecret = "rEwXWlEDzHLlQoZJagofls5O"
             //    };
         
               // UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets,
               // new[] { DriveService.Scope.Drive },
               // "user", CancellationToken.None).Result;

                // This solution from http://peleyal.blogspot.com/2014/01/aspnet-mvc-with-google-openid-and-oauth.html
             // and here https://developers.google.com/api-client-library/dotnet/guide/aaa_oauth
             // https://github.com/peleyal/peleyal

             //var result = await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).
             //    AuthorizeAsync(cancellationToken);

             //if (result.Credential == null)
             //    return RedirectToAction("idxAlbum");
              //   return new RedirectResult(result.RedirectUri);

             //UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets,
             //new[] { DriveService.Scope.Drive }, "user", CancellationToken.None).Result;

       //      var service = new DriveService(new BaseClientService.Initializer()
       //       {
       ////           HttpClientInitializer = credential,
       //           HttpClientInitializer = result.Credential,
       //           ApplicationName = "MyMuse",
       //       });

            //    await apigoogle.uploadinter(UploadFileName, service);
              //await apigoogle.UploadFileAsync(UploadFileName, service).ContinueWith(t =>
              //{
              //    //   uploaded succeeded
              //    Console.WriteLine("\"{0}\" was uploaded successfully", UploadFileName);
              //    //DownloadFile(service, uploadedFile.DownloadUrl);
              //    //DeleteFile(service, uploadedFile);
              //}, TaskContinuationOptions.OnlyOnRanToCompletion);

           //    return RedirectToAction("idxAlbum");
           //}

         //[Authorize]
         //public async Task<ActionResult> UpOneDrive(string UploadFileName, CancellationToken cancellationToken)
         //{
         //    string uri = "https://apis.live.net/v5.0/me/skydrive/files/" + UploadFileName + "?";//+ App.Current.Properties["access_token"];
         //    HttpWebRequest req = WebRequest.CreateHttp(uri);
         //    req.Method = "PUT";
         //    using (Stream reqStream = req.GetRequestStream())
         //    {
         //        byte[] buff = System.IO.File.ReadAllBytes(UploadFileName);
         //        reqStream.Write(buff, 0, buff.Length);
         //    }
         //    string resJson = "";
         //    using (WebResponse res = req.GetResponse())
         //    {
         //        StreamReader sr = new StreamReader(res.GetResponseStream());
         //        resJson = sr.ReadToEnd();
         //    }         
         // return RedirectToAction("idxAlbum");
         //  }

         protected override void Dispose(bool disposing)
         {
             if (disposing)
             {
                 db.Dispose();
             }
             base.Dispose(disposing);
         }
    } // CONTROLLER CLASS
}// NAMESPACE