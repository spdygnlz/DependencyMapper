using System.Collections.Generic;
using CommandLine;

namespace DependencyMapper
{
    public class Options
    {
        [Option('d', "directory", Required = false, HelpText = "A directory to scan for dependencies.  e.g. C:\\Builds\\Bin")]
        public string Directory{ get; set; }

        [Option('f', "file", Required = false, HelpText = "A file to scan for dependencies.  e.g. C:\\Builds\\Bin\\System1.exe")]
        public string File { get; set; }

        [Option('b', "blacklist", Required = false, HelpText = "A list of assemblies to ignore.  e.g. mscorlib")]
        public IEnumerable<string> BlacklistFiles { get; set; }

        [Option('o', "output", Required = false, Default = "DependencyMap.dgml", HelpText = "A path and filename for the output.  Defaults to DependencyMap.dgml in the folder of the DependencyMapper.exe.")]
        public string outputFileName { get; set; }

        [Value(0, Required = false, HelpText = "Either a file or a directory to scan for dependencies.  e.g. C:\\Builds\\Bin or C:\\Builds\\Bin\\System1.exe")]
        public string RegularValue { get; set; }
    }
}