﻿using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace TeisterMask.DataProcessor.ExportDto
{
    [XmlType("Task")]
    public class ExportProjectTaskDto
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        

        [XmlElement("Label")]
        [Required]
        public string Label { get; set; }
    }
}
