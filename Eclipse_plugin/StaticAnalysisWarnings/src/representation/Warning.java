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

import general.FilePath;

import java.util.ArrayList;

import callstacks.Call;

/**
 * This class stores a warning information.
 * 
 * @author Natalia Tyrpakova
 * 
 */
public class Warning {
	/**
	 * The warning's description
	 */
	public String description;
	/**
	 * Path to the file containing the code that raised this warning.
	 */
	public String filePath;
	/**
	 * Truncated path to the file containing the code that raised this warning. 
	 * This path is relative to the open workspace.
	 */
	public String resource;
	/**
	 * Line containing the code that raised this warning. 
	 */
	public int atLine;
	/**
	 * First offset of the code that raised this warning. 
	 */
	public int firstOffset;
	/**
	 * Last offset of the code that raised this warning. 
	 */
	public int lastOffset;
	/**
	 * Indicator of security warning.
	 */
	public boolean security;
	/**
	 * Indicator of high priority
	 */
	public boolean highPriority = false;
	/**
	 * Possible call stacks that lead to this warning
	 */
	public ArrayList<Call> calledFrom = new ArrayList<Call>();
	/**
	 * Taint flows connected with this warning. This field is null in case of a non-security warning. 
	 */
	public TaintFlows taintFlows;
	
	private int atChar;
	
	/**
	 * The constructor saves all the warning information
	 * 
	 * @param securityWarning	true if the warning is a security warning
	 * @param path				a line from the analyzer output that contains the file path
	 * @param info				a line from the analyzer output that contains the warning information
	 */
	Warning(boolean securityWarning, String path, String info){
		security = securityWarning;
		setPath(path);
		setWarning(info);
	}
	
	/**
	 * Parses the line from analyzer output to get the file path 
	 * 
	 * @param line		a line from the analyzer output that contains the file path
	 */
	private void setPath(String line){
		filePath = "";
		if (line.length() > 6){
			filePath = line.substring(6).replace("\\","/");
		}
		resource = FilePath.truncate(filePath); 
	}
	
	/**
	 * Parses the line from analyzer output to get the warning information
	 * 
	 * @param line		a line from the analyzer output that contains the warning information
	 */
	private void setWarning(String line){
		if (line.length() > 16){
			String delims = " ";
			String[] tokens = line.split(delims);
			if (tokens.length < 11) return;
			atLine = Integer.parseInt(tokens[3]);
			atChar = Integer.parseInt(tokens[5]);
			firstOffset = Integer.parseInt(tokens[7]);
			lastOffset = Integer.parseInt(tokens[9]);
			StringBuilder desc = new StringBuilder("");
			for (int t = 11; t< tokens.length; t++){
				desc.append(tokens[t]);
				desc.append(" ");
			}
			desc.deleteCharAt(desc.length()-1);
			description = desc.toString();
		}
	}
	
	/**
	 * The method determines whether this instance is equal to the argument. It is only equal 
	 * if all the fields are equal.
	 * 
	 * @param o		object to be compared
	 * @return		true if the objects are equal to this instance
	 */
	@Override
	public boolean equals(Object o){
		if (o == null) return false;
		if (o == this) return true;
		if (!(o instanceof Warning)) return false;
		Warning w = (Warning)o;
		if (w.calledFrom.size() != this.calledFrom.size()) return false;
		for (Call call: w.calledFrom){
			if (!this.calledFrom.contains(call)) return false;
		}
		if (w.description.equals(this.description) && 
			w.filePath.equals(this.filePath) &&
			(w.atLine == this.atLine) &&
			(w.atChar == this.atChar) &&
			(w.security == this.security)){
			return true;
		}
		return false;
	}
	
	/**
	 * Sets whether the resulting flows should be shown as merged or reversed
	 * @param merge true if flows should be merged
	 * @param reverse true if flows should be reversed
	 */
	public void setMerge(boolean merge,boolean reverse){
		if (taintFlows != null) {
			taintFlows.merge = merge;
			taintFlows.reverse = reverse;
		}
	}
	

}