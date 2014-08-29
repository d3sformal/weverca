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


package representation.tree;


/**
 * This class represents a tree node that holds construct information.
 * It is a leaf node and its parent is an instance of a TreeFile.
 * 
 * @author Natalia Tyrpakova
 * @see		TreeFile
 */
public class TreeConstruct {
	/**
	 * First line of the construct occurrence
	 */
	public int firstLine;
	/**
	 * Last line of the construct occurrence
	 */
	public int lastLine;
	/**
	 * Offset of the first character of the construct
	 */
	public int firstOffset;
	/**
	 * Offset of the last character of the construct
	 */
	public int lastOffset;
	/**
	 * Construct type
	 */
	public String construct;
	/**
	 * TreeFile instance that holds this TreeConstruct
	 * @see TreeFile
	 */
	public TreeFile parent;
	
	/**
	 * The constructor saves all the construct information.
	 * @param type		construct type
	 * @param fstline	position of the first line of the construct
	 * @param lstline	position of the first line of the construct
	 * @param fstoffset	first character position offset
	 * @param lstoffset	last character position offset	
	 */
	TreeConstruct(String type, int fstline, int lstline, int fstoffset, int lstoffset){
		construct = type;
		firstLine = fstline;
		lastLine = lstline;
		firstOffset = fstoffset;
		lastOffset = lstoffset;
	}

}