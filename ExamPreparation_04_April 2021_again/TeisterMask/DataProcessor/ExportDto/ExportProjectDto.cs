using System.Xml.Serialization;

namespace TeisterMask.DataProcessor.ExportDto
{
    [XmlType("Project")]
    public class ExportProjectDto
    {
        [XmlElement("ProjectName")]
        public string ProjectName { get; set; }


        [XmlElement("HasEndDate")]
        public string HasEndDate { get; set; }


        [XmlAttribute("TasksCount")]
        public string TasksCount { get; set; }


        [XmlArray("Tasks")]
        public ExportProjectTaskDto[] Tasks { get; set; }

    }
}
