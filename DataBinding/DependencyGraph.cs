﻿using System.Collections.Generic;

namespace DataBinding
{
    internal class DependencyGraph
    {
        public IReadOnlyCollection<DependencyNode> DependencyRootNodes { get; }

        public IReadOnlyCollection<ConditionalNode> ConditionalRootNodes { get; }

        public DependencyGraph(
            IReadOnlyCollection<DependencyNode> dependencyNodes,
            IReadOnlyCollection<ConditionalNode> conditionalNodes)
        {
            DependencyRootNodes = dependencyNodes;
            ConditionalRootNodes = conditionalNodes;
        }
    }
}
