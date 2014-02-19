using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Abstract visitor used for visiting program points according to their type
    /// </summary>
    public abstract class ProgramPointVisitor
    {
        /// <summary>
        /// Abstract method for visiting points (Is used as default fallback, if other 
        /// visiting methods are not overriden)
        /// </summary>
        /// <param name="p">Visited point</param>
        public abstract void VisitPoint(ProgramPointBase p);

        /// <summary>
        /// Visit empty program point
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitEmpty(EmptyProgramPoint p)
        {
            VisitPoint(p);
        }

        #region Abstract points

        /// <inheritdoc />
        public virtual void VisitValue(ValuePoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitLValue(ValuePoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitRCall(RCallPoint p)
        {
            VisitValue(p);
        }

        #endregion

        #region Expression points

        /// <inheritdoc />
        public virtual void VisitArray(ArrayExPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitIncDec(IncDecExPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitConcat(ConcatExPoint p)
        {
            VisitValue(p);
        }
        
        /// <inheritdoc />
        public virtual void VisitUnary(UnaryExPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitBinary(BinaryExPoint p)
        {
            VisitValue(p);
        }
        
        /// <inheritdoc />
        public virtual void VisitInclude(IncludingExPoint p)
        {
            VisitRCall(p);
        }

        /// <inheritdoc />
        internal void VisitEval(EvalExPoint p)
        {
            VisitRCall(p);
        }

        /// <inheritdoc />
        public virtual void VisitNew(NewExPoint p)
        {
            VisitRCall(p);
        }

        /// <inheritdoc />
        public virtual void VisitInstanceOf(InstanceOfExPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitIsSet(IssetPoint p)
        {
            VisitValue(p);
        }
        
        /// <inheritdoc />
        public virtual void VisitEmptyEx(EmptyExPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitExit(ExitExPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitClassConstPoint(ClassConstPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        internal void VisitConditional(ConditionalExPoint p)
        {
            VisitValue(p);
        }
        #endregion

        #region Assign points

        /// <inheritdoc />
        public virtual void VisitAssign(AssignPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitAssignConcat(AssignConcatPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitAssignOperation(AssignOperationPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitRefAssign(RefAssignPoint p)
        {
            VisitValue(p);
        }

        #endregion

        #region Special points

        /// <inheritdoc />
        public virtual void VisitTryScopeStarts(TryScopeStartsPoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitTryScopeEnds(TryScopeEndsPoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitAssume(AssumePoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitExtension(ExtensionPoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitExtensionSink(ExtensionSinkPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitNativeAnalyzer(NativeAnalyzerPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitCatch(CatchPoint p)
        {
            VisitPoint(p);
        }

        #endregion

        #region Declaration points

        /// <inheritdoc />
        public virtual void VisitTypeDecl(TypeDeclPoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitFunctionDecl(FunctionDeclPoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitConstantDecl(ConstantDeclPoint p)
        {
            VisitValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitConstant(ConstantPoint p)
        {
            VisitValue(p);
        }

        #endregion

        #region Statement points

        /// <inheritdoc />
        public virtual void VisitThrow(ThrowStmtPoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitGlobal(GlobalStmtPoint p)
        {
            VisitPoint(p);        
        }

        /// <inheritdoc />
        public virtual void VisitEcho(EchoStmtPoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitForeach(ForeachStmtPoint p)
        {
            VisitPoint(p);
        }

        /// <inheritdoc />
        public virtual void VisitJump(JumpStmtPoint p)
        {
            VisitPoint(p);
        }

        #endregion

        #region Call points

        /// <inheritdoc />
        public virtual void VisitFunctionCall(FunctionCallPoint p)
        {
            VisitRCall(p);
        }

        /// <inheritdoc />
        public virtual void VisitIndirectFunctionCall(IndirectFunctionCallPoint p)
        {
            VisitRCall(p);
        }

        /// <inheritdoc />
        public virtual void VisitStaticMethodCall(StaticMethodCallPoint p)
        {
            VisitRCall(p);
        }

        /// <inheritdoc />
        public virtual void VisitIndirectStaticMethodCall(IndirectStaticMethodCallPoint p)
        {
            VisitRCall(p);
        }
        #endregion

        #region LValue points

        /// <inheritdoc />
        public virtual void VisitStaticField(StaticFieldPoint p)
        {
            VisitLValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitIndirectStaticField(IndirectStaticFieldPoint p)
        {
            VisitLValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitVariable(VariablePoint p)
        {
            VisitLValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitIndirectVariable(IndirectVariablePoint p)
        {
            VisitLValue(p);
        }

        /// <inheritdoc />
        public virtual void VisitItemUse(ItemUsePoint p)
        {
            VisitLValue(p);
        }

        #endregion
    }
}
