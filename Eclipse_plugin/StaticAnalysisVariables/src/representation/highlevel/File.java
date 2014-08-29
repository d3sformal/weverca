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

import java.util.HashMap;

/**
 * An object that stores the ProgramPoints from one file. The ProgramPoints are stored
 * in a HashMap that maps the line number to a ProgramPoint information.
 * 
 * @author Natalia Tyrpakova
 */
public class File {
	/**
	 * HashMap of lines and corresponding ProgramPoints
	 * @see ProgramPoint
	 */
	public HashMap<Integer,ProgramPoint> programPoints = new HashMap<Integer,ProgramPoint>();
	/**
	 * File path to this file
	 */
	public String filePath = "";
	
	/**
	 * The constructor initializes the file name.
	 * 
	 * @param path		file path
	 */
	public File(String path){
		filePath = path;
	}
	
	/**
	 * Adds a new ProgramPoint to the stored HashMap.
	 * 
	 * @param p		ProgramPoint to add
	 */
	public void add(ProgramPoint p){
		programPoints.put( p.point.firstLine, p);
	}
	
}