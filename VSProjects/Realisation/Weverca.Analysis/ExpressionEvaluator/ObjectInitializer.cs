/*
Copyright (c) 2012-2014 David Skorvaga and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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

        /// <summary>
        /// Visits the int literal.
        /// </summary>
        /// <param name="x">The x.</param>
        public override void VisitIntLiteral(IntLiteral x)
        {
            initializationValue = expressionResolver.IntLiteral(x);
        }

        /// <summary>
        /// Visits the long int literal.
        /// </summary>
        /// <param name="x">The x.</param>
        public override void VisitLongIntLiteral(LongIntLiteral x)
        {
            initializationValue = expressionResolver.LongIntLiteral(x);
        }

        /// <summary>
        /// Visits the double literal.
        /// </summary>
        /// <param name="x">The x.</param>
        public override void VisitDoubleLiteral(DoubleLiteral x)
        {
            initializationValue = expressionResolver.DoubleLiteral(x);
        }

        /// <summary>
        /// Visits the string literal.
        /// </summary>
        /// <param name="x">The x.</param>
        public override void VisitStringLiteral(StringLiteral x)
        {
            initializationValue = expressionResolver.StringLiteral(x);
        }

        /// <summary>
        /// Visits the binary string literal.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void VisitBinaryStringLiteral(BinaryStringLiteral x)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Visits the bool literal.
        /// </summary>
        /// <param name="x">The x.</param>
        public override void VisitBoolLiteral(BoolLiteral x)
        {
            initializationValue = expressionResolver.BoolLiteral(x);
        }

        /// <summary>
        /// Visits the null literal.
        /// </summary>
        /// <param name="x">The x.</param>
        public override void VisitNullLiteral(NullLiteral x)
        {
            initializationValue = expressionResolver.NullLiteral(x);
        }

        /// <summary>
        /// Visits the global constant use.
        /// </summary>
        /// <param name="x">The x.</param>
        public override void VisitGlobalConstUse(GlobalConstUse x)
        {
            initializationValue = expressionResolver.Constant(x);
        }

        /// <summary>
        /// Visits the class constant use.
        /// </summary>
        /// <param name="x">The x.</param>
        public override void VisitClassConstUse(ClassConstUse x)
        {
            initializationValue = expressionResolver.ClassConstant(new MemoryEntry(OutSet.CreateString(x.ClassName.QualifiedName.Name.Value)),new VariableName(x.Name.Value));
        }

        /// <summary>
        /// Visit new array items initializers.
        /// </summary>
        /// <param name="x">The visited value.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <exception cref="System.NotSupportedException">There is no other array item type</exception>
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