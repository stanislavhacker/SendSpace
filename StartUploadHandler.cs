using SendSpace.Enums;
using SendSpace.Interface;
using SendSpace.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SendSpace
{
    public class StartUploadHandler : Handler
    {
        #region PRIVATE

        private AnonymousUploadGetInfo uploadInfo = null;
        private Stream dataStream = null;
        private String fileName = null;
        private String contentType = null;
        private Exception error = null;

        private String email;
        private String message;
        private Boolean sendEmail = false;

        #endregion

        #region EVENTS

        public event EventHandler<UploadDone> Complete;
        public event EventHandler<Progress> Progress;
        public event EventHandler<Exception> Error;

        #endregion

        #region PROPERTY

        /// <summary>
        /// Request stream
        /// </summary>
        private Stream requestStream = null;
        public Stream RequestStream
        {
            get
            {
                return requestStream;
            }
            set
            {
                requestStream = value;
            }
        }

        /// <summary>
        /// Web reponse
        /// </summary>
        private WebResponse response = null;
        public WebResponse Response
        {
            get
            {
                return response;
            }
            set
            {
                response = value;
            }
        }

        #endregion


        /// <summary>
        /// Upload handler
        /// </summary>
        public StartUploadHandler()
        {              
        }

        /// <summary>
        /// Set data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file"></param>
        /// <param name="contentType"></param>
        public void SetData(String name, String contentType, Stream file)
        {
            this.fileName = name;
            this.contentType = contentType;
            this.dataStream = file;
            this.beginUpload();
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="message"></param>
        public void SendEmail(string email, string message)
        {
            this.sendEmail = true;
            this.email = email;
            this.message = message;
        }

        #region PRIVATE

        /// <summary>
        /// Process response
        /// </summary>
        private void processResponse()
        {
            XmlSerializer xml = null;
            Stream stream = this.Response.GetResponseStream();
            XmlReader reader = XmlReader.Create(stream);

            //for: AnonymousUploadGetInfo
            xml = new XmlSerializer(typeof(AnonymousUploadGetInfo));
            if (xml.CanDeserialize(reader))
            {
                AnonymousUploadGetInfo uploadInfo = (AnonymousUploadGetInfo)xml.Deserialize(reader);
                this.processUploadInfo(uploadInfo);
            }
        }

        /// <summary>
        /// Process upload info
        /// </summary>
        /// <param name="uploadInfo"></param>
        private void processUploadInfo(AnonymousUploadGetInfo uploadInfo)
        {
            this.uploadInfo = uploadInfo;
            this.beginUpload();
        }

        /// <summary>
        /// Process progress
        /// </summary>
        private async Task processProgress()
        {
            var info = this.uploadInfo;
            var url = new Uri(info.Upload.ProgressUrl);

            await Task.Delay(TimeSpan.FromMilliseconds(1000));

            ProgressHandler handler = Factory.Get<ProgressHandler>(url);
            handler.Progress += UploadProgress;
            handler.Error += ErrorProgress;
        }

        /// <summary>
        /// Process upload
        /// </summary>
        private void processUpload()
        {
            var info = this.uploadInfo;
            var url = new Uri(info.Upload.Url);

            UploadHandler handler = Factory.Post<UploadHandler>(url);
            handler.AddPostData("MAX_FILE_SIZE", info.Upload.MaxFileSize.ToString());
            handler.AddPostData("UPLOAD_IDENTIFIER", info.Upload.UploadIdentifier);
            handler.AddPostData("extra_info", info.Upload.ExtraInfo);

            //send email
            if (this.sendEmail)
            {
                handler.AddPostData("notify_uploader", this.email);
                handler.AddPostData("recipient_message", this.message);
            }

            handler.SetData(fileName, contentType, dataStream);
            handler.Complete += UploadComplete;
            handler.Error += UploadError;
        }

        /// <summary>
        /// Begin upload
        /// </summary>
        private void beginUpload()
        {
            //check data
            if (this.dataStream != null && this.uploadInfo != null)
            {
                this.processUpload();
                this.processProgress();
            }
        }

        /// <summary>
        /// Progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UploadProgress(object sender, Progress e)
        {
            //call progress
            if (Progress != null)
            {
                Progress.Invoke(this, e);
            }

            if (e.Status == "ok")
            {
                await this.processProgress();
            }
        }

        /// <summary>
        /// Uplaod complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadComplete(object sender, UploadDone e)
        {
            if (Complete != null)
            {
                Complete.Invoke(this, e);
            }
        }

        /// <summary>
        /// Progress error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ErrorProgress(object sender, Exception e)
        {
            Progress progress = new Responses.Progress();
            progress.Elapsed = null;
            progress.Eta = null;
            progress.Percent = 0;
            progress.Speed = 0;
            progress.Status = "error";
            progress.TotalSize = 0;
            progress.UploadedBytes = 0;

            this.UploadProgress(this, progress);
        }

        /// <summary>
        /// Upload error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadError(object sender, Exception e)
        {
            if (Error != null)
            {
                Error.Invoke(this, e);
            }
        }


        #endregion

        #region INTERFACE METHODS

        /// <summary>
        /// Get response
        /// </summary>
        /// <returns></returns>
        public WebResponse GetResponse()
        {
            return this.Response;
        }

        /// <summary>
        /// Get request stream
        /// </summary>
        /// <returns></returns>
        public Stream GetRequestStream()
        {
            return this.RequestStream;
        }

        /// <summary>
        /// Set reponse
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task SetResponse(WebResponse response)
        {
            this.Response = response;
            this.processResponse();
        }

        /// <summary>
        /// Set request stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task SetRequestStream(Stream stream)
        {
            this.RequestStream = stream;
        }

        /// <summary>
        /// Get boundary
        /// </summary>
        /// <returns></returns>
        public string GetBoundary()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set boudnary
        /// </summary>
        /// <param name="boundary"></param>
        public void SetBoundary(string boundary)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Get error
        /// </summary>
        /// <returns></returns>
        public Exception GetError()
        {
            return this.error;
        }

        /// <summary>
        /// Set error
        /// </summary>
        /// <param name="error"></param>
        public void SetError(Exception error)
        {
            this.error = error;

            //callback
            if (Error != null)
            {
                Error.Invoke(this, error);
            }
        }

        #endregion
    }
}
