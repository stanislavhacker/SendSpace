using CoPilot.Interfaces;
using CoPilot.Interfaces.EventArgs;
using SendSpace.Enums;
using SendSpace.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SendSpace
{
    public class SendSpaceClient : NetClient
    {
        #region STATICS

        public const string API_URL = "http://api.sendspace.com";
        public const string DOWNLOAD_URL = "http://www.sendspace.com/file/";

        #endregion

        #region EVENTS

        public event EventHandler<UploadEventArgs> UploadComplete;
        public event EventHandler<ProgressEventArgs> UploadProgress;
        public event EventHandler<ExceptionEventArgs> UploadError;

        public event EventHandler<UriEventArgs> GetComplete;
        public event EventHandler<ExceptionEventArgs> GetError;

        public event EventHandler DeleteComplete;
        public event EventHandler<ExceptionEventArgs> DeleteError;

        public event EventHandler<StreamEventArgs> DownloadComplete;
        public event EventHandler<ExceptionEventArgs> DownloadError;

        #endregion

        #region PRIVATE

        internal String apiKey;
        internal String apiVersion;
        internal String email;
        internal String message;
        private Boolean sendEmail = false;

        #endregion

        /// <summary>
        /// Send space klient
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="apiVersion"></param>
        public SendSpaceClient(String apiKey,  String apiVersion = "1.0")
        {
            this.apiKey = apiKey;
            this.apiVersion = apiVersion;
        }

        /// <summary>
        /// Email send
        /// </summary>
        /// <param name="email"></param>
        /// <param name="message"></param>
        public void Email(String email, String message)
        {
            this.sendEmail = true;
            this.email = email;
            this.message = message;
        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file"></param>
        /// <param name="contentType"></param>
        public void Upload(String name, String contentType, Stream file)
        {
            //get url
            var url = this.getUrl(Methods.ANONYMOUS_UPLOAD_GET_INFO);

            //handler
            StartUploadHandler handler = Factory.Get<StartUploadHandler>(url);
            handler.SetData(name, contentType, file);
            handler.Complete += uploadComplete;
            handler.Progress += uploadProgress;
            handler.Error += uploadError;

            //email
            if (this.sendEmail)
            {
                handler.SendEmail(this.email, this.message);
            }
        }

        /// <summary>
        /// Get url of file
        /// </summary>
        /// <param name="url"></param>
        public void Get(string url)
        {
            //handler
            StartDownloadHandler handler = Factory.Get<StartDownloadHandler>(new Uri(url));
            handler.Complete += openComplete;
            handler.Error += openError;
        }

        /// <summary>
        /// Download by url
        /// </summary>
        /// <param name="url"></param>
        public void Download(Uri url)
        {
            //handler
            StartDownloadHandler handler = Factory.Get<StartDownloadHandler>(url);
            handler.Complete += (object sender, Uri e) =>
            {
                DownloadHandler downHandler = Factory.Get<DownloadHandler>(e);
                downHandler.Complete += downloadComplete;
                downHandler.Error += downloadError;
            };
            handler.Error += downloadError;
        }

         /// <summary>
        /// Download by id
        /// </summary>
        /// <param name="url"></param>
        public void Download(String id)
        {
            Uri url = new Uri(DOWNLOAD_URL + id);
            this.Download(url);
        }

        /// <summary>
        /// Delete file by url
        /// </summary>
        /// <param name="url"></param>
        public void Delete(string url)
        {
            //add delete action
            url += "?action=1";

            //handler
            DeleteHandler handler = Factory.Get<DeleteHandler>(new Uri(url));
            handler.Complete += deleteComplete;
            handler.Error += deleteError;
        }

        #region PRIVATE

        /// <summary>
        /// Download complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadComplete(object sender, Stream e)
        {
            if (DownloadComplete != null)
            {
                StreamEventArgs args = new StreamEventArgs();
                args.Stream = e;
                DownloadComplete.Invoke(this, args);
            }
        }

        /// <summary>
        /// Download error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadError(object sender, Exception e)
        {
            if (DownloadError != null)
            {
                ExceptionEventArgs args = new ExceptionEventArgs();
                args.Exception = e;
                DownloadError.Invoke(sender, args);
            }
        }

        /// <summary>
        /// Delete error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteError(object sender, Exception e)
        {
            if (DeleteError != null)
            {
                ExceptionEventArgs args = new ExceptionEventArgs();
                args.Exception = e;
                DeleteError.Invoke(sender, args);
            }
        }

        /// <summary>
        /// Delete complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteComplete(object sender, EventArgs e)
        {
            if (DeleteComplete != null)
            {
                DeleteComplete.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Open complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openComplete(object sender, Uri e)
        {
            if (GetComplete != null)
            {
                UriEventArgs args = new UriEventArgs();
                args.Uri = e;
                GetComplete.Invoke(this, args);
            }
        }

        /// <summary>
        /// Open error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openError(object sender, Exception e)
        {
            if (GetError != null)
            {
                ExceptionEventArgs args = new ExceptionEventArgs();
                args.Exception = e;
                GetError.Invoke(this, args);
            }
        }

        /// <summary>
        /// Upload progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uploadProgress(object sender, Responses.Progress e)
        {
            if (UploadProgress != null)
            {
                CoPilot.Interfaces.Progress progress = new CoPilot.Interfaces.Progress();
                progress.Elapsed = e.Elapsed;
                progress.Eta = e.Eta;
                progress.Percent = e.Percent;
                progress.Speed = e.Speed;
                progress.Status = e.Status;
                progress.TotalSize = e.TotalSize;
                progress.UploadedBytes = e.UploadedBytes;

                ProgressEventArgs args = new ProgressEventArgs();
                args.Progress = progress;
                UploadProgress.Invoke(this, args);
            }
        }

        /// <summary>
        /// Upload complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uploadComplete(object sender, Responses.UploadDone e)
        {
            if (UploadComplete != null)
            {
                CoPilot.Interfaces.UploadDone upload = new CoPilot.Interfaces.UploadDone();
                upload.DeleteUrl = e.DeleteUrl;
                upload.DownloadUrl = e.DownloadUrl;
                upload.FileId = e.FileId;
                upload.Status = e.Status;

                UploadEventArgs args = new UploadEventArgs();
                args.UploadDone = upload;
                UploadComplete.Invoke(this, args);
            }
        }

        /// <summary>
        /// Upload error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uploadError(object sender, Exception e)
        {
            if (UploadError != null)
            {
                ExceptionEventArgs args = new ExceptionEventArgs();
                args.Exception = e;
                UploadError.Invoke(this, args);
            }
        }

        /// <summary>
        /// Get url path
        /// </summary>
        /// <param name="method"></param>
        /// <param name="speedLimit"></param>
        /// <returns></returns>
        public Uri getUrl(Methods method, Double speedLimit = 0)
        {
            var url = API_URL + "/rest/?";

            List<String> parts = new List<string>();
            parts.Add("method=" + method.ToString());
            parts.Add("speed_limit=" + speedLimit);
            parts.Add("api_key=" + this.apiKey);
            parts.Add("api_version=" + this.apiVersion);

            return new Uri(url + String.Join("&", parts));
        }

        #endregion
    }
}
