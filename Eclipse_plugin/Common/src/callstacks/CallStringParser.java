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


package callstacks;

import java.util.ArrayList;
import java.util.Stack;

/**
 * Parser for the PHP code call stacks
 * 
 * @author Natalia Tyrpakova
 */
public class CallStringParser {
	
	/**
	 * Parses a call stack into separate calls that are stored hierarchically.
	 *  
	 * @param string		call stack as a String
	 * @return				list of resulting Calls
	 * @see Call
	 */
	public static ArrayList<Call> parse(String string){
		Stack<Call> stack = new Stack<Call>();
		ArrayList<Call> result = new ArrayList<Call>();
		int it = 0; //string iterator
		
		int length = string.length();
		while (it < length){
			if(it < length-3 && string.substring(it,it+3).equals("->(")){ //new call
				int filePathEnd = string.indexOf(" at position ",it);
				String filePath = string.substring(it+3,filePathEnd);
				filePath = filePath.replace("\\","/");
				
				int firstPositionStart = filePathEnd + 14; // index of the first digit of first position
				int firstPositionEnd = string.indexOf(')', firstPositionStart);
				String firstPosition = string.substring(firstPositionStart, firstPositionEnd); 
				String[] positions = firstPosition.split(",");
				int firstLine = -1;
				int firstCol = -1;
				if (positions.length > 1){
					firstLine = Integer.parseInt(positions[0]);
					firstCol = Integer.parseInt(positions[1]);
				}
				
				int secondPositionStart = firstPositionEnd + 3; // index of the first digit of second position
				int secondPositionEnd = string.indexOf(')', secondPositionStart);
				String secondPosition = string.substring(secondPositionStart, secondPositionEnd); 
				positions = secondPosition.split(",");
				int lastLine = -1;
				int lastCol = -1;
				if (positions.length > 1){
					lastLine = Integer.parseInt(positions[0]);
					lastCol = Integer.parseInt(positions[1]);
				}
				
				it = secondPositionEnd+1;
					
				Call newCall = new Call(filePath,firstLine,lastLine,firstCol,lastCol);
				if (!stack.isEmpty()){
					newCall.parentCall = stack.peek();
					stack.peek().childrenCalls.add(newCall);
				}
				stack.push(newCall);
			}
			else if (string.charAt(it) == ')'){
				Call pop = stack.pop();
				if (stack.empty()) result.add(pop);
				it++;
			}
			else if (it < length-2 && string.substring(it,it+2).equals("or")){
				it = it+2;
			}
		}
		
		return result;
	}
	


}