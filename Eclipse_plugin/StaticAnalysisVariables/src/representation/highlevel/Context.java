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


package representation.highlevel;

import java.util.AbstractMap;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Enumeration;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Set;

import representation.variables.Variable;
import representation.variables.VariableType;
import callstacks.Call;

/**
 * Context holds the information about variable values that come from the same context 
 * of a Program Point. 
 * 
 * @author Natalia Tyrpakova
 */
public class Context {
	/**
	 * Number of types of variables stored in the context.
	 */
	public static final int NUM_VARIABLE_TYPES = 7;
	/**
	 * Number of non-control types of variables stored in the context.
	 */
	public static final int NUM_NON_CONTROL_VARIABLE_TYPES = 5;
	public static final int ALIASES_INDEX_VARIABLE_TYPES = 2;
	
	/**
	 * Number of fixpoint iterations.
	 */
	public int fixpointIterations = 0;
	
	/**
	 * Maps UID to variables that contain values with given UID.
	 */
	private final AbstractMap<Integer, Set<Variable>> uidToVariables = new HashMap<Integer, Set<Variable>>();
	
	/**
	 * Variable information before the code represented by the ProgramPoint holding this Context is processed
	 */
	public VariableType[] before = new VariableType[NUM_VARIABLE_TYPES];
	/**
	 * Variable information after the code represented by the ProgramPoint holding this Context is processed
	 */
	public VariableType[] after = new VariableType[NUM_VARIABLE_TYPES];
	/**
	 * Call stacks that lead to this context
	 */
	public ArrayList<Call> calledFrom;
	/**
	 * ProgramPoint this context belongs to
	 */
	public ProgramPoint parent;
	/**
	 * Indicator of an unreachable code
	 */
	public boolean deadCode = false;
	

	/**
	 * The constructor initializes the fields.
	 * 
	 * @param calledFrom	list of calls corresponding to this particular context
	 * @param parent		parent Program Point
	 */
	public Context(ArrayList<Call> calledFrom, ProgramPoint parent){
		this.calledFrom = calledFrom;
		this.parent = parent;
		if (calledFrom == null) this.calledFrom = new ArrayList<Call>();
		
		before[0] = new VariableType("Global variables",this); 
		before[1] = new VariableType("Local variables",this);   
		before[ALIASES_INDEX_VARIABLE_TYPES] = new VariableType("Aliases",this);    
		before[3] = new VariableType("Arrays",this);     
		before[4] = new VariableType("Fields",this);                  
		before[5] = new VariableType("Global controls",this);                    
		before[6] = new VariableType("Local controls",this);
		
		after[0] = new VariableType("Global variables",this); 
		after[1] = new VariableType("Local variables",this);   
		after[ALIASES_INDEX_VARIABLE_TYPES] = new VariableType("Aliases",this);    
		after[3] = new VariableType("Arrays",this);     
		after[4] = new VariableType("Fields",this);                  
		after[5] = new VariableType("Global controls",this);                    
		after[6] = new VariableType("Local controls",this);
	}
	
	/**
	 * A context is only determined by its call stack.
	 */
	@Override
	public boolean equals(Object o){
		if (o == null) return false;
		if (o == this) return true;
		if (!(o instanceof Context)) return false;
		Context c = (Context)o;
		if (c.calledFrom.size() != this.calledFrom.size()) return false;
		for (Call call : c.calledFrom){
			if (!this.calledFrom.contains(call)) return false;
		}
		return true;	
	}
	
	/**
	 * Adds value to given variable. Establishes mapping valueUID -> set of variables containing the value.
	 * @param valueUID UID of the value to be added.
	 * @param variable variable to that the value is added.
	 */
	public void addValueToVariable(int valueUID, Variable variable) {
		Set<Variable> variables = uidToVariables.get(valueUID);
		if (variables == null) variables = new HashSet<Variable>(1);
		variables.add(variable);
		uidToVariables.put(valueUID, variables);
	}
	
	/**
	 * Gets variables that contain values with given UID.
	 * @param valueUID the UID of values.
	 * @return variables that contain values with given UID.
	 */
	public Collection<Variable> getVariablesContainingUID(int valueUID) {
		Set<Variable> variables = uidToVariables.get(valueUID);
		if (variables == null) return new LinkedList<Variable>();
		return variables;
	}
	
}