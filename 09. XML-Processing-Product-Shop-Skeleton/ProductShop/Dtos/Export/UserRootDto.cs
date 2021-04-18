using System.Xml.Serialization;
using ProductShop.DataTransferObjects.Output;

namespace ProductShop.Dtos.Export
{
    [XmlType("Users")]
    public class UserRootDto
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("users")]
        public GetUsersWithProductsModel[] Users { get; set; }
    }
}
