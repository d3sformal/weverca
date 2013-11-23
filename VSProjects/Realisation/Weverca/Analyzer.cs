﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.Analysis;
using Weverca.AnalysisFramework;
using Weverca.Parsers;

namespace Weverca
{
    internal static class Analyzer
    {
        /// <summary>
        /// Perform static analysis on a file with PPG as the result
        /// </summary>
        /// <param name="entryFile">File with PHP source</param>
        /// <returns>PPG generated by forward analysis</returns>
        internal static ProgramPointGraph Run(FileInfo entryFile)
        {
            var cfg = GenerateCfg(entryFile);
            return Analyze(cfg);
        }

        /// <summary>
        /// Analyses CFG and return PPG generated by forward analysis
        /// </summary>
        /// <param name="entryMethod">Start point CFG of PHP code</param>
        /// <returns>PPG generated by forward analysis</returns>
        internal static ProgramPointGraph Analyze(ControlFlowGraph.ControlFlowGraph entryMethod)
        {
            var analysis = new ForwardAnalysis(entryMethod, MemoryModels.MemoryModels.VirtualReferenceMM);

            analysis.Analyse();

            return analysis.ProgramPointGraph;
        }

        /// <summary>
        /// Using the Phalanger parser generates <see cref="ControlFlowGraph"/> for a given file.
        /// </summary>
        /// <param name="fileInfo">File with PHP source</param>
        /// <returns>Created ControlFlowGraph</returns>
        internal static ControlFlowGraph.ControlFlowGraph GenerateCfg(FileInfo fileInfo)
        {
            var fileName = fileInfo.FullName;
            string code;
            using (var reader = new StreamReader(fileName))
            {
                code = reader.ReadToEnd();
            }

            var directoryPath = new FullPath(fileInfo.DirectoryName);
            var filePath = new FullPath(fileInfo.FullName);
            var sourceFile = new PhpSourceFile(directoryPath, filePath);
            var parser = new SyntaxParser(sourceFile, code);
            parser.Parse();

            return new ControlFlowGraph.ControlFlowGraph(parser.Ast);
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
