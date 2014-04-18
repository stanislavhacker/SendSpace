using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SendSpace.Interface
{
    public interface Handler
    {
        WebResponse GetResponse();
        Stream GetRequestStream();
        String GetBoundary();
        Exception GetError();

        Task SetResponse(WebResponse stream);
        Task SetRequestStream(Stream stream);
        void SetBoundary(String boundary);
        void SetError(Exception error);
    }
}
