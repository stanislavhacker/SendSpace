using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SendSpace.Responses
{

    /// <upload_done>
    ///     <status>ok</status>
    ///     <download_url>http://www.sendspace.com/file/XXXXX</download_url>
    ///     <delete_url>http://www.sendspace.com/delete/XXXXX/YYYY</delete_url>
    ///     <file_id>XXXXX</file_id>
    /// </upload_done>

    [XmlRoot("upload_done")]
    public class UploadDone
    {
        [XmlElement("status")]
        public String Status { get; set; }

        [XmlElement("download_url")]
        public String DownloadUrl { get; set; }

        [XmlElement("delete_url")]
        public String DeleteUrl { get; set; }

        [XmlElement("file_id")]
        public String FileId { get; set; }
    }
}
