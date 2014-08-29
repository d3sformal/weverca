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

import wevercarunner.StaticAnalysisParser;

public class OverviewParser implements StaticAnalysisParser {
	private StaticAnalysis_Runner runner;
	// Total number of items to parse
	
	public OverviewParser(StaticAnalysis_Runner runner) {
		this.runner = runner;
	}
	
	@Override
	public void parseLine(String line) {
		if (line.contains("Total number of warnings:"))
			runner.warningsNum = Integer.parseInt(line.substring(line.indexOf(':')+2));
		
		if (line.contains("Number of warnings in the first phase:")) {
			runner.warningsFirstPhaseNum = Integer.parseInt(line.substring(line.indexOf(':')+2));
		}
		
		if (line.contains("Number of warnings in the second phase:")) {
			runner.warningsSecondPhaseNum = Integer.parseInt(line.substring(line.indexOf(':')+2));
		}
		
		if (line.contains("Weverca analyzer time consumption:")) {
			runner.wevercaTime = Long.parseLong(line.substring(line.indexOf(':')+2));
		}
		
		if (line.contains("First phase time consumption:")) {
			runner.firstPhaseTime = Long.parseLong(line.substring(line.indexOf(':')+2));
		}
		
		if (line.contains("Second phase time consumption:")) {
			runner.secondPhaseTime = Long.parseLong(line.substring(line.indexOf(':')+2));
		}
		
		if (line.contains("The number of nodes in the application is:")) {
			runner.numberOfPPoints = Integer.parseInt(line.substring(line.indexOf(':')+2));
		}
		
	}
	
	@Override
	public boolean parsingShouldEnd() {
		return false;
	}

	@Override
	public InputParts partToParse() {
		return InputParts.OVERVIEW;
	}
}