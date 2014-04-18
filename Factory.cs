using SendSpace.Enums;
using SendSpace.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SendSpace
{
    public class Factory
    {

        /// <summary>
        /// Create get handler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public static T Get<T>(Uri url)
        {
            Handler intance = (Handler)Activator.CreateInstance(typeof(T));
            intance.SetRequestStream(null);

            WebRequest request = WebRequest.Create(url);
            request.Method = "GET";
            request.BeginGetResponse(async (responseCallback) =>
            {
                try
                {
                    WebResponse response = request.EndGetResponse(responseCallback);
                    await intance.SetResponse(response);
                }
                catch(Exception e)
                {
                    intance.SetError(e);
                }
            }, null);

            return (T)intance;
        }

        /// <summary>
        /// Create post handler
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="uploadUrl"></param>
        /// <param name="data"></param>
        /// <param name="dataStream"></param>
        public static T Post<T>(Uri url)
        {
            String boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            Handler intance = (Handler)Activator.CreateInstance(typeof(T));
            intance.SetBoundary(boundary);

            WebRequest request = WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.BeginGetRequestStream(async (result) =>
            {
                try
                {
                    Stream requestStream = request.EndGetRequestStream(result);
                    await intance.SetRequestStream(requestStream);

                    request.BeginGetResponse(async (e) =>
                    {
                        try
                        {
                            WebResponse response = request.EndGetResponse(e);
                            await intance.SetResponse(response);
                        }
                        catch (Exception error)
                        {
                            intance.SetError(error);
                        }
                    }, null);
                }
                catch (Exception error)
                {
                    intance.SetError(error);
                }

            }, null);

            return (T)intance;
        }


    }
}
