/*
Copyright (c) 2012-2014 Marcel Kikta, David Skorvaga, Matyas Brenner, and David Hauzar

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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.VirtualReferenceModel;

namespace Weverca.Analysis.UnitTest
{
    /// <summary>
    /// Summary description for MemoryAssistantTester
    /// </summary>
    [TestClass]
    public class MemoryAssistantTester
    {

        [TestMethod]
        public void SimplifyIntTest()
        {
            var assistant = new MemoryAssistant();
            var snapshot = new Snapshot();
            assistant.InitContext(snapshot);
            var result=assistant.Simplify(new MemoryEntry(new Value[] { snapshot.CreateInt(0), snapshot.CreateInt(1), snapshot.CreateInt(3), snapshot.CreateInt(4) }));
            List<Value> res = new List<Value>(result.PossibleValues);
            Debug.Assert(res.Count == 1);
            Debug.Assert(res[0] is IntegerIntervalValue);
            Debug.Assert((res[0] as IntegerIntervalValue).Start == 0);
            Debug.Assert((res[0] as IntegerIntervalValue).End == 4);
        }

        [TestMethod]
        public void SimplifyLongTest()
        {
            var assistant = new MemoryAssistant();
            var snapshot = new Snapshot();
            assistant.InitContext(snapshot);
            var result = assistant.Simplify(new MemoryEntry(new Value[] { snapshot.CreateLongintInterval(0,10),snapshot.CreateLong(-10) }));
            List<Value> res = new List<Value>(result.PossibleValues);
            Debug.Assert(res.Count == 1);
            Debug.Assert(res[0] is LongintIntervalValue);
            Debug.Assert((res[0] as LongintIntervalValue).Start == -10);
            Debug.Assert((res[0] as LongintIntervalValue).End == 10);
        }


        [TestMethod]
        public void SimplifyDoubleTest()
        {
            var assistant = new MemoryAssistant();
            var snapshot = new Snapshot();
            assistant.InitContext(snapshot);
            var result = assistant.Simplify(new MemoryEntry(new Value[] { snapshot.AnyFloatValue, snapshot.CreateFloatInterval(0,5)}));
            List<Value> res = new List<Value>(result.PossibleValues);
            Debug.Assert(res.Count == 1);
            Debug.Assert(res[0] is FloatIntervalValue);
            Debug.Assert((res[0] as FloatIntervalValue).Start == double.MinValue);
            Debug.Assert((res[0] as FloatIntervalValue).End == double.MaxValue);
        }

        [TestMethod]
        public void SimplifyStringTest()
        {
            var assistant = new MemoryAssistant();
            var snapshot = new Snapshot();
            assistant.InitContext(snapshot);
            var result = assistant.Simplify(new MemoryEntry(new Value[] { snapshot.AnyStringValue, snapshot.CreateString("a"), snapshot.CreateString("ap") }));
            List<Value> res = new List<Value>(result.PossibleValues);
            Debug.Assert(res.Count == 1);
            Debug.Assert(res[0] is AnyStringValue);

        }

        [TestMethod]
        public void SimplifyBoolTest()
        {
            var assistant = new MemoryAssistant();
            var snapshot = new Snapshot();
            assistant.InitContext(snapshot);
            var result = assistant.Simplify(new MemoryEntry(new Value[] { snapshot.CreateBool(true), snapshot.CreateBool(true), snapshot.CreateBool(true) }));
            List<Value> res = new List<Value>(result.PossibleValues);
            Debug.Assert(res.Count == 1);
            Debug.Assert(res[0] is BooleanValue);
            Debug.Assert((res[0] as BooleanValue).Value);

        }


        [TestMethod]
        public void SimplifyBoolTest2()
        {
            var assistant = new MemoryAssistant();
            var snapshot = new Snapshot();
            assistant.InitContext(snapshot);
            var result = assistant.Simplify(new MemoryEntry(new Value[] { snapshot.CreateBool(false), snapshot.CreateBool(true), snapshot.CreateBool(true) }));
            List<Value> res = new List<Value>(result.PossibleValues);
            Debug.Assert(res.Count == 1);
            Debug.Assert(res[0] is AnyBooleanValue);

        }

    }
}