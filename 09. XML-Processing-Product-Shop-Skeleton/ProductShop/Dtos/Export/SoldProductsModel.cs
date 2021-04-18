using ProductShop.DataTransferObjects.Output;
using ProductShop.DataTransferObjects.Output.GetSoldProductsDto;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    [XmlType("SoldProducts")]
    public class SoldProductsModel
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("products")]
        public ProductModel[] Products { get; set; }
    }
}
