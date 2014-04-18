using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SendSpace.Responses
{
    /// <result method="anonymous.uploadgetinfo" status="ok"></result>

    [XmlRoot("result")]
    public class AnonymousUploadGetInfo
    {
        [XmlAttribute("method")]
        public String Method { get; set; }

        [XmlAttribute("status")]
        public String Status { get; set; }

        [XmlElement("upload")]
        public Upload Upload { get; set; }
    }
}
