using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    class AlgorithmKey
    {
        IAlgorithm algorithmInstance;
        AlgorithmType algorithmType;

        public AlgorithmKey(AlgorithmType algorithmType, IAlgorithm algorithmInstance = null)
        {
            this.algorithmType = algorithmType;
            this.algorithmInstance = algorithmInstance;
        }

        public override bool Equals(object obj)
        {
            AlgorithmKey key = obj as AlgorithmKey;

            if (key == null)
            {
                return false;
            }

            if (algorithmType != key.algorithmType)
            {
                return false;
            }

            if (algorithmInstance != null)
            {
                return algorithmInstance.Equals(key.algorithmInstance);
            }
            else
            {
                return key.algorithmInstance == null;
            }
        }

        public override int GetHashCode()
        {
            int hash = algorithmType.GetHashCode();

            if (algorithmInstance != null)
            {
                hash ^= algorithmInstance.GetHashCode();
            }

            return hash;
        }
    }
}
