/*
Copyright (c) 2012-2014 Natalia Tyrpakova

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


package staticanalysis;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Hashtable;
import java.util.LinkedList;
import java.util.List;

import exceptions.AnalyzerNotFoundException;
import wevercarunner.OutputProcessor;
import wevercarunner.Runner;
import wevercarunner.StaticAnalysisParser;
import wevercarunner.StaticAnalysisParser.InputParts;

/**
 * This class serves as a connection between this plug-in and Weverca analyzer. 
 * It gets the static analysis result using the Runner class.
 * 
 * @author Natalia Tyrpakova
 *
 */
class StaticAnalysis_Runner {
	/**
	 * Time of the first phase analysis in ms
	 */
	long firstPhaseTime;
	/**
	 * Time of the taint analysis in ms
	 */
	long secondPhaseTime;
	/**
	 * Overall time of Weverca analyzer analysis (includes printing the output)
	 */
	long wevercaTime;
	/**
	 * Total number of warnings
	 */
	int warningsNum;
	/**
	 * Number of warnings in the first phase
	 */
	int warningsFirstPhaseNum;
	/**
	 * Number of warnings in the second phase
	 */
	int warningsSecondPhaseNum;
	
	/**
	 * Number of processed program points in Weverca analyzer
	 */
	int numberOfPPoints;
	private Runner runner;
	
	/**
	 * The function takes a list of files and gets them analyzed. 
	 * 
	 * @param fileNames		paths of the files to be analyzed
	 * @throws IOException
	 * @throws AnalyzerNotFoundException 
	 */
	void computeAnalysisResult(ArrayList<String> fileNames, StaticAnalysisParser[] parsers) throws IOException, AnalyzerNotFoundException{
		ArrayList<String> parameters = new ArrayList<String>(Arrays.asList("-cmide", "-staticanalysis"));
		parameters.addAll(fileNames);
				
		runner = new Runner();
		List<StaticAnalysisParser> listParsers = new LinkedList<StaticAnalysisParser>(Arrays.asList(parsers));
		listParsers.add(new OverviewParser(this));
		runner.runWeverca(parameters, new StaticAnalysisOptimizingOutput(listParsers));
	}
	
	/**
	 * Stops the Runner
	 */
	void stopRunner(){
		if (runner != null) runner.stopProcess();
	}
	
	private static class StaticAnalysisOutput implements OutputProcessor {
		private List<StaticAnalysisParser> parsers;
		
		private StaticAnalysisOutput(List<StaticAnalysisParser> parsers) {
			this.parsers = parsers;
		}
		
		@Override
		public void processLine(String line) {
			for (StaticAnalysisParser parser : parsers) {
				if (!parser.parsingShouldEnd()) parser.parseLine(line);
			}
			
		}
	}
	
	private static class StaticAnalysisOptimizingOutput implements OutputProcessor {
		private Hashtable<InputParts, StaticAnalysisParser> parsers;
		private InputParts currentPart;
		private StaticAnalysisParser currentParser;
		
		private StaticAnalysisOptimizingOutput(List<StaticAnalysisParser> listParsers) {
			parsers = new Hashtable<>(listParsers.size());
			for (StaticAnalysisParser parser : listParsers) {
				parsers.put(parser.partToParse(), parser);
			}
			
			currentPart = InputParts.WARNINGS;
			currentParser = parsers.get(currentPart);
		}
		
		@Override
		public void processLine(String line) {
			if (currentPart == InputParts.WARNINGS && line.contains("Variables:")) {
				currentPart = InputParts.VARIABLES;
				currentParser = parsers.get(currentPart);
			} else if (currentPart == InputParts.VARIABLES && line.contains("Overview:")) {
				currentPart = InputParts.OVERVIEW;
				currentParser = parsers.get(currentPart);
			}
			
			//if (!currentParser.parsingShouldEnd()) 
			currentParser.parseLine(line);
		}
	}
	
}