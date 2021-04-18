using System.Xml.Serialization;

namespace CarDealer.DTO.Input
{
    [XmlType("Sale")]
    public class SalesInputObject
    {
        [XmlElement("carId")]
        public int CarId { get; set; }

        [XmlElement("customerId")]
        public int CustomerId { get; set; }

        [XmlElement("discount")]
        public decimal Discount { get; set; }

    }

    //<Sale>
     //   <carId>105</carId>
     //   <customerId>30</customerId>
      //  <discount>30</discount>
   //</Sale>
}
