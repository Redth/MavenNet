using System;
using System.Collections.Generic;
using System.Text;

namespace MavenNet.Models.DevOps
{

    public class Packages
    {
        public int count { get; set; }
        public Package[] value { get; set; }
    }

    public class Package
    {
        public string name { get; set; }

        public string groupId => name?.Substring(0, name.IndexOf(":"));
        public string artifactId => name?.Substring(name.IndexOf(":")+1);

        public string url { get; set; }
        public Version[] versions { get; set; }
    }

    public class Version
    {
        public string version { get; set; }
        public DateTime publishDate { get; set; }
    }
}
