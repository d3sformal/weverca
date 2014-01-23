using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework.Expressions
{
    internal delegate void OnPointCreated(ProgramPointBase point);

    /// <summary>
    /// Expands statement into chain of program points connected with stack connections
    /// Program points are ordered according to Postfix evaluation order
    /// </summary>
    internal class ElementExpander : TreeVisitor
    {
        private LangElement _statement;

        /// <summary>
        /// Registered program points registered to their lang elements
        /// </summary>
        private readonly Dictionary<LangElement, ProgramPointBase> _programPoints
            = new Dictionary<LangElement, ProgramPointBase>();

        private readonly LValueFactory _lValueCreator;

        private readonly RValueFactory _rValueCreator;

        private readonly AliasValueFactory _aliasValueCreator;

        private static readonly Type[] FlowOmittedElements = new Type[]
        {
            typeof(ActualParam), typeof(FormalParam), typeof(DirectTypeRef)
        };

        /// <summary>
        /// Prevents a default instance of the <see cref="ElementExpander" /> class from being created.
        /// </summary>
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

        public static ProgramPointBase[] ExpandStatement(LangElement statement,
            OnPointCreated onPointCreated)
        {
            var expander = new ElementExpander();
            expander.Expand(statement);
            var postfix = Converter.GetPostfix(statement);

            var expandedChain = createPointsChain(expander, postfix);
            registerCreatedPoints(expander, onPointCreated);

            return expandedChain.ToArray();
        }

        private static IEnumerable<ProgramPointBase> createPointsChain(ElementExpander expander,
            IEnumerable<LangElement> orderedPartialas)
        {
            ProgramPointBase lastPoint = null;
            foreach (var partial in orderedPartialas)
            {
                //skip all flow omitted lang elements
                if (FlowOmittedElements.Contains(partial.GetType()))
                {
                    continue;
                }

                var currentPoint = expander.GetProgramPoint(partial);

                if (lastPoint == null)
                {
                    //there is no parent to add child
                    lastPoint = currentPoint;

                    yield return currentPoint; //because we dont want to miss first point
                    continue;
                }

                //join statement chain with flow edges in postfix computation order
                if (!currentPoint.FlowChildren.Any())
                {
                    //because of sharing points in some expressions - point is on flow path before this
                    lastPoint.AddFlowChild(currentPoint);

                    yield return currentPoint; //report point in right order
                }

                lastPoint = currentPoint;
            }
        }

        private static void registerCreatedPoints(ElementExpander expander, OnPointCreated onPointCreated)
        {
            foreach (var pair in expander._programPoints)
            {
                onPointCreated(pair.Value);
            }
        }

        private static int getNearestFlowPartial(Postfix postfix, int startIndex)
        {
            while (startIndex < postfix.Length
                && FlowOmittedElements.Contains(postfix.GetElement(startIndex).GetType()))
            {
                ++startIndex;
            }

            return startIndex;
        }

        private ProgramPointBase GetProgramPoint(LangElement partial)
        {
            return _programPoints[partial];
        }

        #region Program point creation

        internal ValuePoint CreateRValue(LangElement el)
        {
            if (el == null)
            {
                return null;
            }

            ProgramPointBase existingPoint;
            if (_programPoints.TryGetValue(el, out existingPoint))
            {
                return existingPoint as ValuePoint;
            }

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

        internal ValuePoint CreateAliasValue(LangElement el)
        {
            var result = _aliasValueCreator.CreateValue(el);

            if (_programPoints.ContainsKey(el))
            {
                //aliased value can be created as RValue (it will be added twice)
                if (_programPoints[el] != result)
                {
                    throw new NotSupportedException("Cannot add two different program point to single element");
                }
            }
            else
            {
                _programPoints.Add(el, result);
            }

            return result;
        }

        private void Result(ProgramPointBase point)
        {
            _programPoints.Add(_statement, point);
        }

        /// <summary>
        /// Set result from created rvalue (doesn't add result twice into result)
        /// </summary>
        /// <param name="el"></param>
        private void RValueResult(LangElement el)
        {
            //result is added via creation call
            CreateRValue(el);
        }

        #endregion

        #region TreeVisitor implementation

        #region Global visiting

        /// <inheritdoc />
        public override void VisitGlobalConstantDecl(GlobalConstantDecl x)
        {
            var constantValue = CreateRValue(x.Initializer);

            Result(new ConstantDeclPoint(x, constantValue));
        }

        /// <inheritdoc />
        public override void VisitGlobalStmt(GlobalStmt x)
        {
            var variables = new List<LValuePoint>();
            foreach (var varItem in x.VarList)
            {
                var lValue = CreateLValue(varItem);

                variables.Add(lValue);
            }

            Result(new GlobalStmtPoint(x, variables.ToArray()));
        }

        #endregion

        #region Variable visiting

        /// <inheritdoc />
        public override void VisitItemUse(ItemUse x)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void VisitDirectVarUse(DirectVarUse x)
        {
            RValueResult(x);
        }

        #endregion

        #region Assign expressions visiting

        /// <inheritdoc />
        public override void VisitRefAssignEx(RefAssignEx x)
        {
            var rOperand = CreateAliasValue(x.RValue);
            var lOperand = CreateLValue(x.LValue);

            Result(new RefAssignPoint(x, lOperand, rOperand));
        }

        /// <inheritdoc />
        public override void VisitValueAssignEx(ValueAssignEx x)
        {
            RValueResult(x);
        }

        #endregion

        #region Function visiting

        /// <inheritdoc />
        public override void VisitDirectFcnCall(DirectFcnCall x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitIndirectFcnCall(IndirectFcnCall x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitDirectStMtdCall(DirectStMtdCall x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitIndirectStMtdCall(IndirectStMtdCall x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitFunctionDecl(FunctionDecl x)
        {
            Result(new FunctionDeclPoint(x));
        }

        /// <inheritdoc />
        public override void VisitLambdaFunctionExpr(LambdaFunctionExpr x)
        {
            RValueResult(x);
        }

        #endregion

        /// <inheritdoc />
        public override void VisitElement(LangElement element)
        {
            throw new NotImplementedException("Element is not supported as statement");
        }

        /// <inheritdoc />
        public override void VisitBinaryEx(BinaryEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitUnaryEx(UnaryEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitConcatEx(ConcatEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitIncDecEx(IncDecEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitArrayEx(ArrayEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitJumpStmt(JumpStmt x)
        {
            var expression = CreateRValue(x.Expression);
            Result(new JumpStmtPoint(expression, x));
        }

        /// <inheritdoc />
        public override void VisitTypeDecl(TypeDecl x)
        {
            Result(new TypeDeclPoint(x));
        }

        /// <inheritdoc />
        public override void VisitGlobalConstUse(GlobalConstUse x)
        {
            RValueResult(x);
        }

        public override void VisitClassConstUse(ClassConstUse x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitNewEx(NewEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitInstanceOfEx(InstanceOfEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitIncludingEx(IncludingEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitForeachStmt(ForeachStmt x)
        {
            var enumeree = CreateRValue(x.Enumeree);

            LValuePoint keyVar = null;
            LValuePoint valueVar = null;

            if (x.KeyVariable != null)
            {
                keyVar = CreateLValue(x.KeyVariable.Variable);
            }

            if (x.ValueVariable != null)
            {
                valueVar = CreateLValue(x.ValueVariable.Variable);
            }

            Result(new ForeachStmtPoint(x, enumeree, keyVar, valueVar));
        }

        /// <inheritdoc />
        public override void VisitEchoStmt(EchoStmt x)
        {
            var parameters = new List<ValuePoint>();
            foreach (var param in x.Parameters)
            {
                parameters.Add(CreateRValue(param));
            }

            Result(new EchoStmtPoint(x, parameters.ToArray()));
        }

        /// <inheritdoc />
        public override void VisitIssetEx(IssetEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitEmptyEx(EmptyEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitExitEx(ExitEx x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitThrowStmt(ThrowStmt x)
        {
            var throwedValue = CreateRValue(x.Expression);
            Result(new ThrowStmtPoint(x, throwedValue));
        }

        #region Literals

        /// <inheritdoc />
        public override void VisitIntLiteral(IntLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitLongIntLiteral(LongIntLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitDoubleLiteral(DoubleLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitStringLiteral(StringLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitBinaryStringLiteral(BinaryStringLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitBoolLiteral(BoolLiteral x)
        {
            RValueResult(x);
        }

        /// <inheritdoc />
        public override void VisitNullLiteral(NullLiteral x)
        {
            RValueResult(x);
        }

        #endregion

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
