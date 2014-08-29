/*
Copyright (c) 2012-2014 David Hauzar, Miroslav Vodolan

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
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;
using PHP.Core.Reflection;

namespace Weverca.AnalysisFramework.UnitTest
{
    /// <summary>
    /// Expression evaluation is resolved here
    /// </summary>
    internal class SimpleExpressionEvaluator : ExpressionEvaluatorBase
    {
        public override void Assign(ReadWriteSnapshotEntryBase target, MemoryEntry value)
        {
            target.WriteMemory(OutSnapshot, value);
        }

        public override ReadWriteSnapshotEntryBase ResolveField(ReadSnapshotEntryBase objectValue, VariableIdentifier field)
        {
            return objectValue.ReadField(OutSnapshot, field);
        }

        public override ReadWriteSnapshotEntryBase ResolveStaticField(GenericQualifiedName type, VariableIdentifier field)
        {
            var varName = string.Format("{0}::{1}", type, field.DirectName);

            return OutSet.GetVariable(new VariableIdentifier(varName));
        }

        public override bool TryIdentifyInteger(string value, out int convertedValue)
        {
            return int.TryParse(value, out convertedValue);
        }

        public override ReadWriteSnapshotEntryBase ResolveIndirectStaticField(MemoryEntry typeNames, VariableIdentifier field)
        {
            var variables = new List<string>();
            foreach (var typeName in TypeNames(typeNames))
            {
                var varName = string.Format("{0}::{1}", typeName, field.DirectName);
                variables.Add(varName);
            }

            var identifier = new VariableIdentifier(variables.ToArray());
            return OutSet.GetVariable(identifier);
        }

        public override ReadWriteSnapshotEntryBase ResolveIndex(ReadSnapshotEntryBase arrayValue, MemberIdentifier index)
        {
            return arrayValue.ReadIndex(OutSnapshot, index);
        }

        public override void AliasAssign(ReadWriteSnapshotEntryBase target, ReadSnapshotEntryBase aliasedValue)
        {
            target.SetAliases(OutSnapshot, aliasedValue);
        }

        public override MemoryEntry ResolveIndexedVariable(VariableIdentifier variable)
        {
            var snapshotEntry = ResolveVariable(variable);
            var entry = snapshotEntry.ReadMemory(OutSnapshot);
            Debug.Assert(entry.Count > 0, "Every resolved variable must give at least one value");

            // NOTE: there should be precise resolution of multiple values
            var arrayValue = entry.PossibleValues.First();
            var undefinedValue = arrayValue as UndefinedValue;
            if (undefinedValue != null)
            {
                arrayValue = OutSet.CreateArray();
                var newEntry = new MemoryEntry(arrayValue);
                snapshotEntry.WriteMemory(OutSnapshot, newEntry);
                return newEntry;
            }
            else
            {
                return entry;
            }
        }

        public override ReadWriteSnapshotEntryBase ResolveVariable(VariableIdentifier variable)
        {
            var value = OutSet.GetVariable(variable);
            return value;
        }

        public IEnumerable<GenericQualifiedName> TypeNames(MemoryEntry typeValue)
        {
            return from StringValue possible in typeValue.PossibleValues select new GenericQualifiedName(new QualifiedName(new Name(possible.Value)));
        }

        public override IEnumerable<string> VariableNames(MemoryEntry value)
        {
            //TODO convert all value types
            return from StringValue possible in value.PossibleValues select possible.Value;
        }

        public override MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return areEqual(leftOperand, rightOperand);
                case Operations.Add:
                    return add(leftOperand, rightOperand);
                case Operations.Sub:
                    return sub(leftOperand, rightOperand);
                case Operations.GreaterThan:
                    return gte(leftOperand, rightOperand);
                case Operations.LessThan:
                    //same as GreaterThan but with inversed operands
                    return gte(rightOperand, leftOperand);
                case Operations.Or:
                    return or(leftOperand, rightOperand);
                default:
                    throw new NotImplementedException();
            }
        }

        public override MemoryEntry UnaryEx(Operations operation, MemoryEntry operand)
        {
            var result = new HashSet<IntegerValue>();
            switch (operation)
            {
                case Operations.Minus:
                    var negations = from IntegerValue number in operand.PossibleValues select Flow.OutSet.CreateInt(-number.Value);
                    result.UnionWith(negations);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return new MemoryEntry(result.ToArray());
        }

        public override MemoryEntry IncDecEx(IncDecEx operation, MemoryEntry incrementedValue)
        {
            var inc = operation.Inc ? 1 : -1;

            var values = new List<Value>();

            foreach (var incremented in incrementedValue.PossibleValues)
            {
                var integer = incremented as IntegerValue;
                if (integer == null)
                    return new MemoryEntry(OutSet.AnyValue);

                var result = OutSet.CreateInt(integer.Value + inc);
                values.Add(result);
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry ArrayEx(IEnumerable<KeyValuePair<MemoryEntry, MemoryEntry>> keyValuePairs)
        {
            // If the array expression does not have empty arguments, it is not supported
            foreach (var keyValue in keyValuePairs)
            {
                throw new NotImplementedException();
            }
            return new MemoryEntry(OutSet.CreateArray());
        }

        #region Expression evaluation helpers

        private MemoryEntry add(MemoryEntry left, MemoryEntry right)
        {
            if (left.Count != 1 || right.Count != 1)
            {
                throw new NotImplementedException();
            }

            var leftValue = left.PossibleValues.First() as IntegerValue;
            var rightValue = right.PossibleValues.First() as IntegerValue;

            if (leftValue == null || rightValue == null)
                return new MemoryEntry(OutSet.AnyValue);

            return new MemoryEntry(OutSet.CreateInt(leftValue.Value + rightValue.Value));
        }

        private MemoryEntry sub(MemoryEntry left, MemoryEntry right)
        {
            if (left.Count != 1 || right.Count != 1)
            {
                throw new NotImplementedException();
            }

            var leftValue = left.PossibleValues.First() as IntegerValue;
            var rightValue = right.PossibleValues.First() as IntegerValue;

            return new MemoryEntry(OutSet.CreateInt(leftValue.Value - rightValue.Value));
        }

        private MemoryEntry or(MemoryEntry left, MemoryEntry right)
        {
            if (left.Count != 1 || right.Count != 1)
            {
                throw new NotImplementedException();
            }

            var leftValue = left.PossibleValues.First() as BooleanValue;
            var rightValue = right.PossibleValues.First() as BooleanValue;

            if (rightValue == null)
                //incomplete evaluation is possible
                rightValue = leftValue;

            return new MemoryEntry(OutSet.CreateBool(leftValue.Value || rightValue.Value));
        }

        private MemoryEntry gte(MemoryEntry left, MemoryEntry right)
        {
            var canBeTrue = false;
            var canBeFalse = false;
            foreach (var leftVal in left.PossibleValues)
            {
                var leftInt = leftVal as IntegerValue;
                if (leftInt == null)
                    canBeTrue = canBeFalse = true;

                if (canBeTrue && canBeFalse)
                    //no need for continuation
                    break;

                foreach (var rightVal in right.PossibleValues)
                {
                    var rightInt = rightVal as IntegerValue;

                    if (rightInt == null)
                    {
                        canBeTrue = canBeFalse = true;
                        break;
                    }

                    if (leftInt.Value > rightInt.Value)
                        canBeTrue = true;
                    else
                        canBeFalse = true;
                }
            }

            var values = new List<Value>();
            if (canBeTrue)
                values.Add(OutSet.CreateBool(true));

            if (canBeFalse)
                values.Add(OutSet.CreateBool(false));

            return new MemoryEntry(values);
        }

        private void keepParentInfo(MemoryEntry parent, MemoryEntry child)
        {
            AnalysisTestUtils.CopyInfo(OutSet, parent, child);
        }

        private MemoryEntry areEqual(MemoryEntry left, MemoryEntry right)
        {
            var result = new List<BooleanValue>();
            if (canBeDifferent(left, right))
            {
                result.Add(OutSet.CreateBool(false));
            }

            if (canBeSame(left, right))
            {
                result.Add(OutSet.CreateBool(true));
            }

            return new MemoryEntry(result.ToArray());
        }

        private bool canBeSame(MemoryEntry left, MemoryEntry right)
        {
            if (containsAnyValue(left) || containsAnyValue(right))
            {
                return true;
            }

            foreach (var possibleValue in left.PossibleValues)
            {
                if (right.PossibleValues.Contains(possibleValue))
                {
                    return true;
                }
            }

            return false;
        }

        private bool canBeDifferent(MemoryEntry left, MemoryEntry right)
        {
            if (containsAnyValue(left) || containsAnyValue(right))
            {
                return true;
            }

            if (left.PossibleValues.Count() > 1 || left.PossibleValues.Count() > 1)
            {
                return true;
            }

            return !left.Equals(right);
        }

        private bool containsAnyValue(MemoryEntry entry)
        {
            //TODO Undefined value maybe is not correct to be treated as any value
            return entry.PossibleValues.Any((val) => val is AnyValue);
        }

        #endregion

        public override void Foreach(MemoryEntry enumeree, ReadWriteSnapshotEntryBase keyVariable,
            ReadWriteSnapshotEntryBase valueVariable)
        {
            var values = new HashSet<Value>();

            var array = enumeree.PossibleValues.First() as AssociativeArray;
            var arrayEntry = OutSet.CreateSnapshotEntry(new MemoryEntry(array));

            var indexes = arrayEntry.IterateIndexes(OutSnapshot);
            foreach (var index in indexes)
            {
                var indexEntry = arrayEntry.ReadIndex(OutSnapshot, index);
                var element = indexEntry.ReadMemory(OutSnapshot);
                values.UnionWith(element.PossibleValues);
            }

            valueVariable.WriteMemory(OutSnapshot, new MemoryEntry(values));
        }

        public override MemoryEntry Constant(GlobalConstUse x)
        {
            Value result;
            switch (x.Name.Name.Value)
            {
                case "true":
                    result = OutSet.CreateBool(true);
                    break;
                case "false":
                    result = OutSet.CreateBool(false);
                    break;
                default:
                    var constantName = ".constant_" + x.Name;
                    var constantVar = new VariableIdentifier(constantName);
                    OutSet.FetchFromGlobal(constantVar.DirectName);
                    return OutSet.ReadVariable(constantVar).ReadMemory(OutSnapshot);
            }

            return new MemoryEntry(result);
        }

        public override void ConstantDeclaration(ConstantDecl x, MemoryEntry constantValue)
        {
            var constantName = ".constant_" + x.Name;
            var constantVar = new VariableIdentifier(constantName);
            OutSet.FetchFromGlobal(constantVar.DirectName);

            OutSet.GetVariable(constantVar).WriteMemory(OutSnapshot, constantValue);
        }

        public override MemoryEntry Concat(IEnumerable<MemoryEntry> parts)
        {
            var result = new StringBuilder();
            foreach (var part in parts)
            {
                if (part.Count != 1)
                {
                    throw new NotImplementedException();
                }

                var partValue = part.PossibleValues.First() as ScalarValue;
                result.Append(partValue.RawValue);
            }

            return new MemoryEntry(Flow.OutSet.CreateString(result.ToString()));
        }

        public override void Echo(EchoStmt echo, MemoryEntry[] values)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Value> IssetEx(IEnumerable<ReadSnapshotEntryBase> entries)
        {
            Debug.Assert(entries.GetEnumerator().MoveNext(),
                "isset expression must have at least one parameter");


            var canBeDefined = false;
            var canBeUndefined = false;
            foreach (var snapshotEntry in entries)
            {

                if (snapshotEntry.IsDefined(OutSnapshot))
                {
                    canBeDefined = true;
                }
                else
                {
                    canBeUndefined = true;
                }
            }

            if (canBeDefined)
                yield return OutSet.CreateBool(true);

            if (canBeUndefined)
                yield return OutSet.CreateBool(false);
        }

        public override MemoryEntry EmptyEx(ReadWriteSnapshotEntryBase variable)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry Exit(ExitEx exit, MemoryEntry status)
        {
            var end = new ThrowInfo(new CatchBlockDescription(Flow.ProgramEnd, new GenericQualifiedName(), null), status);
            Flow.SetThrowBranching(new[] { end }, true);
            // Exit expression never returns, but it is still expression so it must return something
            return new MemoryEntry(OutSet.AnyValue);
        }

        public override MemoryEntry CreateObject(QualifiedName typeName)
        {
            var types = OutSet.ResolveType(typeName);
            if (!types.GetEnumerator().MoveNext())
            {
                // TODO: If no type is resolved, exception should be thrown
                Debug.Fail("No type resolved");
            }

            var values = new List<ObjectValue>();
            foreach (var type in types)
            {
                var newObject = CreateInitializedObject(type);
                values.Add(newObject);
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry IndirectCreateObject(MemoryEntry possibleNames)
        {
            var declarations = new HashSet<TypeValue>();

            foreach (StringValue name in possibleNames.PossibleValues)
            {
                var qualifiedName = new QualifiedName(new Name(name.Value));
                var types = OutSet.ResolveType(qualifiedName);
                if (!types.GetEnumerator().MoveNext())
                {
                    // TODO: If no type is resolved, exception should be thrown
                    Debug.Fail("No type resolved");
                }

                declarations.UnionWith(types);
            }

            var values = new List<ObjectValue>();
            foreach (var declaration in declarations)
            {
                var newObject = CreateInitializedObject(declaration);
                values.Add(newObject);
            }

            return new MemoryEntry(values);
        }

        public override MemoryEntry InstanceOfEx(MemoryEntry expression, QualifiedName className)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry IndirectInstanceOfEx(MemoryEntry expression, MemoryEntry possibleNames)
        {
            throw new NotImplementedException();
        }

        public override MemberIdentifier MemberIdentifier(MemoryEntry memberRepresentation)
        {
            var possibleNames = new List<string>();
            foreach (var possibleMember in memberRepresentation.PossibleValues)
            {
                var value = possibleMember as ScalarValue;
                if (value == null)
                {
                    continue;
                }

                possibleNames.Add(value.RawValue.ToString());
            }

            return new MemberIdentifier(possibleNames);
        }

        public override MemoryEntry ClassConstant(MemoryEntry thisObject, VariableName variableName)
        {
            throw new NotImplementedException();
        }

        public override MemoryEntry ClassConstant(QualifiedName qualifiedName, VariableName variableName)
        {
            throw new NotImplementedException();
        }

        public override ObjectValue CreateInitializedObject(TypeValue type)
        {
            return OutSet.CreateObject(type);
        }

        public override void DeclareGlobal(TypeDecl declaration)
        {
            var type = OutSet.CreateType(convertToType(declaration));
            OutSet.DeclareGlobal(type);
        }

        private ClassDecl convertToType(TypeDecl declaration)
        {
            ClassDeclBuilder result = new ClassDeclBuilder();
            result.BaseClasses = new List<QualifiedName>();
            if (declaration.BaseClassName.HasValue)
            {
                result.BaseClasses.Add(declaration.BaseClassName.Value.QualifiedName);
            }

            result.IsFinal = declaration.Type.IsFinal;
            result.IsInterface = declaration.Type.IsInterface;
            result.IsAbstract = declaration.Type.IsAbstract;
            result.QualifiedName = new QualifiedName(declaration.Name);



            foreach (var member in declaration.Members)
            {
                if (member is FieldDeclList)
                {
                    foreach (FieldDecl field in (member as FieldDeclList).Fields)
                    {
                        Visibility visibility;
                        if (member.Modifiers.HasFlag(PhpMemberAttributes.Private))
                        {
                            visibility = Visibility.PRIVATE;
                        }
                        else if (member.Modifiers.HasFlag(PhpMemberAttributes.Protected))
                        {
                            visibility = Visibility.PROTECTED;
                        }
                        else
                        {
                            visibility = Visibility.PUBLIC;
                        }
                        bool isStatic = member.Modifiers.HasFlag(PhpMemberAttributes.Static);
                        //multiple declaration of fields
                        if (result.Fields.ContainsKey(new FieldIdentifier(result.QualifiedName, field.Name)))
                        {
                            //dont need to set warning in simpleAnalysis
                            // setWarning("Cannot redeclare field " + field.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_FIELD_DECLARATION);
                        }
                        else
                        {
                            result.Fields.Add(new FieldIdentifier(result.QualifiedName, field.Name), new FieldInfo(field.Name, result.QualifiedName, "any", visibility, field.Initializer, isStatic));
                        }
                    }

                }
                else if (member is ConstDeclList)
                {
                    foreach (var constant in (member as ConstDeclList).Constants)
                    {
                        if (result.Constants.ContainsKey(new FieldIdentifier(result.QualifiedName, constant.Name)))
                        {
                            //dont need to set warning in simpleAnalysis
                            // setWarning("Cannot redeclare constant " + constant.Name, member, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION);
                        }
                        else
                        {
                            //in php all object constatns are public
                            Visibility visbility = Visibility.PUBLIC;
                            result.Constants.Add(new FieldIdentifier(result.QualifiedName, constant.Name), new ConstantInfo(constant.Name, result.QualifiedName, visbility, constant.Initializer));
                        }
                    }
                }
                else if (member is MethodDecl)
                {
                    var methosIdentifier = new MethodIdentifier(result.QualifiedName, (member as MethodDecl).Name);
                    if (!result.SourceCodeMethods.ContainsKey(methosIdentifier))
                    {
                        result.SourceCodeMethods.Add(methosIdentifier, OutSet.CreateFunction(member as MethodDecl, Flow.CurrentScript));
                    }
                    else
                    {
                        //dont need to set warning in simpleAnalysis
                        // setWarning("Cannot redeclare constant " + (member as MethodDecl).Name, member, AnalysisWarningCause.CLASS_MULTIPLE_CONST_DECLARATION);
                    }
                }
                else
                {
                    //ignore traits are not supported by AST, only by parser
                }
            }


            // NativeTypeDecl result=new NativeTypeDecl();

            return result.Build();
        }


        public override ReadWriteSnapshotEntryBase ResolveStaticField(GenericQualifiedName type, MemoryEntry field)
        {
            throw new NotImplementedException();
        }

        public override ReadWriteSnapshotEntryBase ResolveIndirectStaticField(MemoryEntry possibleTypes, MemoryEntry field)
        {
            throw new NotImplementedException();
        }

    }
}