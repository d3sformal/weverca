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

/**
 * This class stores a construct information of one construct. It contains
 * a path to the file that contains the construct, a construct type and its position.
 * 
 * @author Natalia Tyrpakova
 */
public class Construct {
	/**
	 * Path to the file where the construct is located
	 */
	public String path;
	/**
	 * Construct type
	 */
	public String construct;
	/**
	 * Position if the construct in the code. 
	 * index 0 - first line
	 * index 1 - offset of the first character
	 * index 2 - last line
	 * index 3 - offset of the last character
	 */
	public int[] position;
	
	/**
	 * The constructor saves all the construct information.
	 * 
	 * @param path			path to the file that contains the construct
	 * @param construct		construct type
	 * @param firstline		position of the first line of the construct
	 * @param firstoffset	first character position offset
	 * @param lastline		position of the first line of the construct
	 * @param lastoffset	last character position offset
	 */
	Construct(String path, String construct, int firstline, int firstoffset, int lastline, int lastoffset ){
		this.path = path;
		this.construct = construct;
		position = new int[] {firstline, firstoffset,lastline,lastoffset};
	}
}