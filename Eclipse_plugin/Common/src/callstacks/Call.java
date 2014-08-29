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

import general.FilePath;

import java.util.ArrayList;


/**
 * Stores a call stack for a specific PHP code context
 * 
 * @author Natalia Tyrpakova
 *
 */
public class Call {
	/**
	 * Parent Call of a Call
	 */
	public Call parentCall = null;
	/**
	 * Parent object of a Call
	 */
	public Object parent = null;
	/**
	 * List of children Calls of a Call, contains the call stacks that led to this call
	 */
	public ArrayList<Call> childrenCalls = new ArrayList<Call>();
	public final String filePath;
	public String truncatedFilePath = "";
	public int firstLine;
	public int lastLine;
	public int firstCol;
	public int lastCol;
	
	/**
	 * The constructor initializes the call position;
	 * 
	 * @param filePath		call filePath
	 * @param firstLine		call first line
	 * @param lastLine		call last line
	 * @param firstCol		call first column
	 * @param lastCol		call last column
	 */
	Call(String filePath, int firstLine, int lastLine, int firstCol, int lastCol){
		this.filePath = filePath;
		truncatedFilePath = FilePath.truncate(filePath);
		this.firstLine = firstLine;
		this.lastLine = lastLine;
		this.firstCol = firstCol;
		this.lastCol = lastCol;
	}
	
	/**
	 * Compares another Object to this instance of Call. They are equal, if the compared Object is Call
	 * and they have the same position and children calls
	 * 
	 * @param o		Object to compare
	 * @return		true if the compared object is equal to this Call
	 */
	@Override
	public boolean equals(Object o){
		if (o == null) return false;
		if (o == this) return true;
		if (!(o instanceof Call)) return false;
		Call c = (Call)o;
		if (c.childrenCalls.size() != this.childrenCalls.size()) return false;
		for (Call child : c.childrenCalls){
			if (!this.childrenCalls.contains(child)) return false;
		}
		if (c.filePath.equals(this.filePath) &&
			c.firstLine == this.firstLine &&
			c.lastLine == this.lastLine &&
			c.firstCol == this.firstCol &&
			c.lastCol == this.lastCol) return true;
		return false;	
	}
}