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

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Result of StaticObjectVisitor
    /// </summary>
    public enum StaticObjectVisitorResult
    {
        /// <summary>
        /// No result was found
        /// </summary>
        NO_RESULT,

        /// <summary>
        /// Excaly one result was found
        /// </summary>
        ONE_RESULT,

        /// <summary>
        /// Infinite results were found
        /// </summary>
        MULTIPLE_RESULTS
    }

    /// <summary>
    /// Class resolving value of variable. On this variables static property has benn accessed
    /// </summary>
    public class StaticObjectVisitor : PartialExpressionEvaluator
    {
        /// <summary>
        /// Indicator what visitor 
        /// </summary>
        public StaticObjectVisitorResult Result;
        
        /// <summary>
        /// Resolved class name
        /// </summary>
        public QualifiedName className;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticObjectVisitor" /> class.
        /// </summary>
        public StaticObjectVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticObjectVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        public StaticObjectVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            Result = StaticObjectVisitorResult.NO_RESULT;
        }

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            Result = StaticObjectVisitorResult.MULTIPLE_RESULTS;
        }

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            Result = StaticObjectVisitorResult.MULTIPLE_RESULTS;
        }

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            Result = StaticObjectVisitorResult.MULTIPLE_RESULTS;
        }

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            className = OutSet.ObjectType(value).Declaration.QualifiedName;
            Result = StaticObjectVisitorResult.ONE_RESULT;
        }

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            className = new QualifiedName(new Name(value.Value));
            Result = StaticObjectVisitorResult.ONE_RESULT;
        }
    }
}