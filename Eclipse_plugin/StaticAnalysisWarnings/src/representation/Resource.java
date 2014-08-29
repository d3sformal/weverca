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
 * This class represents a resource. It is necessary for a TreeViewer to be stored this way.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class Resource {
	public String name;
	public FlowPoint parent;
	
	/**
	 * Constructor initializes the resource name
	 * 
	 * @param name	resource name
	 */
	public Resource(String name){
		this.name = name;
	}
}