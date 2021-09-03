using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DependencyMapper;

namespace DependencyAnalyzer
{
    public class DirectoryScanner : ScannerBase
    {
        private readonly string _location;

        private readonly Dictionary<string, Assembly> files = new Dictionary<string, Assembly>();

        public DirectoryScanner(string location)
        {
            _location = location;
        }

        public override List<Assembly> LoadAssemblies(bool recursive, Action<int, int> progressAction)
        {
            //TODO: use probing paths and AssemblyResolve like the FileScanner does?

            SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var stringList = Directory.Exists(_location)
                ? Directory
                    .GetFiles(_location, "*.dll", searchOption)
                    .Concat(Directory.GetFiles(_location, "*.exe", searchOption))
                    .ToList()
                : throw new InvalidOperationException("The specified folder doesn't exist.");

            var num = 0;
            foreach (var str in stringList)
            {
                progressAction(++num, stringList.Count);
                try
                {
                    var assembly = Assembly.LoadFile(str);
                    files.Add(str, assembly);
                }
                catch
                {
                    // ignored
                }
            }

            return files.Select(f => f.Value).ToList();
        }
    }
}