using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
namespace Weverca.ControlFlowGraph.Analysis
{

    delegate ProgramPoint<FlowInfo> ProgramPointCreator<FlowInfo>();

    public class ProgramPointGraph<FlowInfo>
    {
        Dictionary<object, ProgramPoint<FlowInfo>> _points = new Dictionary<object, ProgramPoint<FlowInfo>>();

        internal IEnumerable<ProgramPoint<FlowInfo>> Points { get { return _points.Values; } }
        public readonly ProgramPoint<FlowInfo> Root;
        public readonly ProgramPoint<FlowInfo> End;

        internal ProgramPointGraph(ControlFlowGraph method)
        {
            Root = empty(method.start);
            addChildren(Root, method.start);

            var endPoints = new List<ProgramPoint<FlowInfo>>();
            foreach (var point in _points.Values)
            {
                if (point.Children.Any())
                    continue;

                endPoints.Add(point);
            }

            if (endPoints.Count == 0)
            {
                //End point is not reachable
                End = null;
                return;
            }

            End = empty(Root);
            foreach (var endPoint in endPoints)
            {
                endPoint.AddChild(End);
            }
        }

        private void addChildren(ProgramPoint<FlowInfo> parent, BasicBlock block)
        {
            var current = parent;
            foreach (var stmt in block.Statements)
            {
                //create chain of program points
                var child = fromStatement(stmt,block);
                current.AddChild(child);
                current = child;
            }

            
            foreach (var edge in block.OutgoingEdges)
            {
                var condition = fromCondition(edge.Condition,block);
                addChildren(condition, edge.To);
                current.AddChild(condition);
            }

            if (block.DefaultBranch != null)
            {
                var conditionExpressions = from edge in block.OutgoingEdges select edge.Condition;

                if (conditionExpressions.Any())
                {
                    var defaultPoint = fromDefaultBranch(conditionExpressions, block);
                    addChildren(defaultPoint, block.DefaultBranch.To);
                    current.AddChild(defaultPoint);
                }
                else
                {
                    //there is no condition on edge
                    addChildren(current, block.DefaultBranch.To);
                }
            }
        }

        private ProgramPoint<FlowInfo> fromDefaultBranch(IEnumerable<Expression> conditionalExpressions,BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.SomeNot, conditionalExpressions.ToArray());
            return fromCondition(assumption, outerBlock);
        }


        private ProgramPoint<FlowInfo> fromStatement(LangElement statement,BasicBlock outerBlock)
        {            
            return getPoint(statement, () => new ProgramPoint<FlowInfo>(statement,outerBlock));
        }

        private ProgramPoint<FlowInfo> fromCondition(Expression condition, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.All, condition);
            return fromCondition(assumption, outerBlock);
        }

        private ProgramPoint<FlowInfo> fromCondition(AssumptionCondition condition, BasicBlock outerBlock)
        {            
            return getPoint(condition, () => new ProgramPoint<FlowInfo>(condition,outerBlock));
        }

        private ProgramPoint<FlowInfo> empty(object key)
        {
            return getPoint(key, () => new ProgramPoint<FlowInfo>());
        }

        private ProgramPoint<FlowInfo> getPoint(object obj, ProgramPointCreator<FlowInfo> creator)
        {
            ProgramPoint<FlowInfo> result;

            if (!_points.TryGetValue(obj, out result))
            {
                result = creator();
                _points.Add(obj, result);                
            }
            return result;
        }



        
    }
}
