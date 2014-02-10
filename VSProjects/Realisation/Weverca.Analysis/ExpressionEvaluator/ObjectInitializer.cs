using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using PHP.Core;
using PHP.Core.AST;
using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework;

namespace Weverca.Analysis.ExpressionEvaluator
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
        public MemoryEntry initializationValue;

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
        public void InitializeObject(ObjectValue createdObject, ClassDecl typeDeclaration)
        {
            Debug.Assert(thisObject == null, "Created object must be selected after enter to method");

            try
            {
                thisObject = createdObject;
                Debug.Assert(thisObject != null, "Initialized object must stay until end of initialization");
                ReadSnapshotEntryBase objectEntry = OutSet.CreateSnapshotEntry(new MemoryEntry(createdObject));

                List<QualifiedName> classHierarchy=new List<QualifiedName>(typeDeclaration.BaseClasses);
                classHierarchy.Add(typeDeclaration.QualifiedName);
                Dictionary<VariableName, FieldInfo> currentClassFields = new Dictionary<VariableName, FieldInfo>();
                
                foreach (var className in classHierarchy)
                {
                    foreach (var entry in typeDeclaration.Fields.Where(a => a.Key.ClassName.Equals(className)))
                    {
                        if (entry.Value.IsStatic == false)
                        {
                            currentClassFields[entry.Key.Name] = entry.Value;
                        }
                    }

                }

                foreach (var entry in currentClassFields.Values)
                {
                    var fieldEntry=objectEntry.ReadField(OutSet.Snapshot, new VariableIdentifier(entry.Name.Value));
                    if (entry.InitValue != null)
                    {
                        fieldEntry.WriteMemory(OutSet.Snapshot, entry.InitValue);
                    }
                    else if (entry.Initializer != null)
                    {
                        entry.Initializer.VisitMe(this);
                        fieldEntry.WriteMemory(OutSet.Snapshot, initializationValue);
                    }
                    else 
                    {
                        fieldEntry.WriteMemory(OutSet.Snapshot, new MemoryEntry(OutSet.UndefinedValue));
                    }

                }
            }
            finally
            {
                thisObject = null;
            }
        }
        
        #region Literals

        /// <inheritdoc />
        public override void VisitIntLiteral(IntLiteral x)
        {
            initializationValue = expressionResolver.IntLiteral(x);
        }

        /// <inheritdoc />
        public override void VisitLongIntLiteral(LongIntLiteral x)
        {
            initializationValue = expressionResolver.LongIntLiteral(x);
        }

        /// <inheritdoc />
        public override void VisitDoubleLiteral(DoubleLiteral x)
        {
            initializationValue = expressionResolver.DoubleLiteral(x);
        }

        /// <inheritdoc />
        public override void VisitStringLiteral(StringLiteral x)
        {
            initializationValue = expressionResolver.StringLiteral(x);
        }

        /// <inheritdoc />
        public override void VisitBinaryStringLiteral(BinaryStringLiteral x)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void VisitBoolLiteral(BoolLiteral x)
        {
            initializationValue = expressionResolver.BoolLiteral(x);
        }

        /// <inheritdoc />
        public override void VisitNullLiteral(NullLiteral x)
        {
            initializationValue = expressionResolver.NullLiteral(x);
        }

        /// <inheritdoc />
        public override void VisitGlobalConstUse(GlobalConstUse x)
        {
            initializationValue = expressionResolver.Constant(x);
        }

        /// <inheritdoc />
        public override void VisitClassConstUse(ClassConstUse x)
        {
            initializationValue = expressionResolver.ClassConstant(new MemoryEntry(OutSet.CreateString(x.ClassName.QualifiedName.Name.Value)),new VariableName(x.Name.Value));
        }

        /// <inheritdoc />
        public override void VisitArrayEx(ArrayEx x)
        {
            List<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs=new List<KeyValuePair<MemoryEntry,MemoryEntry>>();

            foreach (var item in x.Items)
            {
                MemoryEntry index = null;
                if (item.Index != null)
                {
                    item.Index.VisitMe(this);
                    index = initializationValue;
                }

                MemoryEntry value = null;
                var valueItem = item as ValueItem;
                if (valueItem != null)
                {
                    valueItem.ValueExpr.VisitMe(this);
                    value = initializationValue;
                }
                else
                {
                    var refItem = item as RefItem;
                    if (refItem != null)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotSupportedException("There is no other array item type");
                    }
                }

                keyValuePairs.Add(new KeyValuePair<MemoryEntry, MemoryEntry>(index, value));
            }

            initializationValue = expressionResolver.ArrayEx(keyValuePairs);
        }
        #endregion
    }
}
