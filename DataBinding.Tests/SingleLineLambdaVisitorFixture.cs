using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DataBinding.Tests
{
    public class SingleLineLambdaVisitorFixture : BindableBase
    {
        private ComplexType _propertyComplex;

        public ComplexType PropertyComplex
        {
            get => _propertyComplex;
            set => SetProperty(ref _propertyComplex, value);
        }

        [Fact]
        public void SmokeTest()
        {
            var a = new ComplexType();
            var b = new ComplexType();
            var c = new ComplexType();
            var d = new ComplexType();

            var graph1 = ExpressionObserver.GenerateDependencyGraph(() => a.NestedProp.BoolProp
                ? (b.BoolProp
                    ? GetComplexTypeStatic(a, b, c.NestedProp, d.IntProp).NestedProp.NestedProp
                    : a.ComplexList[b.NestedProp.IntProp].NestedProp.NestedProp).NestedProp
                : b.NestedProp.NestedProp);
            var graph2 = ExpressionObserver.GenerateDependencyGraph(
                () => GetComplexTypeStatic(a, b, c.NestedProp, d.IntProp).NestedProp.IntProp);
            var graph3 = ExpressionObserver.GenerateDependencyGraph(
                () => a.ComplexList[b.NestedProp.IntProp + c.IntProp].NestedProp);
            var graph4 = ExpressionObserver.GenerateDependencyGraph(
                () => (PropertyComplex + b).IntProp);
            var graph5 = ExpressionObserver.GenerateDependencyGraph(
                () => c.IListProperty[(a.NestedProp + c.NestedProp).NestedProp.IntProp % b.NestedProp.IntProp]);
            var graph6 = ExpressionObserver.GenerateDependencyGraph(
                () => a.BoolProp ? "asd" : "asd");

            var data1 = GetDrawData(graph1.DependencyRootNodes);
            var data2 = GetDrawData(graph2.DependencyRootNodes);
            var data3 = GetDrawData(graph3.DependencyRootNodes);
            var data4 = GetDrawData(graph4.DependencyRootNodes);
            var data5 = GetDrawData(graph5.DependencyRootNodes);
        }

        private string GetDrawData(IReadOnlyCollection<DependencyNode> dependencyNodes)
        {
            var elements = new JObject();
            var nodes = new JArray();
            var edges = new JArray();

            foreach (var dependencyNode in dependencyNodes)
            {
                nodes.Add(new JObject { { "data", new JObject { { "id", $"{dependencyNode}" } } } });
                PopulateData(dependencyNode, nodes, edges);
            }

            elements.Add("nodes", nodes);
            elements.Add("edges", edges);

            return elements.ToString(Formatting.Indented);
        }

        private void PopulateData(DependencyNode owner, JArray nodes, JArray edges)
        {
            foreach (var dependencyNode in owner.DownstreamNodes)
            {
                nodes.Add(new JObject { { "data", new JObject { { "id", $"{dependencyNode}" } } } });
                edges.Add(new JObject
                {
                    {
                        "data", new JObject
                        {
                            {"id", $"{owner}-{dependencyNode}"},
                            {"weight", 1},
                            {"source", $"{owner}"},
                            {"target", $"{dependencyNode}"}
                        }
                    }
                });

                PopulateData(dependencyNode, nodes, edges);
            }
        }

        [Fact]
        public void ParseIndirectlyDependencyGraph()
        {
            var a = new ComplexType();
            var b = new ComplexType();
            var c = new ComplexType();
            var d = new ComplexType();

            Func<ComplexType> lam = () => a;

            //var graph12 = SingleLineLambdaVisitor.Parse(() => lam.Invoke().NestedProp is object);
            //var graph = SingleLineLambdaVisitor.Parse(() => ~a.IntProp);

            //var graph0 = SingleLineLambdaVisitor.Parse(() => a.ComplexList[b.IntProp].NestedProp);

            //var graph1 = SingleLineLambdaVisitor.Parse(() => (a + b).IntProp);
            //var graph2 = SingleLineLambdaVisitor.Parse(() => (a.NestedProp + b).IntProp);
            //var graph3 = SingleLineLambdaVisitor.Parse(() => (a.NestedProp + b.NestedProp).NestedProp.IntProp);
            //var graph4 = SingleLineLambdaVisitor.Parse(() => (a.NestedProp + a.NestedProp).NestedProp.IntProp);
            //var graph5 = SingleLineLambdaVisitor.Parse(() => GetComplexTypeStatic(a, b, c, d).NestedProp.IntProp);
            //var graph6 = SingleLineLambdaVisitor.Parse(() => GetComplexType(PropertyComplex, b, c, d).NestedProp.IntProp);

            // Check the count of the tree node.
            //Assert.Empty(graph1);
            //Assert.Equal(1, graph2.Count);
            //Assert.Equal(5, graph3.Count);
            //Assert.Equal(3, graph4.Count);
            //Assert.Equal(5, graph5.Count);
            //Assert.Equal(6, graph6.Count);

            // Check each nodes
            var expectedGraph2 = new (string Expression, int Count)[] { ("(NestedProp)", 1) };
            var expectedGraph3 = new (string Expression, int Count)[]
            {
                ("(IntProp)", 1),
                ("(NestedProp) -> [() -> [(IntProp)]]", 2),
                ("() -> [(IntProp)]", 2),
            };
            var expectedGraph4 = new (string Expression, int Count)[]
            {
                ("(IntProp)", 1),
                ("(NestedProp) -> [() -> [(IntProp)]]", 1),
                ("() -> [(IntProp)]", 1)
            };
            var expectedGraph5 = new (string Expression, int Count)[]
            {
                ("(IntProp)", 1),
                ("() -> [(IntProp)]", 4)
            };
            var expectedGraph6 = new (string Expression, int Count)[]
            {
                ("(IntProp)", 1),
                ("(PropertyComplex) -> [(IntProp), () -> [(IntProp)]]", 1),
                ("() -> [(IntProp)]", 4)
            };

            //CheckGraph(expectedGraph2, graph2);
            //CheckGraph(expectedGraph3, graph3);
            //CheckGraph(expectedGraph4, graph4);
            //CheckGraph(expectedGraph5, graph5);
            //CheckGraph(expectedGraph6, graph6);
        }

        [Fact]
        public void ParseDirectlyDependencyGraph()
        {

        }

        [Fact]
        public void ParseConditionalExpression()
        {

        }

        //private static void CheckGraph(IEnumerable<(string Expression, int Count)> expected, IEnumerable<PropertyDependencyNode> actual)
        //{
        //    var graphNodeStrings = actual.Select(item => item.ToString());
        //    foreach ((string expression, int count) in expected)
        //    {
        //        Assert.Equal(count, graphNodeStrings.Count(item => item == expression));
        //    }
        //}

        private static ComplexType GetComplexTypeStatic(ComplexType a, ComplexType b, ComplexType c, int d)
        {
            return new ComplexType();
        }

        private ComplexType GetComplexType(ComplexType a, ComplexType b, ComplexType c, ComplexType d)
        {
            return new ComplexType();
        }

        //private static XElement ToXElement(IEnumerable<PropertyDependencyNode> trees)
        //{
        //    var root = new XElement("Root");
        //    foreach (var tree in trees)
        //    {
        //        ToXElement(tree, root);
        //    }
        //    return root;
        //}

        //private static void ToXElement(PropertyDependencyNode node, XElement owner)
        //{
        //    var element = new XElement("Inpc");
        //    element.Add(new XAttribute("Props", "{" + string.Join(", ", node.PropertyNames) + "}"));
        //    foreach (var child in node.Children)
        //    {
        //        ToXElement(child, element);
        //    }
        //    owner.Add(element);
        //}
    }
}
