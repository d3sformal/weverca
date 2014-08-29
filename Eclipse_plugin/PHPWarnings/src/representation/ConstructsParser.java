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

import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import exceptions.AnalyzerNotFoundException;
import wevercarunner.CollectingOutputProcessor;
import wevercarunner.Runner;


/**
 * This class gets the construct occurrences using the Runner and parses them. It only has one static method that returns
 * the construct occurrences of specified constructs in given resource.
 * 
 * @author Natalia Tyrpakova
 */
public class ConstructsParser {
	
	/**
	 * This method gets occurrences of given constructs in given resource.
	 * 
	 * @param source		path to the file to be searched for constructs
	 * @param constructs	construct types to be searched for
	 * @param editorPath	actual file that is searched for constructs  (does not have to be file passed to Weverca)
	 * @return				ConstructsInfo with search result
	 * @throws IOException
	 * @throws AnalyzerNotFoundException 
	 * @see		ConstructsInfo
	 */
	public static ConstructsInfo GetConstructOccurances(String source,ArrayList<String> constructs,String editorPath) throws IOException, AnalyzerNotFoundException
	{		
		ArrayList<String> parameters = new ArrayList<String>(Arrays.asList("-cmide", "-constructs"));
		parameters.addAll(constructs);
		parameters.add(source);
		
		Runner runner = new Runner();
		CollectingOutputProcessor out = new CollectingOutputProcessor();
		runner.runWeverca(parameters, out);
		List<String> output = out.getOutput();
		
		ConstructsInfo constructInfo = new ConstructsInfo();

		for (String line: output){
			if (line.equals("error")) return null;
			if (line.length() > 10){
				String substr = line.substring(0, 7);
				if (!substr.equals("Process")) {
					constructInfo.ProcessLine(line,editorPath);
				}
			}
		}	
		return constructInfo;
	}
}