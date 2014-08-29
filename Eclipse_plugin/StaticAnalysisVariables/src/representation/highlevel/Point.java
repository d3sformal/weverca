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

/**
 * Point stores a position information.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class Point {
	/**
	 * The first line of a code position represented by this Point
	 */
	public int firstLine;
	/**
	 * The last line of a code position represented by this Point
	 */
	public int lastLine;
	
	/**
	 * The constructor takes a line from analyzer output that contains the position
	 * and parses it into position information.
	 * 
	 * @param line		line from analyzer that contains the position information
	 */
	public Point(String line){
		String delims = " ";
		String[] tokens = line.split(delims);
		if (tokens.length < 14) return;
		firstLine = Integer.parseInt(tokens[4]);
		lastLine = Integer.parseInt(tokens[7]);
	}
	
	/**
	 * This constructor creates an instance from the position represented by int values
	 * 
	 * @param firstLine		first line of the position
	 * @param lastLine		last line of the position
	 */
	public Point(int firstLine, int lastLine){
		this.firstLine = firstLine;
		this.lastLine = lastLine;
	}
	
}