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


package representation;

import java.util.ArrayList;

import wevercarunner.StaticAnalysisParser;
import wevercarunner.StaticAnalysisParser.InputParts;
import callstacks.Call;
import callstacks.CallStringParser;
import general.Messages;


/**
 * This class parses the static analysis result into the warnings and their calls and taint flows
 * 
 * @author Natalia Tyrpakova
 * @author David Hauzar
 *
 */
public class WarningsParser implements StaticAnalysisParser {
	private ArrayList<Warning> warnings = new ArrayList<Warning>();
	private boolean securityWarnings = false;
	private Warning warning = null;
	private String path = "";
	private boolean taintFlowIndicator = false;
	private ArrayList<String> taintFlow = new ArrayList<String>();
	private boolean variables = true;
	private boolean flowsSplitted = false;
	
	private boolean endParsing = false;
		
	/**
	 * Gets the parsed warnings that were found by the analyzer. 
	 * Should be called after parsing was finished.
	 * @return warnings.
	 * @see					Warning
	 */
	public ArrayList<Warning> getWarnings() {
		if (!flowsSplitted) {
			for (Warning w : warnings){
				if (w.taintFlows != null){
					w.taintFlows.splitFlows();
				}
			}
			flowsSplitted = true;
		}
		return warnings;
	}
	
	/**
	 * Indicates whether the parsing should end
	 * @return true if parsing should end.
	 */
	public boolean parsingShouldEnd() { return endParsing; }
	
	
	public void parseLine(String line) {
		if (line.equals("error")){
			Messages.staticAnalysisFailed(path);
			endParsing = true;
		}
			
		if (line.equals("")) return;
		if (line.equals("Analysis warnings:")){
			securityWarnings = false;
			variables = false;
			return;
		}
		if (variables) return;
		
		if (line.equals("Security warnings with taint flow:")){
			securityWarnings = true;
			return;
		}
		
		if (line.equals("Variables:")){
			variables = true;
			return;
		}
		
		if (line.equals("High priority")){
			Warning lastWarning = warnings.get(warnings.size()-1);
			lastWarning.highPriority = true;
		}
		
		if (line.equals("Possible flow: ")){
			taintFlowIndicator = true;
			taintFlow = new ArrayList<String>();
			return;
		}
		
		if (line.equals("End flow")){
			taintFlowIndicator = false;
			Warning lastWarning = warnings.get(warnings.size()-1);
			if (lastWarning.taintFlows == null) lastWarning.taintFlows = new TaintFlows();
			lastWarning.taintFlows.addFlow(taintFlow);
			lastWarning.taintFlows.parent = lastWarning;
			return;
		}
		
		if (taintFlowIndicator){
			taintFlow.add(line);
			return;
		}
		
		if (line.length()>5 && line.substring(0,5).equals("File:")){
			path = line;
			return;
		}
		
		if (line.length()>7 && line.substring(0,7).equals("Warning")){
			warning = new Warning(securityWarnings,path,line);
			if (!warnings.contains(warning)) { 
				warnings.add(warning);
			}
			return;
		}
		
		if (line.length()>2 && line.substring(0,2).equals("->")){
			ArrayList<Call> calls = CallStringParser.parse(line);
			Warning lastWarning = warnings.get(warnings.size()-1);
			for (Call call :calls){
				call.parent = lastWarning;
			}
			lastWarning.calledFrom = calls;	
			return;
		}
	}
	
	@Override
	public InputParts partToParse() {
		return InputParts.WARNINGS;
	}
	
		
}