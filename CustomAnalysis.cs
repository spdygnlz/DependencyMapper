using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenSoftware.DgmlTools.Model;

namespace DependencyMapper
{
    public class CustomAnalysis : IGraphAnalysis
    {
        public void Execute(DirectedGraph graph)
        {
            foreach (var link in graph.Links)
            {
                if (graph.Nodes.All(x => x.Id != link.Source)) graph.Nodes.Add(new Node {Id = link.Source});

                if (graph.Nodes.All(x => x.Id != link.Target)) graph.Nodes.Add(new Node {Id = link.Target});
            }

            foreach (var node in graph.Nodes)
            {
                if (node.Properties.ContainsKey("Assembly") &&
                    File.Exists(((Assembly) node.Properties["Assembly"]).Location))
                {
                    node.Category = "Found";
                }
                else
                {
                    node.Category = "MissingCategory";
                }
            }
        }

        public IEnumerable<Property> GetProperties(DirectedGraph graph)
        {
            return graph.Properties;
        }

        public IEnumerable<Style> GetStyles(DirectedGraph graph)
        {
            var missingCategoryStyle = new Style
            {
                Condition = new List<Condition> {new Condition {Expression = "HasCategory('MissingCategory')"}},
                GroupLabel = "Missing Category",
                Setter = new List<Setter> {new Setter {Property = "Background", Value = "#FFE51400"}},
                TargetType = "Node",
                ValueLabel = "Has category"
            };

            return new[] {missingCategoryStyle};
        }
    }
}