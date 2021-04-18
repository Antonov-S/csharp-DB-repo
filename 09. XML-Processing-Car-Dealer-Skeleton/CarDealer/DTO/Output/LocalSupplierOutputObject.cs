using System.Xml.Serialization;

namespace CarDealer.DTO.Output
{
    [XmlType("suplier")]
    public class LocalSupplierOutputObject
    {
        [XmlAttribute("id")]
        public int Id { get; set; }


        [XmlAttribute("name")]
        public string Name { get; set; }


        [XmlAttribute("parts-count")]
        public int PartCount { get; set; }

    }
}
