/*
Copyright (c) 2012-2014 Marcel Kikta and David Hauzar.

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
using System.IO;
using System.Text;
using PHP.Core;
using PHP.Core.AST;
using PHP.Core.Parsers;
using Weverca.Parsers;
using Weverca.ControlFlowGraph;
using Weverca.Common;

namespace Weverca.ControlFlowGraph.Tester
{
    /******* Poznamka pro veverky:
     * Testovaci program (Weverca.Parser.Tester) je nastaven tak, ze pokud ho spustite bez parametru,
     * tak vezme soubory typu *.php z adresare PHP_sources v trunku, vyrobi pro ne CFG a ulozi je do souboru. 
     * Pokud budete chtít testovat vlastni sadu souboru, tak mu muzete nastavit libovolny pocet vlastnich 
     * souboru a adresaru pomoci parametru.
     * 
     * Vygenerovane soubory budou pojmenovany jako NAZEV_SOUBORU.cfg pro textovy vypis controll flow grafu
     * a NAZEV_SOUBORU.png pro obrazek. Soubory budou vytvoreny ve stejne slozce, jako je zpracovavany soubor.
     * 
     * Ve zdrojaku je zadratovana relativni cesta do trunk adresare, tak pokud se bude neco menit, tak se to
     * musi opravit i tady.
     * 
     * Jinak preji prijemne pouzivani programu =)
     * Pavel
     */

    class Program
    {
        //Used file extensions
        public static readonly string PHP_FILE_EXTENSION = ".php";
        public static readonly string GRAPH_FILE_EXTENSION = ".cfg";
        public static readonly string IMAGE_FILE_EXTENSION = ".png";

        /// <summary>
        /// Using the phalanger parser generates ControlFlowGraph for a given file.
        /// </summary>
        /// <param name="fileName">Name of the file with php source.</param>
        /// <returns></returns>
        static Weverca.ControlFlowGraph.ControlFlowGraph GenerateCFG(string fileName)
        {
            try
            {
                var file = new FileInfo(fileName);
                return ControlFlowGraph.FromFile(file);
            }
            catch (Weverca.ControlFlowGraph.ControlFlowException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        /// <summary>
        /// Generates CFG and image and stores them into the folder with input file.
        /// Names of generated files is:
        ///     Text CFG representation - fileName.GRAPH_FILE_EXTENSION
        ///     Image CFG representation - fileName.IMAGE_FILE_EXTENSION
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        static void ProcessFile(string fileName)
        {
            System.Diagnostics.Debug.Assert(File.Exists(fileName));

            Console.WriteLine("Processing file: {0}", fileName);
            Weverca.ControlFlowGraph.ControlFlowGraph cfg = GenerateCFG(fileName);

            if (cfg == null)
            {
                return;
            }

            //Saves CFG representation into the file
            string cfgFileName = fileName + GRAPH_FILE_EXTENSION;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(cfgFileName))
            {
                file.WriteLine(cfg.getTextRepresentation());
            }

            //Runs the graphviz component
            System.Diagnostics.Process imageMaker = new System.Diagnostics.Process();
            imageMaker.StartInfo.FileName = TrunkStructure.GRAPHVIZ_PATH;
            imageMaker.StartInfo.Arguments = "-Tpng " + cfgFileName;
            imageMaker.StartInfo.UseShellExecute = false;
            imageMaker.StartInfo.RedirectStandardOutput = true;
            imageMaker.StartInfo.RedirectStandardError = true;
            imageMaker.Start();

            //And writes the generated image into file
            string imageFileName = fileName + IMAGE_FILE_EXTENSION;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(imageFileName))
            {
                imageMaker.StandardOutput.BaseStream.CopyTo(file.BaseStream);
            }
        }

        /// <summary>
        /// For each PHP sources in the given directory calls ProcessFile method.
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        static void ProcessDirectory(string directoryName)
        {
            System.Diagnostics.Debug.Assert(Directory.Exists(directoryName));

            foreach (string fileName in Directory.EnumerateFiles(directoryName))
            {
                string fileExtension = Path.GetExtension(fileName);

                if (fileExtension == PHP_FILE_EXTENSION)
                {
                    ProcessFile(fileName);
                }
            }
        }

        static void Main(string[] args)
        {
            //If there is some args process all of them
            if (args.Length > 0)
            {
                foreach (string path in args)
                {
                    if (Directory.Exists(path))
                    {
                        ProcessDirectory(path);
                    }
                    else if (File.Exists(path))
                    {
                        ProcessFile(path);
                    }
                }
            }
            //Or just process folder with test PHP sources
            else
            {
                ProcessDirectory(TrunkStructure.PHP_SOURCES_DIR);
            }
        }
    }
}