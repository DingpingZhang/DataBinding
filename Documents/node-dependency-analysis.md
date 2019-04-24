# 数据绑定 依赖图 分析

## 1. 公理

> 1. 依赖图的点（Node）：`必要不充分条件`：`Inpc` == 有能力监听子节点；`Prop` == 有能力报告自身变更。
> 2. 依赖图的边（Edge）：即点的依赖关系，每一条边必然是：`Inpc -> Prop`。

## 2. 推论

1. 由`公理1`及`公理2`可得：
   1. 依赖图中**有且仅有三种点**：
      - `(Inpc, Filed|Prop)` - 根节点（`Root-Node`）：对应的实例为常量；
      - `(Inpc, Prop)` - 中继节点（`Relay-Node`）：对应的实例为变量；
      - `(Any, Prop)` - 叶节点（`Leaf-Node`）：没有对应的实例，其仅提供`PropertyName`。
   2. 故**有且仅有四种边**：
      - `(Inpc, Filed|Prop) -> (Inpc, Prop)`；
      - `(Inpc, Filed|Prop) -> (Any, Prop)`；
      - `(Inpc, Prop) -> (Inpc, Prop)`；
      - `(Inpc, Prop) -> (Any, Prop)`。
2. 节点可能扮演以下角色：
   1. 监听：监听子节点的变更，并通知重新计算表达式；
   2. 刷新：通知当前发生变更的子节点做刷新；
   3. 广播：当自身发生刷新时，通知所有`Inpc`子节点做刷新。
3. 各节点分析：
   - 根节点 - [监听，广播]，**根节点被默认为表达式中的常量**，不会被更新，其只有两种情况： 1. 来自普通的类：通过`ConstantExpression`获取； 2. 来自闭包：通过`MemeberExpression (Member.MemberType == MemberTypes.Field)`获取，其父类一定是`(<>__DisplayClass)`；
   - 中继节点 - [监听，刷新，广播]：扮演所有角色；
   - 叶节点 - [无]：叶节点的存在仅为其父节点提供一个`PropertyName`，什么也不必负责。

## 3. 条件表达式树

条件表达式树独立于依赖节点图，负责控制依赖节点的激活与失活。

当一个Owner在监听到其Property Changed时，应当尝试刷新该Property依赖节点（Test）所对应的

递归通知子树更新

节点添加规则：

1. Test中的节点与条件表达式同级；它的激活不受本条件表达式的影响（可能有上游表达式），若一个节点以Test的身份被加入，将其放置在TestNodes中，并删除IfTrueNodes或IfFalseNodes中的该节点，并记录；
2. 若某节点同时存在于IfTrue和IfFalse中

## 3. 特殊情况分析

1. 理想属性链：`Inpc->[(Inpc&Prop)->]->Prop`，除去末节点，每个都是`Inpc`；除去根节点，每个都是`Prop`。

2. 断裂属性链：不完全满足（1）中的情况：尽可能多的观测可观测对象。

   1. 出现 Filed 导致断裂。**不允许中继节点为不可观测对象**，否则该节点被更新而后继不知道，无法解绑 Handler，岂不是内存泄露。
   2. 出现函数导致断裂，包括：二元表达式、`CallExpr`、索引器等。函数会返回一个虚拟的节点，该虚拟节点的更新是可以被观测的，它依赖函数的 Owner 及参数列表。

   若后续出现`[(Inpc&Prop)]`，将跳过中断部分，被挂接到前面的节点上，但标记为`间接依赖`，更新随父节点影响，但不作为其父节点的观测子节点（`PropertyName`无效）。

3. 条件表达式：内含三部分：`Test`、`IfTrue`、`IfFalse`。需要分别对三部分的节点进行标记（来自哪个表达式，属于该表达式的哪个部分）。其中：

   1. `Test`和`IfTrue & IfFalse`：和普通节点（`Normal`）等效，自身更新触发计算整个表达式；
   2. `IfTrue | IfFalse`：以当前`Test`计算值为准，决定是否重新计算整个表达式，当然无论如何，自己依赖链的更新是一定要做的。

