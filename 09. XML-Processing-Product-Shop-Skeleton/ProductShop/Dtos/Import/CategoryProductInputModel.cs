﻿using System.Xml.Serialization;

namespace ProductShop.Dtos.Import
{
    [XmlType("CategoryProduct")]
    public class CategoryProductInputModel
    {
        [XmlElement("CategoryId")]
        public int CategoryId { get; set; }

        [XmlElement("ProductId")]
        public int ProductId { get; set; }
    }

    //<CategoryProduct>
        //<CategoryId>4</CategoryId>
        //<ProductId>1</ProductId>
    //</CategoryProduct>
}