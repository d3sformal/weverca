using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign
{
    class AssignOperation
    {
        private IndexCollectors.CollectorNode childNode;

        public AssignOperation(IndexCollectors.CollectorNode childNode)
        {
            // TODO: Complete member initialization
            this.childNode = childNode;
        }

        public IndexCollectors.CollectorNode CollectorNode { get; set; }
    }
}
