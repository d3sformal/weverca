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


package representation.points;

import general.FilePath;

/**
 * Stores a position information
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
	 * The first column of a code position represented by this Point
	 */
	public int firstCol;
	/**
	 * The last column of a code position represented by this Point
	 */
	public int lastCol;
	/**
	 * The file path of a code position represented by this Point
	 */
	public String filePath = "";
	
	private String truncatedFilePath = "";
	
	/**
	 * Parses the point information from Weverca analyzer
	 * 
	 * @param point point information from Weverca analyzer
	 */
	public Point(String point){
		String delims = "[(),-]+";
		String[] tokens = point.split(delims);
		if (tokens.length < 5) return;
		firstLine = Integer.parseInt(tokens[1]);
		lastLine = Integer.parseInt(tokens[3]);
		firstCol = Integer.parseInt(tokens[2]);
		lastCol = Integer.parseInt(tokens[4]);
	}
	
	/**
	 * A copy constructor
	 * 
	 * @param point Point to be copied
	 */
	public Point(Point point){
		firstLine = point.firstLine;
		lastLine = point.lastLine;
		firstCol = point.firstCol;
		lastCol = point.lastCol;
		filePath = point.filePath;
		truncatedFilePath = point.truncatedFilePath;
	}
	
	/**
	 * Gets the resource name
	 * 
	 * @return resource name
	 */
	public String getResourceName(){
		return truncatedFilePath;
	}
	
	/**
	 * Sets the file path and also extracts the short resource name
	 * 
	 * @param filepath	file path to be set
	 */
	public void setFilePath(String filepath){
		this.filePath = filepath;
		this.truncatedFilePath = FilePath.truncate(filePath);
	}
	
	/**
	 * Two points are equal if they represent the same position.
	 */
	@Override
	public boolean equals(Object obj){
		if (obj == null) return false;
		if (obj == this) return true;
		if (!(obj instanceof Point)) return false;
		
		Point other = (Point)obj;
		if (other.firstLine == this.firstLine &&
			other.lastLine == this.lastLine &&
			other.firstCol == this.firstCol &&
			other.lastCol == this.lastCol &&
			other.filePath.equals(this.filePath)) return true;
		
		return false;
	}
	
	@Override
	public String toString(){
		if (firstLine == lastLine){
			return (" -> Line " + firstLine);
		}
		else return (" -> Lines " + firstLine + "-" + lastLine);
	}
}