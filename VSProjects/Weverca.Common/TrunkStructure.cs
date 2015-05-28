/*
Copyright (c) 2012-2014 Matyas Brenner.

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

namespace Weverca.Common
{
    /// <summary>
    /// Static class with properties that must be changed in case of changes in svn directory structure
    /// </summary>
    public static class TrunkStructure
    {
        /// <summary>
        /// Relative path to the trunk folder in SVN
        /// </summary>
		public static readonly string TRUNK_PATH = @"..\..\..\..\"; // On windows
		//public static readonly string TRUNK_PATH = @"../../../../"; // On linux

        /// <summary>
        /// Path to the graphviz tool
        /// </summary>
		public static readonly string GRAPHVIZ_PATH = TRUNK_PATH + @"Tools\dot_graphviz\dot.exe"; // On windows
		//public static readonly string GRAPHVIZ_PATH = @"dot"; // On linux

        /// <summary>
        /// Directory with PHP sources
        /// </summary>
		public static readonly string PHP_SOURCES_DIR = TRUNK_PATH + @"PHP_sources\"; // On windows
		//public static readonly string PHP_SOURCES_DIR = TRUNK_PATH + @"PHP_sources/"; // On linux
    }
}