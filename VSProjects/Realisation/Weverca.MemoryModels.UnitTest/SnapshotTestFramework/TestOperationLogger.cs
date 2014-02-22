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
