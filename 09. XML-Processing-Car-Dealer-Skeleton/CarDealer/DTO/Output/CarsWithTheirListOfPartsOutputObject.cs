using System.Collections.Generic;
using System.Xml.Serialization;

namespace CarDealer.DTO.Output
{
    [XmlType("car")]
    public class CarsWithTheirListOfPartsOutputObject
    {
        [XmlAttribute("make")]
        public string Make { get; set; }

        [XmlAttribute("model")]
        public string Model { get; set; }

        [XmlAttribute("travelled-distance")]
        public long TravelledDistance { get; set; }

        [XmlArray("parts")]
        public PartOutputObject[] Parts { get; set; }
    }
}
