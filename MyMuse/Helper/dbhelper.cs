using Microsoft.AspNet.SignalR;
using MyMuse.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Web.Mvc;
using System.Xml;
using TagLib;
using System.Linq;
using System.Data.Entity;


namespace MyMuse.Controllers
{
   
    public class helper : Controller
    {
        public static object _dirlock = new object();

        public static List<Album> AlbumInit (string xml)
        {
            List<Album> AlbumInit = new List<Album>();
            xml = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/") + xml;
            if (System.IO.File.Exists(xml))
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(AlbumsView));
                FileStream fs = new FileStream(xml, FileMode.Open);
                XmlDictionaryReader reader = XmlDictionaryReader.
                    CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                AlbumsView BuildAlbums = (AlbumsView)dcs.ReadObject(reader);
                reader.Close();
                foreach (Album EachAlbum in BuildAlbums)
                    AlbumInit.Add(EachAlbum);
            }
                return AlbumInit;                             
        }

        // XML DESERIALIZATION 
        //public static List<AlbumView> Deserialize(string xml)
          public static void Deserialize(string xml)
        {
        //    Database.SetInitializer<MyMuseContext>(new DropCreateDatabaseAlways<MyMuseContext>());
            lock (_dirlock)
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(AlbumsView));
                FileStream fs = new FileStream(xml, FileMode.Open);
                XmlDictionaryReader reader = XmlDictionaryReader.
                    CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                AlbumsView BuildAlbums = (AlbumsView)dcs.ReadObject(reader);
                reader.Close();

                var PH = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();

                MyMuseContext db = new MyMuseContext();
                try
                {
                    var all = from c in db.Albums select c;
                    db.Albums.RemoveRange(all);
                    var bll = from c in db.AlbumsView select c;
                    db.AlbumsView.RemoveRange(bll);
                    db.SaveChanges();
                }
                catch {
                }

                int albumcount = 0;
                foreach (var albumview in BuildAlbums)
                {
                    ++albumcount;
                    var guid = Guid.NewGuid();
                    double pct = (double)albumcount / BuildAlbums.Count;
                    ProgressHub.ReportMessage(" ", albumcount, BuildAlbums.Count, pct);
                    Album tracks = new Album(); int i = 0;
                    
                    foreach (string title in albumview.Title)
                    {
                        tracks.AlbumArtUrl = albumview.AlbumArtUrl;
                        tracks.AlbumGuid = guid;
                        tracks.AlbumName = albumview.AlbumName;
                        tracks.Artist = albumview.Artist;
                        tracks.Title = title;
                        tracks.FileURI = albumview.FileURI[i++];
                        tracks.Track = albumview.Track;
                        tracks.Year = albumview.Year;
                        db.Albums.Add(tracks);
                        db.SaveChanges();
                    }
                }
   //             return BuildAlbums;
            }
        }

         //XML SERIALIZTION
        public static void Serialize(object obj, string xml)
        {
            using (Stream stream = new FileStream(xml, FileMode.Create, FileAccess.Write))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                 serializer.WriteObject(stream, obj);
            }
        }

        // EXTRACT ALBUM COVER
        public static string coverpath(string APPD, TagLib.File tagFile, string f)
        {
            string viewpath = "";
            // CHECK WMP !!!        
            foreach (string h in Directory.GetFiles(Path.GetDirectoryName(f), "AlbumArt_*_Large.jpg"))
            {
                viewpath = h;
                System.Drawing.Image iDrawn = System.Drawing.Image.FromFile(h);
                var guid = Guid.NewGuid();
                System.Drawing.Image thumb = iDrawn.GetThumbnailImage(200, 200, new System.Drawing.Image.GetThumbnailImageAbort(delegate { return false; }), IntPtr.Zero);
                string dir = APPD + "Images/" + guid + ".jpg";
                viewpath = "~/App_Data/Images/" + guid + ".jpg";
                thumb.Save(dir, System.Drawing.Imaging.ImageFormat.Jpeg);
                return viewpath;
            }
            // CHECK THE ID3 MP3 TAG... LETS TRY IT THE HARD WAY
            if (tagFile.Tag.Pictures.Length >= 1)
                foreach (TagLib.IPicture pic in tagFile.Tag.Pictures)
                {
                    //if (pic.Type != TagLib.PictureType.FrontCover)
                    //    continue;
                    string mimetype = pic.MimeType;
                    // TRY THE HARD WAY in ID3v1 TAG
                    MemoryStream ms = new MemoryStream(pic.Data.Data);
                    System.Drawing.Image iDrawn = System.Drawing.Image.FromStream(ms);
                    var guid = Guid.NewGuid();
                    System.Drawing.Image thumb = iDrawn.GetThumbnailImage(200, 200, new System.Drawing.Image.GetThumbnailImageAbort(delegate { return false; }), IntPtr.Zero);
                    string dir = APPD + "Images/" + guid + ".jpg";
                    viewpath = "~/App_Data/Images/" + guid + ".jpg";
                    thumb.Save(dir, System.Drawing.Imaging.ImageFormat.Jpeg);
                    ms.Close();
                    return viewpath;
                }
            // CHECK THE ID3v2 MP3 TAG... LETS TRY THE REALLY HARD WAY
            if (tagFile.Tag.Pictures.Length >= 1)
            {
                TagLib.Id3v2.Tag wag = tagFile.GetTag(TagTypes.Id3v2, true) as TagLib.Id3v2.Tag;
                // The file contains a special ID3v2 tag.
                if (wag != null)
                    foreach (TagLib.Id3v2.AttachedPictureFrame pframe in wag.GetFrames("APIC"))
                    {
                        MemoryStream ms = new MemoryStream(pframe.Data.Data);
                        System.Drawing.Image iDrawn = System.Drawing.Image.FromStream(ms);
                        var guid = Guid.NewGuid(); 
                        System.Drawing.Image thumb = new Bitmap(iDrawn.GetThumbnailImage(200, 200, null, System.IntPtr.Zero));
                        viewpath = "~/App_Data/Images/" + guid + ".jpg";
                        string dir = APPD + "Images/" + guid + ".jpg";
                        thumb.Save(dir, System.Drawing.Imaging.ImageFormat.Jpeg);
                        ms.Close();
                        return viewpath;
                    }
            }
            viewpath = "~/Content/nocover.jpg";
             return viewpath;
        } // Coverpath      

        // EXCTRACT MP3s FROM FILE SYSTEM
        public static void DirScrape(string APPD)
        {
            lock (_dirlock)
            {
                var ENV = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                var Muse = Directory.EnumerateFiles(ENV, "*.mp3", SearchOption.AllDirectories);
                List<string> countem = new List<string>(Muse);
                int TotalMP3s = countem.Count;
                var PH = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
 
                MyMuseContext db = new MyMuseContext();
                try
                {
                    var all = from c in db.Albums select c;
                    db.Albums.RemoveRange(all);
                    var bll = from c in db.AlbumsView select c;
                    db.AlbumsView.RemoveRange(bll);
                    db.SaveChanges();
                }
                catch
                {
                }
                AlbumsView BuildAlbums = new AlbumsView(); // For View
                List<Models.Album> AlbumList = new List<Models.Album>(); // For DB
                AlbumView CoverView = new AlbumView();
                bool covered = false; // has a cover image
                bool viewed = false; // has any cover/view (at least one song)
                var guid = Guid.NewGuid();
                string viewpath = "";
                string LastPath = "";

                int aID = 1;
                foreach (var f in Muse)
                {
                    try
                    {
                        string WMPath = Path.GetDirectoryName(f);
                        if (WMPath != LastPath)
                        // FRESH ALBUM OR ARTIST  
                        {
                            LastPath = WMPath;
                            if (viewed) // save the album information 
                            {
                                if (covered) // ... cover path or no image link
                                {
                                    CoverView.AlbumArtUrl = viewpath;
                                }
                                else
                                {
                                    CoverView.AlbumArtUrl = "~/Content/nocover.jpg";
                                }
                                BuildAlbums.Add(CoverView); // KEEP
                            }
                            viewed = false;
                            covered = false;
                        }

                        // FRESH TRACK TITLE / NEW SONG
                        Models.AlbumView TempAlbum = new Models.AlbumView();
                        Models.Album dbTemp = new Models.Album();
                        TagLib.File tagFile = TagLib.File.Create(f);
                        if (!covered) viewpath = coverpath(APPD, tagFile, f);
                        if (viewpath != "~/Content/nocover.jpg") covered = true;
                        // Make a new Song
                        // XML & Memory
                        TempAlbum.AlbumId = ++aID;
                        TempAlbum.AlbumGuid = guid;
                        TempAlbum.AlbumArtUrl = viewpath;
                        TempAlbum.AlbumName = tagFile.Tag.Album;
                        if (tagFile.Tag.AlbumArtists.Length > 0)
                            TempAlbum.Artist = tagFile.Tag.AlbumArtists[0];
                        TempAlbum.Title.Add(tagFile.Tag.Title);
                        TempAlbum.GenreId = tagFile.Tag.Genres[0];
                        TempAlbum.Year = (int)tagFile.Tag.Year;
                        TempAlbum.Track = (int)tagFile.Tag.Track;

                        TempAlbum.FileURI.Add(f);
                        // Database
                        dbTemp.AlbumId = TempAlbum.AlbumId;
                        dbTemp.AlbumGuid = TempAlbum.AlbumGuid;
                        dbTemp.AlbumArtUrl = TempAlbum.AlbumArtUrl;
                        dbTemp.AlbumName = TempAlbum.AlbumName;
                        dbTemp.Artist = TempAlbum.Artist;
                        dbTemp.Title = tagFile.Tag.Title;
                        dbTemp.FileURI = f;
                        dbTemp.GenreId = tagFile.Tag.Genres[0];
                        dbTemp.Year = (int)tagFile.Tag.Year;
                        dbTemp.Track = (int)tagFile.Tag.Track;
                        AlbumList.Add(dbTemp);
                        db.Albums.Add(dbTemp);

                        // Create a new cover view to build or add title to existing
                        if (viewed)
                        {
                            CoverView.Title.Add(tagFile.Tag.Title);
                            CoverView.FileURI.Add(f);
                        }
                        else
                        {
                            CoverView = TempAlbum;
                            viewed = true;
                        }

                        double pct = (double)aID * 100 / TotalMP3s;
                        ProgressHub.ReportMessage(f, aID, TotalMP3s, pct);

                      //  if (aID > 500) break;
                    }// Try
                    catch (System.Exception excpt)
                    {
                        Console.WriteLine(excpt.Message);
                    } // CATCH
                } // End of SONG
                string xmlpath = APPD + "xmlAlbum.xml";
                Serialize(BuildAlbums, xmlpath);
                //AlbumDbInit(AlbumList);
       //         return BuildAlbums;
            } // End of LOCK
        } // End of DIRSCRAPE
    } // helper class
}