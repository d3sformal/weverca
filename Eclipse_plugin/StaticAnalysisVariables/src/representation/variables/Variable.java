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
import java.util.HashSet;
import java.util.Set;

import representation.highlevel.Context;

/**
 * This class represents a variable. It has a name, values and may contain other variables - fields.
 * 
 * @author Natalia Tyrpakova, David Hauzar
 *
 */
public class Variable {
	/**
	 * Variable name
	 */
	public String name;
	/**
	 * List of variable values.
	 * @see Value
	 */
	public ArrayList<Value> values;
	/**
	 * Variable's fields. This field is only non-empty in case of an array or an object with fields.
	 */
	public VariableType fields;
	/**
	 * IDs of a values stored in this variable. Serves for identifying object's fields
	 */
	public final Set<Integer> uids = new HashSet<>(1);
	/**
	 * VariableType that holds this Variable.
	 * @see VariableType
	 */
	public VariableType parentType = null;
	
	/**
	 * This constructor saves the variable name.
	 * 
	 * @param name 		variable name
	 */
	public Variable(String name){
		this.name = name;
		values = new ArrayList<Value>();
		fields = new VariableType("Fields",this);
	}
	
	/**
	 * Adds a value to this variable if it does not already exist. 
	 * If UID is present it is saved too.
	 * 
	 * @param value		variable value to add
	 * @param context	context where the variable is added
	 */
	public void addValue(Context context, String value){
		if (value.contains("UID:")){
			int pos = value.lastIndexOf(':');
			int uid = Integer.parseInt(value.substring(pos+2, value.length()));
			uids.add( uid );
			value = value.substring(0, pos-4);
			context.addValueToVariable(uid, this);
		}
		Value newVal = new Value(value,this);
		if (!values.contains(newVal)) {
			values.add(newVal);
		}
	}
	
	/**
	 * Adds a taint information to this variable.
	 * 
	 * @param taint		list of Strings with taint flags
	 */
	public void addTaint(ArrayList<String> taint){
		for (Value value : values){
			if (value instanceof Taint){
				((Taint)value).addTaint(taint);
				return;
			}
		}	
		Taint newTaint = new Taint(this);
		newTaint.addTaint(taint);
		values.add(newTaint);
	}
	
}