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
		public static readonly string TRUNK_PATH = @"..\..\..\..\..\"; // On windows
		//public static readonly string TRUNK_PATH = @"../../../../../"; // On linux

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
