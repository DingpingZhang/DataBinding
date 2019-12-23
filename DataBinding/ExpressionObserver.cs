using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace DataBinding
{
    public static class ExpressionObserver
    {
        internal static DependencyGraph GenerateDependencyGraph<T>(Expression<Func<T>> expression)
        {
            var visitor = new SingleLineLambdaVisitor();
            visitor.Visit(expression);
            return new DependencyGraph(visitor.RootNodes, visitor.ConditionalNodes);
        }

        /// <summary>
        /// Observes an expression and call the <see cref="onValueChanged"/> callback method when it might change.
        /// </summary>
        /// <typeparam name="T">The type represents the return type of an expression.</typeparam>
        /// <param name="expression">The single-line lambda expression is used to be observed.</param>
        /// <param name="onValueChanged">The callback method.</param>
        /// <returns>Returns a token to unbind.</returns>
        public static IDisposable Observes<T>(Expression<Func<T>> expression, Action<T, Exception> onValueChanged)
        {
            var valueGetter = expression.Compile();

            var graph = GenerateDependencyGraph(expression);
            var dependencyRootNodeDisposables = graph.DependencyRootNodes
                .Select(item => item.Initialize(OnPropertyChanged))
                .ToArray();
            var conditionalRootNodeDisposables = graph.ConditionalRootNodes
                .Select(item => item.Initialize())
                .ToArray();

            return Disposable.Create(() =>
            {
                dependencyRootNodeDisposables.ForEach(item => item.Dispose());
                conditionalRootNodeDisposables.ForEach(item => item.Dispose());
            });

            void OnPropertyChanged(object sender, EventArgs e)
            {
                var newValue = valueGetter.TryGet(out var exception);
                onValueChanged?.Invoke(newValue, exception);

                Debug.WriteLine($"[{DateTime.Now}][Value Changed] NewValue = {newValue}");
            }
        }
    }
}
