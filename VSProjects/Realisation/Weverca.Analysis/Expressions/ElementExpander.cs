using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;

using Weverca.Analysis.Memory;
using Weverca.Analysis.ProgramPoints;

namespace Weverca.Analysis.Expressions
{

    delegate void OnPointCreated(object partial, ProgramPointBase point);

    /// <summary>
    /// Expands statement into chain of program points connected with stack connections
    /// Program points are ordered according to Postfix evaluation order
    /// </summary>
    class ElementExpander : TreeVisitor
    {
        private LangElement _statement;

        /// <summary>
        /// Registered program points registered to their lang elements
        /// </summary>
        private readonly Dictionary<LangElement, ProgramPointBase> _programPoints = new Dictionary<LangElement, ProgramPointBase>();

        private readonly LValueFactory _lValueCreator;

        private readonly RValueFactory _rValueCreator;

        private readonly AliasValueFactory _aliasValueCreator;

        private static readonly Type[] FlowOmittedElements = new Type[]{
            typeof(ActualParam),typeof(FormalParam),typeof(DirectTypeRef)
        };

        private ElementExpander()
        {
            _lValueCreator = new LValueFactory(this);
            _rValueCreator = new RValueFactory(this);
            _aliasValueCreator = new AliasValueFactory(this);
        }

        private void Expand(LangElement statement)
        {
            _statement = statement;
            statement.VisitMe(this);
        }

        public static ProgramPointBase ExpandStatement(LangElement statement, OnPointCreated onPointCreated)
        {
            var expander = new ElementExpander();
            expander.Expand(statement);
            var postfix = Converter.GetPostfix(statement);

            createPointsChain(expander, postfix);
            registerCreatedPoints(expander, onPointCreated);

            var first = expander.GetProgramPoint(postfix.GetElement(0));
            return first;
        }

        internal static ProgramPointBase ExpandCondition(AssumptionCondition condition, OnPointCreated onPointCreated)
        {
            var expander = new ElementExpander();
            var chainOrder = new List<LangElement>();
            var expressionParts = new List<RValuePoint>();

            foreach (var postfix in condition.Parts)
            {
                chainOrder.AddRange(postfix);

                var expressionStart = expander.CreateRValue(postfix.SourceElement);
                expressionParts.Add(expressionStart);
            }

            createPointsChain(expander, chainOrder);
            registerCreatedPoints(expander, onPointCreated);


            var last = expander._programPoints[chainOrder.Last()];

            var assumePoint = new AssumePoint(condition, expressionParts);
            onPointCreated(assumePoint, assumePoint);

            last.AddFlowChild(assumePoint);
            var first = expander._programPoints[chainOrder[0]];

            return first;
        }

        private static void createPointsChain(ElementExpander expander, IEnumerable<LangElement> orderedPartialas)
        {
            ProgramPointBase lastPoint = null;
            foreach (var partial in orderedPartialas)
            {
                //skip all flow omitted lang elements
                if (FlowOmittedElements.Contains(partial.GetType()))
                    continue;

                var currentPoint = expander.GetProgramPoint(partial);

                if (lastPoint == null)
                {
                    //there is no parent to add child
                    lastPoint = currentPoint;
                    continue;
                }

                //join statement chain with flow edges in postfix computation order
                if (!currentPoint.FlowChildren.Any())
                {
                    //because of sharing points in some expressions - point is on flow path before this
                    lastPoint.AddFlowChild(currentPoint);
                }
                lastPoint = currentPoint;
            }
        }

        private static void registerCreatedPoints(ElementExpander expander, OnPointCreated onPointCreated)
        {
            foreach (var pair in expander._programPoints)
            {
                onPointCreated(pair.Key, pair.Value);
            }
        }

        private static int getNearestFlowPartial(Postfix postfix, int startIndex)
        {
            while (startIndex < postfix.Length && FlowOmittedElements.Contains(postfix.GetElement(startIndex).GetType()))
                ++startIndex;

            return startIndex;
        }

        private ProgramPointBase GetProgramPoint(LangElement partial)
        {
            return _programPoints[partial];
        }


        #region Program point creation

        internal RValuePoint CreateRValue(LangElement el)
        {
            ProgramPointBase existingPoint;
            if (_programPoints.TryGetValue(el, out existingPoint))
                return existingPoint as RValuePoint;

            var result = _rValueCreator.CreateValue(el);
            _programPoints.Add(el, result);

            return result;
        }

