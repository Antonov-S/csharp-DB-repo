using System.Xml.Serialization;

namespace CarDealer.DTO.Input
{
    [XmlType("Part")]
    public class PartInputObject
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("price")]
        public decimal Price { get; set; }

        [XmlElement("quantity")]
        public int Quantity { get; set; }

        [XmlElement("supplierId")]
        public int SupplierId { get; set; }

    }

    //<name>Exposed bumper</name>
     //   <price>1400.34</price>
     //   <quantity>10</quantity>
      //  <supplierId>13</supplierId>
}
