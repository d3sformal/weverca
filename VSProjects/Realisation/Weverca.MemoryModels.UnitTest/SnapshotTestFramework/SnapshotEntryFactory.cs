/*
Copyright (c) 2012-2014 Pavel Bastecky.

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
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.UnitTest.SnapshotTestFramework
{
    public class SnapshotEntryFactory<T> where T : SnapshotBase, new()
    {
        private ReadWriteSnapshotEntryBase snapshotEntry;
        private SnapshotTester<T> tester;

        public SnapshotEntryFactory(SnapshotTester<T> tester, ReadWriteSnapshotEntryBase readWriteSnapshotEntryBase)
        {
            this.snapshotEntry = readWriteSnapshotEntryBase;
            this.tester = tester;
        }

        public SnapshotEntryFactory<T> Index(params string[] indexes)
        {
            return new SnapshotEntryFactory<T>(tester, snapshotEntry.ReadIndex(tester.Snapshot, new MemberIdentifier(indexes)));
        }

        public SnapshotEntryFactory<T> Field(params string[] fields)
        {
            return new SnapshotEntryFactory<T>(tester, snapshotEntry.ReadField(tester.Snapshot, new VariableIdentifier(fields)));
        }

        public ReadWriteSnapshotEntryBase getEntry()
        {
            return snapshotEntry;
        }


        public void Read()
        {
            tester.Read(getEntry());
        }

        public void Write(SnapshotEntryFactory<T> source)
        {
            tester.Write(getEntry(), source.snapshotEntry);
        }

        public void Write(Value value)
        {
            tester.Write(getEntry(), value);
        }

        public void Write(int value)
        {
            tester.Write(getEntry(), value);
        }

        public void Write(string value)
        {
            tester.Write(getEntry(), value);
        }

        internal void Alias(SnapshotEntryFactory<T> source)
        {
            tester.Alias(getEntry(), source.snapshotEntry);
        }
    }
}