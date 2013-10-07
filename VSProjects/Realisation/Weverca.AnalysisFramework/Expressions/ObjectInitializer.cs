using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Initializes properties of new instance of given class
    /// </summary>
    /// <remarks>
    /// Within a class is usual to define set of properties. When new instance of the given class is
    /// created, every non-static property gets a value. Properties can be initialized by any value,
    /// but this value must be constant, it means that it must be able to be evaluated at compile time.
    /// If initialization is not stated, value gets default value which is null.
    /// </remarks>
    public class ObjectInitializer : TreeVisitor
    {
        /// <summary>
        /// Expression resolver used to evaluation of initialization values
        /// </summary>
        private ExpressionEvaluatorBase expressionResolver;

        /// <summary>
        /// The object whose properties are initialized
        /// </summary>
        private ObjectValue thisObject;

        /// <summary>
        /// The temporary value storing current initialization value used to initialize current property
        /// </summary>
        private MemoryEntry initializationValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectInitializer" /> class.
        /// </summary>
        /// <param name="resolver">Expression resolver used to evaluate initialization values</param>
        public ObjectInitializer(ExpressionEvaluatorBase resolver)
        {
            expressionResolver = resolver;
        }

        /// <summary>
        /// Gets current output set of the given expression evaluation
        /// </summary>
        public FlowOutputSet OutSet
        {
            get { return expressionResolver.OutSet; }
        }

        /// <summary>
        /// Initializes the given object depending on the given type declaration
        /// </summary>
        /// <param name="createdObject">A new object to be initialized</param>
        /// <param name="typeDeclaration">Type declaration used to initialize the given object</param>
        public void InitializeObject(ObjectValue createdObject, TypeDecl typeDeclaration)
        {
            Debug.Assert(thisObject == null, "Created object must be selected after enter to method");

            try
            {
                thisObject = createdObject;
                VisitTypeDecl(typeDeclaration);
                Debug.Assert(thisObject != null, "Initialized object must stay until end of initialization");
            }
            finally
            {
                thisObject = null;
            }
        }

        #region Statements

        public override void VisitMethodDecl(MethodDecl x)
        {
            // Omit the method
        }

        public override void VisitFieldDecl(FieldDecl x)
        {
            initializationValue = null;

            try
            {
                if (x.Initializer != null)
                {
                    base.VisitFieldDecl(x);
                }
                else
                {
                    initializationValue = new MemoryEntry(OutSet.UndefinedValue);
                }

                Debug.Assert(initializationValue != null, "Every field has any initialization value");
                var index = OutSet.CreateIndex(x.Name.Value);
                OutSet.SetField(thisObject, index, initializationValue);
            }
            finally
            {
                initializationValue = null;
            }
        }

        public override void VisitClassConstantDecl(ClassConstantDecl x)
        {
            // Class constants are set when the type, not object, is declared
        }

        public override void VisitTraitsUse(TraitsUse x)
        {
            // Omit traits
        }

        #endregion

        #region Expressions

        public override void VisitGlobalConstUse(GlobalConstUse x)
        {
            initializationValue = expressionResolver.Constant(x);
        }

        public override void VisitClassConstUse(ClassConstUse x)
        {
            // TODO: Add method to expression resolver which returns class constant
        }

        public override void VisitPseudoConstUse(PseudoConstUse x)
        {
            // TODO: Add method which returns pseudo-constant depedning on position in source code
        }

        #endregion

        #region Literals

        public override void VisitIntLiteral(IntLiteral x)
        {
            initializationValue = expressionResolver.IntLiteral(x);
        }

        public override void VisitLongIntLiteral(LongIntLiteral x)
        {
            initializationValue = expressionResolver.LongIntLiteral(x);
        }

        public override void VisitDoubleLiteral(DoubleLiteral x)
        {
            initializationValue = expressionResolver.DoubleLiteral(x);
        }

        public override void VisitStringLiteral(StringLiteral x)
        {
            initializationValue = expressionResolver.StringLiteral(x);
        }

        public override void VisitBinaryStringLiteral(BinaryStringLiteral x)
        {
            throw new NotSupportedException();
        }

        public override void VisitBoolLiteral(BoolLiteral x)
        {
            initializationValue = expressionResolver.BoolLiteral(x);
        }

        public override void VisitNullLiteral(NullLiteral x)
        {
            initializationValue = expressionResolver.NullLiteral(x);
        }

        #endregion
    }
}
