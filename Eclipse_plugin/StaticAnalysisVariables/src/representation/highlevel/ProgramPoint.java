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


package representation.highlevel;

import java.util.ArrayList;

/**
 * Stores the contexts corresponding to a specific line from the analyzer. 
 * A ProgramPoint may hold multiple contexts, each of them having different call stack.
 * 
 * @author	Natalia Tyrpakova
 * @see		Context
 */
public class ProgramPoint {
	/**
	 * The file this ProgramPoint belongs to
	 */
	public File parentFile = null;
	/**
	 * The script name of the file this ProgramPoint belongs to
	 */
	public String scriptName;
	/**
	 * Truncated file path to the file this ProgramPoint belongs to. This path is relative to the workspace.
	 */
	public String resource = "";
	/**
	 * Point representing the position of this ProgramPoint in the source code
	 * @see Point
	 */
	public Point point;
	/**
	 * List of contexts this ProgramPoint holds
	 * @see Context
	 */
	public ArrayList<Context> contexts = new ArrayList<Context>();
	
	/**
	 * The constructor initializes the fields.
	 * 
	 * @param script		the owning script of this program point
	 * @param point			position of this program point
	 */
	public ProgramPoint(String script, Point point){
		scriptName = script;
		resource = script.substring(script.lastIndexOf('\\')+1,script.length());
		this.point = point;
	}

}