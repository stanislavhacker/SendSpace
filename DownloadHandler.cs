using SendSpace.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SendSpace
{
    public class DownloadHandler : Handler
    {
        #region PRIVATE

        private Exception error = null;

        #endregion

        #region EVENTS

        public event EventHandler<Stream> Complete;
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
        /// Download handler
        /// </summary>
        public DownloadHandler()
        {
        }

        #region PRIVATE

        /// <summary>
        /// Process response
        /// </summary>
        private void processResponse()
        {
            var stream = this.Response.GetResponseStream();

            //callback
            if (Complete != null)
            {
                Complete.Invoke(this, stream);
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
