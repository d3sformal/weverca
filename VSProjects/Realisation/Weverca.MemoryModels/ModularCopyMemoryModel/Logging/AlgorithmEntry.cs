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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    class AlgorithmEntry
    {
        private AlgorithmType algorithmType;

        public int Starts { get; set; }
        public int Stops { get; set; }
        public double Time { get; set; }
        public double MemoryStart { get; set; }
        public double MemoryStop { get; set; }
        public double Memory { get; set; }

        public AlgorithmEntry(AlgorithmType algorithmType)
        {
            this.algorithmType = algorithmType;
        }


    }
}