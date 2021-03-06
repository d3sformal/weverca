/*
Copyright (c) 2012-2014 David Hauzar, Miroslav Vodolan, Marcel Kikta, Pavel Bastecky, David Skorvaga, and Matyas Brenner

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
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.Analysis;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.Parsers;

namespace Weverca
{

    /// <summary>
    /// Static class, which contains functionality for rununy analysis and generating cgf
    /// </summary>
    public static class Analyzer
    {
        /// <summary>
        /// Perform static analysis on a file with PPG as the result
        /// </summary>
        /// <param name="entryFile">File with PHP source</param>
        /// <param name="memoryModel">The memory model used for analysis</param>
        /// <returns>PPG generated by forward analysis</returns>
        public static ProgramPointGraph Run(FileInfo entryFile, MemoryModels.MemoryModelFactory memoryModel)
        {
            var cfg = GenerateCfg(entryFile);
            return Analyze(cfg, memoryModel);
        }

        /// <summary>
        /// Analyses CFG and return PPG generated by forward analysis
        /// </summary>
        /// <param name="entryMethod">Start point CFG of PHP code</param>
        /// <param name="memoryModel">The memory model used for analysis</param>
        /// <returns>PPG generated by forward analysis</returns>
        internal static ProgramPointGraph Analyze(ControlFlowGraph.ControlFlowGraph entryMethod, MemoryModels.MemoryModelFactory memoryModel)
        {
            //ForwardAnalysisBase forwardAnalysis = analysis.CreateAnalysis(entryMethod, memoryModel);
            ForwardAnalysisBase forwardAnalysis = new Weverca.Analysis.ForwardAnalysis(entryMethod, memoryModel);
            forwardAnalysis.Analyse();
            return forwardAnalysis.ProgramPointGraph;
        }

        /// <summary>
        /// Using the Phalanger parser generates <see cref="ControlFlowGraph"/> for a given file.
        /// </summary>
        /// <param name="fileInfo">File with PHP source</param>
        /// <returns>Created ControlFlowGraph</returns>
        internal static ControlFlowGraph.ControlFlowGraph GenerateCfg(FileInfo fileInfo)
        {
            return ControlFlowGraph.ControlFlowGraph.FromFile(fileInfo);
        }

        /// <summary>
        /// Checks path format and find all files matching the given pattern path
        /// </summary>
        /// <param name="path">Pattern of file paths</param>
        /// <returns>
        /// Information about files matching pattern path,
        /// or <c>null</c> if <paramref name="path" /> is not correct
        /// </returns>
        internal static FileInfo[] GetFileNames(string path)
        {
            Debug.Assert(path != null, "Path is passed from command line and cannot be null");

            string directoryName;
            try
            {
                directoryName = Path.GetDirectoryName(path);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (PathTooLongException)
            {
                throw;
            }

            // The path contains root directory
            if (directoryName == null)
            {
                return null;
            }

            DirectoryInfo directoryInfo;
            try
            {
                // If directory name is empty string, path is relative
                directoryInfo = new DirectoryInfo((directoryName.Length <= 0) ? "." : directoryName);
            }
            catch (SecurityException)
            {
                // Directory does not exist or user has not rights to access it
                throw;
            }

            var fileName = Path.GetFileName(path);
            if (fileName.Length <= 0)
            {
                return null;
            }

            FileInfo[] filesInfo;
            try
            {
                filesInfo = directoryInfo.GetFiles(fileName, SearchOption.TopDirectoryOnly);
            }
            catch (SecurityException)
            {
                // Files do not exist or user has not rights to access them
                throw;
            }
            catch
            {
                // No file match
                return new FileInfo[0];
            }

            return filesInfo;
        }
    }
}