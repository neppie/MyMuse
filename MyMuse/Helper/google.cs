using System.Threading.Tasks;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Upload;
using System;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Util.Store;
using System.IO;

namespace MyMuse.Controllers
{
    /// <summary>
    /// A sample for the Drive API. This samples demonstrates resumable media upload and media download.
    ///// See https://developers.google.com/drive/ for more details regarding the Drive API.
    // </summary>
    public class AppAuthFlowMetadata : FlowMetadata
    {
        private static readonly IAuthorizationCodeFlow flow =
          new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer()
              {
                  ClientSecrets = new ClientSecrets
                  {
                      ClientId = "148431152862-6l8g2ebnjmngequndsfd4hju8kthln7d.apps.googleusercontent.com",
                      ClientSecret = "rEwXWlEDzHLlQoZJagofls5O",
                  },
                  Scopes = new[] { DriveService.Scope.Drive },
                  DataStore = new FileDataStore("Google.Apis.Sample.MVC")
              }
              );

        public override string GetUserId(Controller controller)
        {
            var user = controller.User.Identity.GetUserName();
            return user.ToString();
        }

        public override IAuthorizationCodeFlow Flow
        {
            get { return flow; }
        }
    }

    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        protected override Google.Apis.Auth.OAuth2.Mvc.FlowMetadata FlowData
        {
            get { return new AppAuthFlowMetadata(); }
        }
    }

    public class apigoogle
    {

        // ... https://developers.google.com/api-client-library/dotnet/guide/media_upload

            /// <summary>
            /// Insert new file.
            /// </summary>
            /// <param name="service">Drive API service instance.</param>
            /// <param name="title">Title of the file to insert, including the extension.</param>
            /// <param name="description">Description of the file to insert.</param>
            /// <param name="parentId">Parent folder's ID.</param>
            /// <param name="mimeType">MIME type of the file to insert.</param>
            /// <param name="filename">Filename of the file to insert.</param><br>  /// <returns>Inserted file metadata, null is returned if an API error occurred.</returns>
            private static Google.Apis.Drive.v2.Data.File insertFile(DriveService service, String title, String description, String parentId, String mimeType, String filename)
            {
                // File's metadata.
                Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
                body.Title = title;
                body.Description = description;
                body.MimeType = mimeType;

                // Set the parent folder.
                if (!String.IsNullOrEmpty(parentId))
                {
                    body.Parents = new List<ParentReference>() { new ParentReference() { Id = parentId } };
                }

                // File's content.
                byte[] byteArray = System.IO.File.ReadAllBytes(filename);
                MemoryStream stream = new MemoryStream(byteArray);
                try
                {
                    FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, mimeType);
                    request.Upload();

                    Google.Apis.Drive.v2.Data.File file = request.ResponseBody;

                    // Uncomment the following line to print the File ID.
                    // Console.WriteLine("File ID: " + file.Id);

                    return file;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
        
        public static Task uploadinter(string upyours, DriveService service)
        {
            Google.Apis.Drive.v2.Data.File body = new Google.Apis.Drive.v2.Data.File();
            var title = upyours;
            if (title.LastIndexOf('/') != -1)
            {
                title = title.Substring(title.LastIndexOf('/') + 1);
            }
            body.Title = title;
      //      body.Description = "A Blast";
            body.MimeType = "audio/mpeg";

    //        byte[] byteArray = System.IO.File.ReadAllBytes(upyours);
            System.IO.FileStream stream = new System.IO.FileStream(upyours, System.IO.FileMode.Open,
                System.IO.FileAccess.Read);

            FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "audio/mpeg");
            var task = Task.Run(() => request.UploadAsync());

     //       body = request.ResponseBody;
            return task;
        }

        public static Task<IUploadProgress> UploadFileAsync(string uploadfilename, DriveService service)
        {
            var title = uploadfilename;
            if (title.LastIndexOf('/') != -1)
            {
                title = title.Substring(title.LastIndexOf('/') + 1);
            }
            var uploadStream = new System.IO.FileStream(uploadfilename, System.IO.FileMode.Open,
                System.IO.FileAccess.Read);
            var parent = new ParentReference() { Id = "MyMuse" };
            var parentlist = new List<ParentReference>();
            var insert = service.Files.Insert(new Google.Apis.Drive.v2.Data.File
            {
                Title = title,
                //Parents = new List<ParentReference>() { new ParentReference() { Id = "MyMuse" } }
            },
            uploadStream, "audio/mpeg");

            insert.ChunkSize = FilesResource.InsertMediaUpload.MinimumChunkSize * 2;
            var task = insert.UploadAsync();

            task.ContinueWith(t =>
            {
                // NotOnRanToCompletion - this code will be called if the upload fails
                Console.WriteLine("Upload Failed. " + t.Exception);
            }, TaskContinuationOptions.NotOnRanToCompletion);
            task.ContinueWith(t =>
            {
                uploadStream.Dispose();
            });

            return task;
        }

        ///// <summary>Downloads the media from the given URL.</summary>
        //private static void DownloadFile(DriveService service, string url)
        //{
        //    var downloader = new MediaDownloader(service);
        //    downloader.ChunkSize = DownloadChunkSize;
        //    // add a delegate for the progress changed event for writing to console on changes
        //    downloader.ProgressChanged += Download_ProgressChanged;

        //    // figure out the right file type base on UploadFileName extension
        //    var lastDot = UploadFileName.LastIndexOf('.');
        //    var fileName = DownloadDirectoryName + @"\Download" +
        //        (lastDot != -1 ? "." + UploadFileName.Substring(lastDot + 1) : "");
        //    using (var fileStream = new System.IO.FileStream(fileName,
        //        System.IO.FileMode.Create, System.IO.FileAccess.Write))
        //    {
        //        var progress = downloader.Download(url, fileStream);
        //        if (progress.Status == DownloadStatus.Completed)
        //        {
        //            Console.WriteLine(fileName + " was downloaded successfully");
        //        }
        //        else
        //        {
        //            Console.WriteLine("Download {0} was interpreted in the middle. Only {1} were downloaded. ",
        //                fileName, progress.BytesDownloaded);
        //        }
        //    }
        //}

        /// <summary>Deletes the given file from drive (not the file system).</summary>
        private static void DeleteFile(DriveService service, Google.Apis.Drive.v2.Data.File file)
        {
            //CommandLine.WriteLine("Deleting file '{0}'...", file.Id);
            service.Files.Delete(file.Id).Execute();
            //CommandLine.WriteLine("File was deleted successfully");
        }
    }
}

