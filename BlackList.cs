using System.Collections.Generic;

namespace DependencyMapper
{
    public class BlackList
    {
        private HashSet<string> _blacklist = new HashSet<string>
        {
            "System",
            "mscorlib",
            "System.Core",
            "System.Data",
            "System.Data.DataSetExtensions",
            "System.Net.Http",
            "System.Xml",
            "System.Xml.Linq",
            "System.Drawing",
            "System.ValueTuple"
        };

        public HashSet<string> Assemblies => _blacklist;

        public void Add(string assembly)
        {
            _blacklist.Add(assembly);
        }
    }
}