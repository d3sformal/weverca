/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using PHP.Core;

namespace Weverca.AnalysisFramework.ProgramPoints
{
    /// <summary>
    /// Function call representation
    /// </summary>
    public class FunctionCallPoint : RCallPoint
    {
        /// <summary>
        /// Function call expression
        /// </summary>
        public readonly DirectFcnCall FunctionCall;

        /// <inheritdoc />
        public override LangElement Partial { get { return FunctionCall; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionCallPoint" /> class.
        /// </summary>
        /// <param name="functionCall">Function call expression</param>
        /// <param name="thisObj">Program point with an object if the subroutine is method</param>
        /// <param name="arguments">Program points with arguments of function call</param>
        internal FunctionCallPoint(DirectFcnCall functionCall, ValuePoint thisObj, ValuePoint[] arguments)
            : base(thisObj, functionCall.CallSignature, arguments)
        {
            FunctionCall = functionCall;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            PrepareArguments();

            if (ThisObj == null)
            {
                Services.FunctionResolver.Call(FunctionCall.QualifiedName, Flow.Arguments);
            }
            else
            {
                Services.FunctionResolver.MethodCall(ThisObj.Value,
                    FunctionCall.QualifiedName, Flow.Arguments);
            }
        }

        /*
        /// <inheritdoc />
        protected override void extendOutput()
        {
            OutSet.StartTransaction();
            // TODO: Change call handling
            OutSet.ExtendAsCall(_inSet, Flow.CalledObject, Flow.Arguments);
            // OutSet.ExtendAsCall(InSet, Flow.CalledObject, Flow.Arguments);
        }
        */

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitFunctionCall(this);
        }
    }

    /// <summary>
    /// Indirect function call representation
    /// </summary>
    public class IndirectFunctionCallPoint : RCallPoint
    {
        /// <summary>
        /// Indirect function call expression
        /// </summary>
        public readonly IndirectFcnCall FunctionCall;

        /// <summary>
        /// Indirect name of call
        /// </summary>
        public readonly ValuePoint Name;

        /// <inheritdoc />
        public override LangElement Partial { get { return FunctionCall; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndirectFunctionCallPoint" /> class.
        /// </summary>
        /// <param name="functionCall">Indirect function call expression</param>
        /// <param name="name">Indirect name of call</param>
        /// <param name="thisObj">Program point with an object if the indirect subroutine is method</param>
        /// <param name="arguments">Program points with arguments of function call</param>
        internal IndirectFunctionCallPoint(IndirectFcnCall functionCall, ValuePoint name,
            ValuePoint thisObj, ValuePoint[] arguments)
            : base(thisObj, functionCall.CallSignature, arguments)
        {
            FunctionCall = functionCall;
            Name = name;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            PrepareArguments();
            if (ThisObj == null)
            {
                Services.FunctionResolver.IndirectCall(
                    Name.Value.ReadMemory(OutSnapshot), Flow.Arguments);
            }
            else
            {
                Services.FunctionResolver.IndirectMethodCall(ThisObj.Value,
                    Name.Value.ReadMemory(OutSnapshot), Flow.Arguments);
            }
        }

        /*
        /// <inheritdoc />
        protected override void extendOutput()
        {
            OutSet.StartTransaction();
            // TODO: Change call handling
            OutSet.ExtendAsCall(_inSet, Flow.CalledObject, Flow.Arguments);
            // OutSet.ExtendAsCall(InSet, Flow.CalledObject, Flow.Arguments);
        }
        */

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitIndirectFunctionCall(this);
        }
    }

    /// <summary>
    /// Static method call representation
    /// </summary>
    public class StaticMethodCallPoint : RCallPoint
    {
        /// <summary>
        /// Static method call expression
        /// </summary>
        public readonly DirectStMtdCall StaticMethodCall;

        /// <inheritdoc />
        public override LangElement Partial { get { return StaticMethodCall; } }

        /// <summary>
        /// Static method call expression
        /// </summary>
        public readonly ValuePoint ObjectName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticMethodCallPoint" /> class.
        /// </summary>
        /// <param name="staticMethodCall">Static method call expression</param>
        /// <param name="arguments">Program points with arguments of static method call</param>
        /// <param name="objectName">Name of called object</param>
        internal StaticMethodCallPoint(DirectStMtdCall staticMethodCall, ValuePoint objectName, ValuePoint[] arguments)
            : base(objectName, staticMethodCall.CallSignature, arguments)
        {
            StaticMethodCall = staticMethodCall;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            PrepareArguments();

            if (ThisObj == null)
            {
                Flow.CalledObject = new MemoryEntry(OutSet.CreateString(StaticMethodCall.ClassName.QualifiedName.Name.LowercaseValue));
                Services.FunctionResolver.StaticMethodCall(StaticMethodCall.ClassName.QualifiedName,
new QualifiedName(StaticMethodCall.MethodName), Flow.Arguments);
            }
            else
            {
                Services.FunctionResolver.StaticMethodCall(ThisObj.Value,
new QualifiedName(StaticMethodCall.MethodName), Flow.Arguments);
            }


        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitStaticMethodCall(this);
        }
    }

    /// <summary>
    /// Indirect static method call representation
    /// </summary>
    public class IndirectStaticMethodCallPoint : RCallPoint
    {
        /// <summary>
        /// Indirect static method call expression
        /// </summary>
        public readonly IndirectStMtdCall StaticMethodCall;

        /// <summary>
        /// Indirect name of call
        /// </summary>
        public readonly ValuePoint Name;

        /// <inheritdoc />
        public override LangElement Partial { get { return StaticMethodCall; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndirectStaticMethodCallPoint" /> class.
        /// </summary>
        /// <param name="staticMethodCall">Indirect static method call expression</param>
        /// <param name="name">Indirect name of call</param>
        /// <param name="arguments">Program points with arguments of static method call</param>\
        /// <param name="objectName">Name of called object</param>
        internal IndirectStaticMethodCallPoint(IndirectStMtdCall staticMethodCall, ValuePoint objectName, ValuePoint name,
            ValuePoint[] arguments)
            : base(objectName, staticMethodCall.CallSignature, arguments)
        {
            StaticMethodCall = staticMethodCall;
            Name = name;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            PrepareArguments();

            if (ThisObj == null)
            {
                Flow.CalledObject = new MemoryEntry(OutSet.CreateString(StaticMethodCall.ClassName.QualifiedName.Name.LowercaseValue));
                Services.FunctionResolver.IndirectStaticMethodCall(StaticMethodCall.ClassName.QualifiedName,
                 Name.Value.ReadMemory(OutSnapshot), Flow.Arguments);
            }
            else
            {
                Services.FunctionResolver.IndirectStaticMethodCall(ThisObj.Value,
Name.Value.ReadMemory(OutSnapshot), Flow.Arguments);
            }


        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitIndirectStaticMethodCall(this);
        }
    }
}