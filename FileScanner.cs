using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using DependencyAnalyzer;

namespace DependencyMapper
{
    public class FileScanner : ScannerBase
    {
        private readonly string _filePath;

        private List<string> _privatePaths = new List<string>() {""};

        public FileScanner(string filePath)
        {
            _filePath = filePath;
            if (File.Exists(_filePath) && File.Exists(_filePath + ".config"))
            {
                var paths = GetProbingPaths(_filePath + ".config");

                _privatePaths.AddRange(paths);
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private List<string> GetProbingPaths(string filename)
        {
            ConfigXmlDocument c = new ConfigXmlDocument();
            c.Load(filename);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(c.NameTable);
            nsmgr.AddNamespace("bk", "urn:schemas-microsoft-com:asm.v1");


            var probingPaths = c.SelectSingleNode(".//runtime/bk:assemblyBinding/bk:probing/@privatePath", nsmgr).Value;

            return probingPaths.Split(';').ToList();
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblies= ((AppDomain) sender).GetAssemblies();

            string folderPath = Path.GetDirectoryName(_filePath);
            
            if (TryGetFileInProbingPath(folderPath, _privatePaths, new AssemblyName(args.Name).Name, out string assemblyPath))
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }

            return null;
        }

        private bool TryGetFileInProbingPath(string rootPath, List<string> probingPaths, string filename, out string finalPath)
        {
            foreach (var subdir in probingPaths)
            {
                var pathToTry = Path.Combine(rootPath, subdir, filename + ".dll");
                if (File.Exists(pathToTry))
                {
                    finalPath = pathToTry;
                    return true;
                }
            }

            finalPath = string.Empty;
            return false;
        }

        public override List<Assembly> LoadAssemblies(bool recursive, Action<int, int> progressAction)
        {
            List<Assembly> assemblies = new List<Assembly>();

            var currentDir = Path.GetDirectoryName(_filePath);
            Directory.SetCurrentDirectory(currentDir);

            var rootAssembly = Assembly.LoadFrom(_filePath);
            var name = rootAssembly.GetName();
            LoadAssembly(name, assemblies);

            return assemblies;
        }

        private void LoadAssembly(AssemblyName assemblyName, List<Assembly> loaded)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                if (loaded.Contains(assembly)) return;

                loaded.Add(assembly);
                var dependencies = assembly.GetReferencedAssemblies();
                foreach (var dep in dependencies)
                {
                    //var directory = Path.GetDirectoryName(assembly.CodeBase);
                    //dep.CodeBase = $@"{directory}\{dep.Name}.dll";
                    LoadAssembly(dep, loaded);
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException or BadImageFormatException)
            {

            }


        }
    }
}