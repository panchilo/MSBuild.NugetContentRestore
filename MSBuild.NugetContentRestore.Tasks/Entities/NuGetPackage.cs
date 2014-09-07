using System.Collections.Generic;
using System.Xml.Serialization;

namespace MSBuild.NugetContentRestore.Tasks.Entities
{
    [XmlRoot("packages")]  
    public class NuGetPackageConfiguration
    {

        #region Constructor

        public NuGetPackageConfiguration()
        {
            Packages = new List<Package>();
        }

        #endregion

        #region Public Properties

        [XmlElement("package")]
        public List<Package> Packages { get; set; }

        #endregion

    }

    #region Nested Class (Package)

    public class Package
    {
        
        #region Properties

        [XmlAttribute("id")]
        public string Name { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("targetFramework")]
        public string TargetFrameword { get; set; }

        public string FolderName { get { return string.Format("{0}.{1}", Name, this.Version); } }

        #endregion
        
    }

    #endregion

}
