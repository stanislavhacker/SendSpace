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
    public class UploadHandler : Handler
    {
        #region PRIVATE

        private String fileName = null;
        private String contentType = null;
        private Stream dataStream = null;
        private Exception error = null;
        public Dictionary<string, string> data = new Dictionary<string, string>();

        #endregion

        #region EVENTS

        public event EventHandler<UploadDone> Complete;
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

        /// <summary>
        /// Boundary
        /// </summary>
        private String boundary = null;
        public String Boundary
        {
            get
            {
                return boundary;
            }
            set
            {
                boundary = value;
            }
        }

        #endregion

        /// <summary>
        /// Upload handler
        /// </summary>
        public UploadHandler()
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
        }

        /// <summary>
        /// Add post data
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddPostData(string key, string value)
        {
            data.Add(key, value);
        }

        #region PRIVATE

        /// <summary>
        /// Write multi part to stream
        /// </summary>
        /// <param name="s"></param>
        /// <param name="boundary"></param>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        /// <param name="fileContentType"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        private async Task writeMultipartForm(Stream s, string boundary, Dictionary<string, string> data, string fileName, string fileContentType, byte[] fileData)
        {
            /// The first boundary
            byte[] boundarybytes = Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
            /// the last boundary.
            byte[] trailer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "–-\r\n");
            /// the form data, properly formatted
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            /// the form-data file upload, properly formatted
            string fileheaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\";\r\nContent-Type: {2}\r\n\r\n";

            /// Added to track if we need a CRLF or not.
            bool bNeedsCRLF = false;

            if (data != null)
            {
                foreach (string key in data.Keys)
                {
                    /// if we need to drop a CRLF, do that.
                    if (bNeedsCRLF)
                    {
                        await WriteToStream(s, "\r\n");

                        /// Write the boundary.
                        await WriteToStream(s, boundarybytes);
                    }

                    /// Write the key.
                    await WriteToStream(s, string.Format(formdataTemplate, key, data[key]));
                    bNeedsCRLF = true;
                }
            }

            /// If we don't have keys, we don't need a crlf.
            if (bNeedsCRLF)
            {
                await WriteToStream(s, "\r\n");
            }

            await WriteToStream(s, boundarybytes);
            await WriteToStream(s, string.Format(fileheaderTemplate, "userfile", fileName, fileContentType));
            /// Write the file data to the stream.
            await WriteToStream(s, fileData);
            await WriteToStream(s, trailer);
        }

        /// <summary>
        /// Write to stream
        /// </summary>
        /// <param name="s"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private async Task WriteToStream(Stream s, byte[] bytes)
        {
            await s.WriteAsync(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Write to stream
        /// </summary>
        /// <param name="s"></param>
        /// <param name="txt"></param>
        /// <returns></returns>
        private async Task WriteToStream(Stream s, string txt)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(txt);
            await s.WriteAsync(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Process response
        /// </summary>
        private void processResponse()
        {
            XmlSerializer xml = null;
            Stream stream = this.Response.GetResponseStream();
            XmlReader reader = XmlReader.Create(stream);

            //for: UploadDone
            xml = new XmlSerializer(typeof(UploadDone));
            if (xml.CanDeserialize(reader))
            {
                UploadDone uploadInfo = (UploadDone)xml.Deserialize(reader);
                if (Complete != null)
                {
                    Complete.Invoke(this, uploadInfo);
                }
            }
        }

        /// <summary>
        /// Process request
        /// </summary>
        /// <returns></returns>
        private async Task processRequest()
        {
            MemoryStream memoryStream = new MemoryStream();
            dataStream.Seek(0, SeekOrigin.Begin);

            await dataStream.CopyToAsync(memoryStream);
            await this.writeMultipartForm(this.RequestStream, boundary, data, fileName, contentType, memoryStream.ToArray());
            await this.RequestStream.FlushAsync();
            this.RequestStream.Dispose();
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
            await processRequest();
        }

        /// <summary>
        /// Get boundary
        /// </summary>
        /// <returns></returns>
        public string GetBoundary()
        {
            return this.Boundary;
        }

        /// <summary>
        /// Set boudnary
        /// </summary>
        /// <param name="boundary"></param>
        public void SetBoundary(string boundary)
        {
            this.Boundary = boundary;
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
