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

import representation.points.FlowPoint;

/**
 * Serves as a storage for a taint flow accessible from both start and end
 * 
 * @author Natalia Tyrpakova
 *
 */
public class FlowPart{
	/**
	 * First FlowPoint of the flow
	 * @see FlowPoint
	 */
	public FlowPoint start;
	/**
	 * Last FlowPoint of the flow
	 * @see FlowPoint
	 */
	public FlowPoint end;
	
	/**
	 * The constructor initializes the fields
	 * 
	 * @param start		first FlowPoint of the represented flow
	 * @param end		last FlowPoint of the represented flow
	 */
	FlowPart(FlowPoint start, FlowPoint end){
		this.start = start;
		this.end = end;
	}
}