﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;
using Weverca.ControlFlowGraph;

namespace Weverca.Analysis
{

    delegate ProgramPoint ProgramPointCreator();

    public class ProgramPointGraph
    {
        Dictionary<object, ProgramPoint> _points = new Dictionary<object, ProgramPoint>();

        internal IEnumerable<ProgramPoint> Points { get { return _points.Values; } }
        public readonly ProgramPoint Start;
        public readonly ProgramPoint End;

        internal ProgramPointGraph(BasicBlock entryPoint)
        {
            Start = empty(entryPoint);
            addChildren(Start, entryPoint);

            var endPoints = new List<ProgramPoint>();
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

        private void addChildren(ProgramPoint parent, BasicBlock block)
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

        private ProgramPoint fromDefaultBranch(IEnumerable<Expression> conditionalExpressions,BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.SomeNot, conditionalExpressions.ToArray());
            return fromCondition(assumption, outerBlock);
        }


        private ProgramPoint fromStatement(LangElement statement,BasicBlock outerBlock)
        {            
            return getPoint(statement, () => new ProgramPoint(statement,outerBlock));
        }

        private ProgramPoint fromCondition(Expression condition, BasicBlock outerBlock)
        {
            var assumption = new AssumptionCondition(ConditionForm.All, condition);
            return fromCondition(assumption, outerBlock);
        }

        private ProgramPoint fromCondition(AssumptionCondition condition, BasicBlock outerBlock)
        {            
            return getPoint(condition, () => new ProgramPoint(condition,outerBlock));
        }

        private ProgramPoint empty(object key)
        {
            return getPoint(key, () => new ProgramPoint());
        }

        private ProgramPoint getPoint(object obj, ProgramPointCreator creator)
        {
            ProgramPoint result;

            if (!_points.TryGetValue(obj, out result))
            {
                result = creator();
                _points.Add(obj, result);                
            }
            return result;
        }

        
        public static ProgramPointGraph ForNative(NativeAnalyzer analyzer)
        {
            var basicBlock = new BasicBlock();
            basicBlock.AddElement(analyzer);
            return new ProgramPointGraph(basicBlock);
        }
    }
}