using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

namespace Weverca.AnalysisFramework
{

    delegate ProgramPoint<FlowInfo> ProgramPointCreator<FlowInfo>();

    public class ProgramPointGraph<FlowInfo>
    {
        Dictionary<object, ProgramPoint<FlowInfo>> _points = new Dictionary<object, ProgramPoint<FlowInfo>>();

        internal IEnumerable<ProgramPoint<FlowInfo>> Points { get { return _points.Values; } }
        public readonly ProgramPoint<FlowInfo> Start;
        public readonly ProgramPoint<FlowInfo> End;

        internal ProgramPointGraph(BasicBlock entryPoint)
        {
            Start = empty(entryPoint);
            addChildren(Start, entryPoint);

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

            End = empty(Start);
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
                if (edge is ConditionalEdge)
                {
                    var condition = fromCondition((edge as ConditionalEdge).Condition, block);
                    addChildren(condition, edge.To);
                    current.AddChild(condition);
                }
                else 
                {
                //foreach edge
                }
            }

            if (block.DefaultBranch != null)
            {
                var conditionExpressions = from edge in block.OutgoingEdges where edge is ConditionalEdge select (edge as ConditionalEdge).Condition;

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
            var assumption = new AssumptionCondition_deprecated(ConditionForm.SomeNot, conditionalExpressions.ToArray());
            return fromCondition(assumption, outerBlock);
        }


        private ProgramPoint<FlowInfo> fromStatement(LangElement statement,BasicBlock outerBlock)
        {            
            return getPoint(statement, () => new ProgramPoint<FlowInfo>(statement,outerBlock));
        }

        private ProgramPoint<FlowInfo> fromCondition(Expression condition, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition_deprecated(ConditionForm.All, condition);
            return fromCondition(assumption, outerBlock);
        }

        private ProgramPoint<FlowInfo> fromCondition(AssumptionCondition_deprecated condition, BasicBlock outerBlock)
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
