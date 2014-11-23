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
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm
{
    class CopyAlgorithmFactories
    {
        private static readonly AlgorithmFactories copyAlgorithmFactories = new AlgorithmFactoriesBuilder()
        {
            AssignAlgorithmFactory = new CopyAssignAlgorithm(),
            CommitAlgorithmFactory = new TrackingCommitAlgorithm(),
            MemoryAlgorithmFactory = new SimplifyingCopyMemoryAlgorithm(),
            //MergeAlgorithmFactory = new CopyMergeAlgorithm(),
            MergeAlgorithmFactory = new TrackingMergeAlgorithm(),
            ReadAlgorithmFactory = new CopyReadAlgorithm(),
            PrintAlgorithmFactory = new PrintAlgorithm()
        }.Build();

        public static AlgorithmFactories Factories { get { return copyAlgorithmFactories; } }
    }
}