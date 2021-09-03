using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenSoftware.DgmlTools;
using OpenSoftware.DgmlTools.Builders;
using OpenSoftware.DgmlTools.Model;

namespace DependencyMapper
{
    public abstract class ScannerBase
    {
        public abstract List<Assembly> LoadAssemblies(bool recursive, Action<int, int> progressAction);

        public void GenerateDependencyGraph(List<Assembly> assemblies, BlackList blacklist, string graphFileName = "DependencyGraph.dgml")
        {
            var blacklistFiles = blacklist != null ? blacklist.Assemblies : new HashSet<string>();

            var linksBuilder = new LinksBuilder<Assembly>(x => x.GetReferencedAssemblies()
                .Where(d => !blacklistFiles.Contains(d.Name))
                .Select(d => new Link
                {
                    Source = x.GetName().Name,
                    Target = d.Name
                }));

            var nodeBuilder = new NodeBuilder<Assembly>(x =>
            {
                var node = new Node
                {
                    Id = x.GetName().Name,
                };

                if (!File.Exists(x.Location))
                {
                    node.Category = "MissingCategory";
                    node.Properties.Add("Missing", true);
                }
                else
                {
                    node.Properties.Add("Location", x.Location);
                    node.Properties.Add("Assembly", x);
                }

                return node;
            });

            var graphBuilder = new DgmlBuilder(new IGraphAnalysis[]{new CustomAnalysis()})
            {
                NodeBuilders = new List<NodeBuilder>
                {
                    //nodesBuilder,
                    nodeBuilder
                },
                LinkBuilders = new List<LinkBuilder>
                {
                    linksBuilder
                }
            };

            Category missingCategory = new Category()
                {Background = "#FFE551400", Id = "MissingCategory", Label = "Not Found"};

            var directedGraph = graphBuilder.Build(assemblies);
            directedGraph.Categories.Add(missingCategory);
            directedGraph.WriteToFile(graphFileName);
        }
    }
}