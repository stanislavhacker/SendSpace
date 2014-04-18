using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SendSpace.Responses
{
    /// <progress>
    ///     <status>ok/fail/done</status>
    ///     <eta>00:00:00</eta>
    ///     <speed>50</speed> <!-- in kbps -->
    ///     <uploaded_bytes>1000</uploaded_bytes> <!-- in bytes -->
    ///     <total_size>50000</total_size> <!-- in bytes -->
    ///     <elapsed>00:00:00</elapsed>
    ///     <meter>0-100</meter> <!-- percentage of upload done -->
    /// </progress>

    [XmlRoot("progress")]
    public class Progress
    {
        [XmlElement("status")]
        public String Status { get; set; }

        [XmlElement("eta")]
        public String Eta { get; set; }

        [XmlElement("elapsed")]
        public String Elapsed { get; set; }

        [XmlElement("speed")]
        public Double Speed { get; set; }

        [XmlElement("uploaded_bytes")]
        public Double UploadedBytes { get; set; }

        [XmlElement("total_size")]
        public Double TotalSize { get; set; }

        [XmlElement("meter")]
        public Double Percent { get; set; }
    }
}

