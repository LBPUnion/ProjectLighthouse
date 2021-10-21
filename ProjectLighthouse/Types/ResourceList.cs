using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types {
    [XmlRoot("resource"), XmlType("resources")]
    public class ResourceList {
        [XmlElement("resource")] 
        public string[] Resources;
    }
}