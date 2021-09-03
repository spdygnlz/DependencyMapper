using System;
using System.IO;
using System.Reflection;
using CommandLine;
using DependencyAnalyzer;

namespace DependencyMapper
{
    public class DependencyMapper
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                var outputFileName = string.Empty;

                BlackList blacklistFiles = new BlackList(); // Use default blacklist
                foreach (var bfile in o.BlacklistFiles)
                {
                    blacklistFiles.Add(bfile);
                }

                ScannerBase scanner = null;

                if (!string.IsNullOrWhiteSpace(o.Directory) && Directory.Exists(o.Directory))
                {
                    scanner = new DirectoryScanner(o.Directory);
                }
                else if (!string.IsNullOrWhiteSpace(o.File) && File.Exists(o.File))
                {
                    scanner = new FileScanner(o.File);
                }
                else if (!string.IsNullOrWhiteSpace(o.RegularValue) && (Directory.Exists(o.RegularValue)))
                {
                    scanner = new DirectoryScanner(o.RegularValue);
                }
                else if (!string.IsNullOrWhiteSpace(o.RegularValue) && File.Exists(o.RegularValue))
                {
                    scanner = new FileScanner(o.RegularValue);
                }

                if (!string.IsNullOrWhiteSpace(o.outputFileName))
                {
                    var fullpath = o.outputFileName;
                    var directory = Path.GetDirectoryName(o.outputFileName);

                    // If no directory was specified, set it to the current exe directory
                    if (string.IsNullOrWhiteSpace(directory))
                    {
                        directory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
                        fullpath = Path.Combine(directory, o.outputFileName);
                    }

                    if (!Path.GetExtension(fullpath).Contains("dgml"))
                    {
                        fullpath = Path.ChangeExtension(fullpath, "dgml");
                    }

                    // If everything looks good, go ahead with this path
                    if (Directory.Exists(directory))
                    {
                        outputFileName = fullpath;
                    }
                    else
                    {
                        Console.WriteLine($"The directory {directory} specified in the output parameter does not exist.  Please specify an existing directory.");
                        Environment.Exit(-1);
                    }
                }

                if (scanner == null)
                {
                    Console.WriteLine("There was an error in the arguments provided.");
                }
                else
                {
                    var assemblies = scanner.LoadAssemblies(recursive: true,
                        (i, i1) => Console.WriteLine($"Adding {i} of {i1} assemblies."));
                    scanner.GenerateDependencyGraph(assemblies, blacklistFiles, outputFileName);

                    Console.WriteLine();
                    Console.WriteLine($"Finished writing dependency graph to {outputFileName}");
                }
            });
        }
    }
}