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

        public virtual void VisitValue(ValuePoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitLValue(ValuePoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitRCall(RCallPoint p)
        {
            VisitValue(p);
        }

        #endregion
        
        #region Expression points

        public virtual void VisitArray(ArrayExPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitIncDec(IncDecExPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitConcat(ConcatExPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitUnary(UnaryExPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitBinary(BinaryExPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitInclude(IncludingExPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitNew(NewExPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitInstanceOf(InstanceOfExPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitIsSet(IssetPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitEmptyEx(EmptyExPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitExit(ExitExPoint p)
        {
            VisitValue(p);
        }


        public virtual void VisitClassConstPoint(ClassConstPoint p)
        {
            VisitValue(p);
        }
        #endregion

        #region Assign points

        public virtual void VisitAssign(AssignPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitAssignConcat(AssignConcatPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitAssignOperation(AssignOperationPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitRefAssign(RefAssignPoint p)
        {
            VisitValue(p);
        }

        #endregion

        #region Special points

        public virtual void VisitTryScopeStarts(TryScopeStartsPoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitTryScopeEnds(TryScopeEndsPoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitAssume(AssumePoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitExtension(ExtensionPoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitExtensionSink(ExtensionSinkPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitNativeAnalyzer(NativeAnalyzerPoint p)
        {
            VisitValue(p);
        }

        #endregion

        #region Declaration points

        public virtual void VisitTypeDecl(TypeDeclPoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitFunctionDecl(FunctionDeclPoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitConstantDecl(ConstantDeclPoint p)
        {
            VisitValue(p);
        }

        public virtual void VisitConstant(ConstantPoint p)
        {
            VisitValue(p);
        }

        #endregion

        #region Statement points

        public virtual void VisitThrow(ThrowStmtPoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitGlobal(GlobalStmtPoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitEcho(EchoStmtPoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitForeach(ForeachStmtPoint p)
        {
            VisitPoint(p);
        }

        public virtual void VisitJump(JumpStmtPoint p)
        {
            VisitPoint(p);
        }

        #endregion

        #region Call points

        public virtual void VisitFunctionCall(FunctionCallPoint p)
        {
            VisitRCall(p);
        }

        public virtual void VisitIndirectFunctionCall(IndirectFunctionCallPoint p)
        {
            VisitRCall(p);
        }

        public virtual void VisitStaticMethodCall(StaticMethodCallPoint p)
        {
            VisitRCall(p);
        }

        public virtual void VisitIndirectStaticMethodCall(IndirectStaticMethodCallPoint p)
        {
            VisitRCall(p);
        }
        #endregion

        #region LValue points

        public virtual void VisitVariable(VariablePoint p)
        {
            VisitLValue(p);
        }

        public virtual void VisitIndirectVariable(IndirectVariablePoint p)
        {
            VisitLValue(p);
        }

        public virtual void VisitItemUse(ItemUsePoint p)
        {
            VisitLValue(p);
        }

        #endregion
    }
}
