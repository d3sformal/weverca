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

import representation.highlevel.Context;

/**
 * This class stores variables of a specific type. It is necessary for the TreeViewer to be stored this way.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class VariableType {
	/**
	 * Variable type (e.g. Local Variables, Aliases...)
	 */
	public String type = "";
	/**
	 * Context instance that holds this VariableType
	 * @see Context
	 */
	public Context parentContext = null;
	/**
	 * Variable instance that holds this Variable in case it is an object or an array.
	 * In this case this VariableType holds its fields.
	 * @see Variable
	 */
	public Variable parentVariable = null;	
	/**
	 * List of variables of this type that belong to the parentContext or parentVariable
	 */
	public ArrayList<Variable> variables = new ArrayList<Variable>();
	/**
	 * List of aliases of this type (should be alias type) that belong to the parentContext or parentVariable
	 */
	public ArrayList<Alias> aliases = new ArrayList<Alias>();
	
	/**
	 * This constructor saves the type and the parent Context
	 * 
	 * @param type
	 * @param parent
	 */
	public VariableType(String type,Context parent){
		this.type = type;
		this.parentContext = parent;
	}
	
	/**
	 * This constructor saves the type and the parent variable.
	 * @param type
	 * @param parent
	 */
	VariableType(String type,Variable parent){
		this.type = type;
		this.parentVariable = parent;
	}
	
	/**
	 * Adds a variable to this type.
	 * 
	 * @param var variable to add
	 */
	public void addVariable(Variable var){
		var.parentType = this;
		variables.add(var);
	}
	
	/**
	 * Adds an alias to this type.
	 * 
	 * @param var variable to add
	 */
	public void addAlias(Alias a){
		a.parentType = this;
		aliases.add(a);
	}
}