4. 节点更新时，触发重新计算表达式的模式：

```csharp
public enum RecalculateMode
{
      IfTure = 1,  // 选择触发重新计算，Test为True时
      IfFalse = 2, // 选择触发重新计算，Test为False时
      Normal = 3,  // 每次都触发重新计算
      Test = 7,    // 每次触发重新计算，且计算Test表达式
}
```

## 4. 实现细节

1. 借助`ExpressionVisitor`遍历表达式，可在`VisitMember()`中得到`Node-Expr`及`Node-Owner-Expr`，创建`Node`并通过`Context`传输到上层。
2. 那么，此时在每一层可获得的信息有：`Node-Owner-Expr-Type`、`Node`、`Child`，

```csharp
if (OwnerNode.Type == Inpc &&
   (Node.Member.MemberType == Prop ||
   Node.Member.MemberType == Field && ChildNode == null)) // 叶节点
   {
         // Create Node
   }
```

## 5. 表达式类型分析：

| operator           | example              |
| ------------------ | -------------------- |
| Add                | +                    |
| AddChecked         | +                    |
| And                | &                    |
| AndAlso            | &&                   |
| ArrayLength        | array.Length         |
| ArrayIndex         | array[index]         |
| Call               | Method() / important |
| Coalesce           | ??                   |
| Conditional        | ?:                   |
| Constant           | const                |
| Convert            | (type)instance       |
| ConvertChecked     | (type)instance       |
| Divide             | /                    |
| Equal              | ==                   |
| ExclusiveOr        | ^                    |
| GreaterThan        | >                    |
| GreaterThanOrEqual | >=                   |
| Invoke             | ignore               |
| Lambda             | ignore               |
| LeftShift          | <<                   |
| LessThan           | <                    |
| LessThanOrEqual    | <=                   |
| ListInit           | ignore               |
| MemberAccess       | important            |
| MemberInit         | ignore               |
| Modulo             | %                    |
| Multiply           | \*                   |
| MultiplyChecked    | \*                   |
| Negate             | -a                   |
| UnaryPlus          | +a                   |
| NegateChecked      | -a                   |
| New                | new                  |
| NewArrayInit       | ignore               |
| NewArrayBounds     | ignore               |
| Not                | !                    |
| NotEqual           | !=                   |
| Or                 | \|                   |
| OrElse             | \|\|                 |
| Parameter          |
| Power              | ignore               |
| Quote              | ignore               |
| RightShift         | >>                   |
| Subtract           | a - b                |
| SubtractChecked    | -                    |
| TypeAs             | as                   |
| TypeIs             | is                   |
| DebugInfo          | ignore               |
| Decrement          | (a - 1)              |
| Dynamic            | ignore               |
| Default            | ignore               |
| Extension          | ignore               |
| Increment          | (a + 1)              |
| Index              | ignore               |
| Label              | ignore               |
| RuntimeVariables   | ignore               |
| Unbox              | ignore               |
| TypeEqual          | ignore               |
| OnesComplement     | ~                    |
| IsTrue             | true                 |
| IsFalse            | false                |

// Binary expression:

// Invalid expression term:
Loop,
Switch,
Throw,
Try,
Goto,
Block,

// An expression tree cannot contains an assignment operator:
Assign,
AddAssign,
AndAssign,
DivideAssign,
ExclusiveOrAssign,
LeftShiftAssign,
ModuloAssign,
MultiplyAssign,
OrAssign,
PowerAssign,
RightShiftAssign,
SubtractAssign,
AddAssignChecked,
MultiplyAssignChecked,
SubtractAssignChecked,
PreIncrementAssign,
PreDecrementAssign,
PostIncrementAssign,
PostDecrementAssign,
