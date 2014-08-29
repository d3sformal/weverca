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


package representation.variables;

import java.util.ArrayList;

/**
 * Stores a taint information for a variable
 * @author Natalia Tyrpakova
 *
 */
public class Taint extends Value{
	/**
	 * Corresponding TaintValues
	 * @see TaintValue
	 */
	public ArrayList<TaintValue> taints = new ArrayList<TaintValue>();
	
	/**
	 * The constructor initializes the parent variable
	 * 
	 * @param parent	parent variable
	 */
	Taint(Variable parent) {
		super(parent);
	}
	
	/**
	 * Adds a taint information to this instance.
	 * 
	 * @param taint	taint information
	 */
	void addTaint(ArrayList<String> taint){
		for (String t : taint){
			char priority = t.charAt(t.length()-1);
			TaintValue newVal = new TaintValue(t.substring(0,t.length()-1),priority, this);
			if (!taints.contains(newVal)) taints.add(newVal);
		}
	}


	

	
}