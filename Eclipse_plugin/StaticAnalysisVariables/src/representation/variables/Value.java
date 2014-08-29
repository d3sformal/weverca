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

/**
 * This class stores the value of a variable. It is necessary for the TreeViewer to be stored this way.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class Value {
	/**
	 * Actual value represented by this Value
	 */
	public String value = "";
	/**
	 * Variable this Value is a possible value of
	 * @see Variable
	 */
	public Variable parent;
	
	/**
	 * The constructor saves the value and parent variable.
	 * 
	 * @param value		variable value
	 * @param parent	parent variable
	 */
	Value(String value,Variable parent){
		this.value = value;
		this.parent = parent;
	}
	
	/**
	 * This constructor only initializes the parent variable
	 * 
	 * @param parent	parent variable
	 */
	Value(Variable parent){
		this.parent = parent;
	}
	
	/**
	 * Two values are only equal if their value fields are equal.
	 */
	@Override
	public boolean equals(Object o) 
	{
		if (o instanceof Value) 
	    {
	      Value v = (Value) o;
	      if (v.value.equals(value)) return true;
	    }
	    return false;
	}

}