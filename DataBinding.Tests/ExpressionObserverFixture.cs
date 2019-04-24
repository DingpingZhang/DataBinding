using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace DataBinding.Tests
{
    public class ExpressionObserverFixture
    {
        [Fact]
        public void ConditionalExpressionActivateAndInactivate()
        {
            var a = InitializeComplexTypeInstance();

            var result = string.Empty;
            var count = 0;

            ExpressionObserver.Observes(
                () => a.BoolProp ? a.NestedProp.StringProp : a.StringProp,
                (value, exception) =>
                {
                    Assert.Null(exception);
                    result = value;
                    count++;
                });

            var ifFalseTestCases = new[] { "11", "22", "33" };
            var ifTrueTestCases = new[] { "1", "2", "3", "4" };
            var ifTrueNestedPropTestCases = new[]
            {
                new ComplexType {StringProp = "111"},
                new ComplexType {StringProp = "222"},
                new ComplexType {StringProp = "333"},
                new ComplexType {StringProp = "444"},
                new ComplexType {StringProp = "555"},
            };

            // Initial Test = false
            for (int i = 0; i < 5; i++)
            {
                count = 0;

                foreach (string testCase in ifFalseTestCases)
                {
                    a.StringProp = testCase;

                    Assert.Equal(a.BoolProp ? ifTrueNestedPropTestCases.Last().StringProp : testCase, result);
                }

                Assert.Equal(a.BoolProp ? 0 : ifFalseTestCases.Length, count);

                // -------------------------------------------------------

                count = 0;

                foreach (string testCase in ifTrueTestCases)
                {
                    a.NestedProp.StringProp = testCase;

                    Assert.Equal(a.BoolProp ? testCase : ifFalseTestCases.Last(), result);
                }

                Assert.Equal(a.BoolProp ? ifTrueTestCases.Length : 0, count);

                // -------------------------------------------------------

                count = 0;

                foreach (ComplexType testCase in ifTrueNestedPropTestCases)
                {
                    a.NestedProp = testCase;

                    Assert.Equal(a.BoolProp ? testCase.StringProp : ifFalseTestCases.Last(), result);
                }

                Assert.Equal(a.BoolProp ? ifTrueNestedPropTestCases.Length : 0, count);

                // -------------------------------------------------------

                a.BoolProp = !a.BoolProp;

                Assert.Equal(a.BoolProp ? ifTrueNestedPropTestCases.Last().StringProp : ifFalseTestCases.Last(), result);
            }
        }

        [Fact]
        public void MultiNestedConditionalExpressionActivateAndInactivate()
        {
            var a = InitializeComplexTypeInstance();
            var complexTypeTestCases = new[]
            {
                new ComplexType{IntProp = 111},
                new ComplexType{IntProp = 222},
                new ComplexType{IntProp = 333},
                new ComplexType{IntProp = 444},
                new ComplexType{IntProp = 555},
            };

            foreach (var testCase in complexTypeTestCases)
            {
                a.ComplexList.Add(testCase);
            }

            var result = int.MinValue;
            var count = 0;

            ExpressionObserver.Observes(
                () => a.NestedProp.BoolProp
                    ? a.NestedProp.IntProp
                    : a.BoolProp
                        ? a.IntProp
                        : a.ComplexList[a.IntProp].IntProp,
                (value, exception) =>
                {
                    Assert.Null(exception);
                    result = value;
                    count++;
                });

            // 1. False False: a.ComplexList[a.IntProp].IntProp
            for (int i = 1; i < a.ComplexList.Count; i++)
            {
                a.IntProp = i;
                Assert.Equal(a.ComplexList[i].IntProp, result);

                for (int j = 0; j < 5; j++)
                {
                    var expected = a.ComplexList[i].IntProp = j + 10086;
                    Assert.Equal(expected, result);
                }

                Assert.Equal(i * 6, count);
            }

            // 2. False True: a.IntProp
            result = int.MinValue;
            count = 0;

            a.BoolProp = true;

            Assert.Equal(a.IntProp, result);
            Assert.Equal(1, count);

            for (int i = 0; i < 5; i++)
            {
                a.ComplexList.Last().IntProp = i + 10086;
                Assert.Equal(a.IntProp, result);
            }

            Assert.Equal(1, count);

            // 3. True True: a.NestedProp.IntProp
            result = int.MinValue;
            count = 0;

            a.NestedProp.BoolProp = true;

            Assert.Equal(a.NestedProp.IntProp, result);
            Assert.Equal(1, count);

            for (int i = 0; i < 5; i++)
            {
                a.NestedProp.IntProp = i + 10086;
                Assert.Equal(a.NestedProp.IntProp, result);
            }

            Assert.Equal(6, count);

            // 4. True True/False: a.BoolProp ? a.IntProp : a.ComplexList[a.IntProp].IntProp
            result = int.MinValue;
            count = 0;

            for (int i = 0; i < 5; i++)
            {
                a.BoolProp = !a.BoolProp;
                a.IntProp = i + 10086;
                a.ComplexList.Last().IntProp = i + 10086;

                Assert.Equal(int.MinValue, result);
            }

            Assert.Equal(0, count);
        }

        private static ComplexType InitializeComplexTypeInstance()
        {
            return new ComplexType
            {
                StringProp = "a.StringProp",
                ComplexList = new ObservableCollection<ComplexType>(),
                NestedProp = new ComplexType
                {
                    StringProp = "NestedProp.StringProp",
                    NestedProp = new ComplexType()
                }
            };
        }
    }
}