        internal LValuePoint CreateLValue(LangElement el)
        {
            var result = _lValueCreator.CreateValue(el);
            _programPoints.Add(el, result);

            return result;
        }

        internal AliasPoint CreateAliasValue(LangElement el)
        {
            var result = _aliasValueCreator.CreateValue(el);
            _programPoints.Add(el, result);

            return result;
        }

        private void Result(ProgramPointBase point)
        {
            _programPoints.Add(_statement, point);
        }

        /// <summary>
        /// Set result from created rvalue (doesn't add result twice into result)
        /// </summary>
        /// <param name="point"></param>
        private void RValueResult(LangElement el)
        {
            //result is added via creation call
            CreateRValue(el);
        }

        #endregion

        #region TreeVisitor implementation



        #region Global visiting

        public override void VisitGlobalConstantDecl(GlobalConstantDecl x)
        {
            var constantValue = CreateRValue(x.Initializer);

            Result(new ConstantDeclPoint(x, constantValue));
        }

        public override void VisitGlobalStmt(GlobalStmt x)
        {
            var variables = new List<VariableBased>();
            foreach (var varItem in x.VarList)
            {
                var lValue = CreateLValue(varItem);
                var variable = lValue as VariableBased;
                if (variable == null)
                {
                    throw new NotSupportedException("Cannot set given LValue as global variable");
                }

                variables.Add(variable);
            }

            Result(new GlobalStmtPoint(x, variables.ToArray()));
        }

        #endregion

        #region Variable visiting

        public override void VisitItemUse(ItemUse x)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Assign expressions visiting

        public override void VisitRefAssignEx(RefAssignEx x)
        {
            var rOperand = CreateAliasValue(x.RValue);
            var lOperand = CreateLValue(x.LValue);

            Result(new RefAssignPoint(x, lOperand, rOperand));


        }

        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            RValueResult(x);
        }

        #endregion

        #region Function visiting

        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            RValueResult(x);
        }

        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            RValueResult(x);
        }

        public override void VisitFunctionDecl(FunctionDecl x)
        {
            Result(new FunctionDeclPoint(x));
        }

        public override void VisitLambdaFunctionExpr(LambdaFunctionExpr x)
        {
            RValueResult(x);
        }

        #endregion

        public override void VisitElement(LangElement element)
        {
            throw new NotImplementedException("Element is not supported as statement");
        }

        public override void VisitBinaryEx(BinaryEx x)
        {
            RValueResult(x);
        }

        public override void VisitUnaryEx(UnaryEx x)
        {
            RValueResult(x);
        }

        public override void VisitConcatEx(ConcatEx x)
        {
            RValueResult(x);
        }

        public override void VisitIncDecEx(IncDecEx x)
        {
            RValueResult(x);
        }

        public override void VisitArrayEx(ArrayEx x)
        {
            RValueResult(x);
        }

        public override void VisitJumpStmt(JumpStmt x)
        {
            var expression = CreateRValue(x.Expression);
            Result(new JumpStmtPoint(expression, x));
        }

        public override void VisitTypeDecl(TypeDecl x)
        {
            Result(new TypeDeclPoint(x));
        }

        public override void VisitNewEx(NewEx x)
        {
            RValueResult(x);
        }

        public override void VisitIncludingEx(IncludingEx x)
        {
            RValueResult(x);
        }

        public override void VisitForeachStmt(ForeachStmt x)
        {
            var enumeree = CreateRValue(x.Enumeree);


            VariableBased keyVar = null;
            VariableBased valueVar = null;

            if (x.KeyVariable != null)
            {
                keyVar = CreateLValue(x.KeyVariable.Variable) as VariableBased;
            }

            if (x.ValueVariable != null)
            {
                valueVar = CreateLValue(x.ValueVariable.Variable) as VariableBased;
            }

            Result(new ForeachStmtPoint(x, enumeree, keyVar, valueVar));
        }


        public override void VisitEchoStmt(EchoStmt x)
        {
            var parameters = new List<RValuePoint>();
            foreach (var param in x.Parameters)
            {
                parameters.Add(CreateRValue(param));
            }

            Result(new EchoStmtPoint(x, parameters.ToArray()));
        }

        #endregion


        /// <summary>
        /// Visit method for NativeAnalyzer
        /// </summary>
        /// <param name="nativeAnalyzer">Native analyzer</param>
        internal void VisitNative(NativeAnalyzer nativeAnalyzer)
        {
            Result(new NativeAnalyzerPoint(nativeAnalyzer));
        }
    }
}
