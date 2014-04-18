using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SendSpace.Responses
{
    /// <upload url="" progress_url="" max_file_size="" upload_identifier="" extra_info="" />

    public class Upload
    {
        [XmlAttribute("url")]
        public String Url { get; set; }

        [XmlAttribute("progress_url")]
        public String ProgressUrl { get; set; }

        [XmlAttribute("max_file_size")]
        public Double MaxFileSize { get; set; }

        [XmlAttribute("upload_identifier")]
        public String UploadIdentifier { get; set; }

        [XmlAttribute("extra_info")]
        public String ExtraInfo { get; set; }
    }
}
