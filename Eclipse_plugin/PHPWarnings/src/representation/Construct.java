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
 * This class stores a construct information about one construct - its type and position.
 * 
 * @author Natalia Tyrpakova
 */
public class Construct {
	/**
	 * Construct type (e.g. Magic methods, Class Presence...)
	 */
	public String type;
	/**
	 * First offset of the construct position in the source code 
	 */
	public int firstOffset;
	/**
	 * Last offset of the construct position in the source code 
	 */
	public int lastOffset;
	
	/**
	 * The constructor initializes all the fields.
	 * 
	 * @param type			construct type
	 * @param firstoffset	first character position offset
	 * @param lastoffset	last character position offset
	 */
	Construct(String type, int firstoffset, int lastoffset){
		this.type = type;
		firstOffset = firstoffset;
		lastOffset = lastoffset;
	}
}