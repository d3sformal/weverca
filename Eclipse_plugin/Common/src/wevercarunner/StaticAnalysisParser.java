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


package wevercarunner;

/**
 * Interface for parsing input of the static analysis. 
 * @author David Hauzar
 *
 */
public interface StaticAnalysisParser {
	public enum InputParts {
		VARIABLES, OVERVIEW, WARNINGS
	}
	
	/**
	 * Parses one line of the input.
	 * @param line line of the input to be parsed.
	 */
	public void parseLine(String line);
	
	/**
	 * Indicates whether there is nothing to be parsed.
	 * @return true if there is nothing to be parsed.
	 */
	public boolean parsingShouldEnd();
	
	/**
	 * Indicates which part of the input the parser parses.
	 * @return
	 */
	public InputParts partToParse();
}