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

/**
 * Stores all the constructs found in one file as an ArrayList of Constructs.
 * @author Natalia Tyrpakova
 * @see		Construct
 */
public class FileInfo {
	public String path;
	public ArrayList<Construct> constructs;
	
	/**
	 * The constructor sets the file which information will be stored.
	 * 
	 * @param path		path to the file
	 */
	FileInfo(String path){
		this.path = path;
		constructs = new ArrayList<Construct>();
	}
	
	/**
	 * Adds a new construct to this class
	 * 
	 * @param type			construct type
	 * @param firstoffset	offset of the first character of this construct
	 * @param lastoffset	offset of the last character of this construct
	 */
	void addConstruct(String type, int firstoffset, int lastoffset ){
		Construct construct = new Construct(type,firstoffset,lastoffset);
		constructs.add(construct);
	}
		
}