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


        /// <summary>
        /// Visits the value program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitValue(ValuePoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the L value program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitLValue(ValuePoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the R call program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitRCall(RCallPoint p)
        {
            VisitValue(p);
        }

        #endregion

        #region Expression points

        /// <summary>
        /// Visits the array program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitArray(ArrayExPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the increment / decrement program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitIncDec(IncDecExPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the constant program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitConcat(ConcatExPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the program point of unary operation.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitUnary(UnaryExPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the program point binary operation.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitBinary(BinaryExPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the include program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitInclude(IncludingExPoint p)
        {
            VisitRCall(p);
        }

        /// <summary>
        /// Visits the eval program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        internal void VisitEval(EvalExPoint p)
        {
            VisitRCall(p);
        }

        /// <summary>
        /// Visits the program point of new construct.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitNew(NewExPoint p)
        {
            VisitRCall(p);
        }

        /// <summary>
        /// Visits the "Instance of" program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitInstanceOf(InstanceOfExPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the program point of "is set" operator.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitIsSet(IssetPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the program point of empty expression.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitEmptyEx(EmptyExPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the exit program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitExit(ExitExPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the class constant program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitClassConstPoint(ClassConstPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the conditional program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        internal void VisitConditional(ConditionalExPoint p)
        {
            VisitValue(p);
        }
        #endregion

        #region Assign points

        /// <summary>
        /// Visits the assignment program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitAssign(AssignPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the concatenation program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitAssignConcat(AssignConcatPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the assignment operation program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitAssignOperation(AssignOperationPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the reference assignment program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitRefAssign(RefAssignPoint p)
        {
            VisitValue(p);
        }

        #endregion

        #region Special points

        /// <summary>
        /// Visits the try scope start program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitTryScopeStarts(TryScopeStartsPoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the try scope end program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitTryScopeEnds(TryScopeEndsPoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the assumption program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitAssume(AssumePoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the extension program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitExtension(ExtensionPoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the extension sink program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitExtensionSink(ExtensionSinkPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the native analyzer program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitNativeAnalyzer(NativeAnalyzerPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the catch program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitCatch(CatchPoint p)
        {
            VisitPoint(p);
        }

        #endregion

        #region Declaration points

        /// <summary>
        /// Visits the type declaration program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitTypeDecl(TypeDeclPoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the function declaration program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitFunctionDecl(FunctionDeclPoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the constant declaration program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitConstantDecl(ConstantDeclPoint p)
        {
            VisitValue(p);
        }

        /// <summary>
        /// Visits the constant program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitConstant(ConstantPoint p)
        {
            VisitValue(p);
        }

        #endregion

        #region Statement points

        /// <summary>
        /// Visits the throw program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitThrow(ThrowStmtPoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the global statement program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitGlobal(GlobalStmtPoint p)
        {
            VisitPoint(p);        
        }

        /// <summary>
        /// Visits the echo statement program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitEcho(EchoStmtPoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the foreach program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitForeach(ForeachStmtPoint p)
        {
            VisitPoint(p);
        }

        /// <summary>
        /// Visits the jump program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitJump(JumpStmtPoint p)
        {
            VisitPoint(p);
        }

        #endregion

        #region Call points

        /// <summary>
        /// Visits the function call program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitFunctionCall(FunctionCallPoint p)
        {
            VisitRCall(p);
        }

        /// <summary>
        /// Visits the indirect function call program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitIndirectFunctionCall(IndirectFunctionCallPoint p)
        {
            VisitRCall(p);
        }

        /// <summary>
        /// Visits the static method call program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitStaticMethodCall(StaticMethodCallPoint p)
        {
            VisitRCall(p);
        }

        /// <summary>
        /// Visits the indirect static method call program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitIndirectStaticMethodCall(IndirectStaticMethodCallPoint p)
        {
            VisitRCall(p);
        }
        #endregion

        #region LValue points

        /// <summary>
        /// Visits the static field program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitStaticField(StaticFieldPoint p)
        {
            VisitLValue(p);
        }

        /// <summary>
        /// Visits the indirect static field program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitIndirectStaticField(IndirectStaticFieldPoint p)
        {
            VisitLValue(p);
        }

        /// <summary>
        /// Visits the variable program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitVariable(VariablePoint p)
        {
            VisitLValue(p);
        }

        /// <summary>
        /// Visits the indirect variable program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitIndirectVariable(IndirectVariablePoint p)
        {
            VisitLValue(p);
        }

        /// <summary>
        /// Visits the item use program point.
        /// </summary>
        /// <param name="p">Visited point</param>
        public virtual void VisitItemUse(ItemUsePoint p)
        {
            VisitLValue(p);
        }

        #endregion
    }
}
