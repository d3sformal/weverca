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
using System.IO;

namespace Weverca.MemoryModels.UnitTest.SnapshotTestFramework
{
    public interface TestOperationLogger
    {
        void Init(object snapshot);
        void Close(object snapshot);

        void WriteLine(string line, params object[] args);
    }

    public class BlankLogger : TestOperationLogger
    {
        public void Init(object snapshot)
        {
        }

        public void Close(object snapshot)
        {
        }

        public void WriteLine(string line, params object[] args)
        {
        }
    }

    public class FileLogger : TestOperationLogger
    {
        String fileName;
        StreamWriter writer;

        public FileLogger(String fileName)
        {
            this.fileName = fileName;
        }

        public void Init(object snapshot)
        {
            writer = new StreamWriter(fileName);
        }

        public void Close(object snapshot)
        {
            writer.Close();
        }
        
        public void WriteLine(string line, params object[] args)
        {
            if (args.Length == 0)
            {
                writer.WriteLine(line);
            }
            else
            {
                writer.WriteLine(line, args);
            }
            if(args.Length!=0)
                writer.WriteLine(line, args);
            else
                writer.WriteLine(line);
        }
    }
}