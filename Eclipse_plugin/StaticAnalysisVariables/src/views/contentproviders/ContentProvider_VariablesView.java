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


package views.contentproviders;

import java.util.ArrayList;

import org.eclipse.jface.viewers.ArrayContentProvider;
import org.eclipse.jface.viewers.ITreeContentProvider;

import representation.highlevel.Context;
import representation.highlevel.ProgramPoint;
import representation.variables.*;
import callstacks.Call;


/**
 * ITreeContentProvider for the VariablesView view. The tree nodes are instances of ProgramPoints,
 * Contexts, Calls, VariableTypes, Variables, Aliases, AliasTypes and Values.
 * 
 * @author 	Natalia Tyrpakova
 * @see		ITreeContentProvider
 * @see		ProgramPoint
 * @see		Context
 * @see		Call
 * @see		VariableType
 * @see		Variable
 * @see		Alias
 * @see		AliasType
 * @see		Value
 */
public class ContentProvider_VariablesView extends ArrayContentProvider implements ITreeContentProvider {
	
	/**
	 * Determines whether to show local and global controls and fields without parent objects
	 */
	public boolean showControls;
	
	/**
	 * determines whether to show line input set or output set
	 */
	public boolean before = false; 

	@Override
    public Object[] getElements(Object inputElement) {
    	return getChildren(inputElement);
    }
    
    @Override
    public Object[] getChildren(Object parentElement) {
    	if (parentElement instanceof ProgramPoint){
    		if (((ProgramPoint)parentElement).contexts.size() == 1 &&
    				!((ProgramPoint)parentElement).contexts.get(0).deadCode){
    			return getChildren(((ProgramPoint)parentElement).contexts.get(0));
    		}
    		else return ((ProgramPoint)parentElement).contexts.toArray();
    	}
    	if (parentElement instanceof Context){
    		Context context = (Context) parentElement;
    		VariableType[] types;
    		if (before) types = context.before;
    		else types = context.after;
    		ArrayList<Object> returnarray = new ArrayList<Object>();
    		
    		if (showControls) returnarray.add("Fixpoint iterations: " + context.fixpointIterations);
    		
    		for (int i = 0; i<=Context.NUM_VARIABLE_TYPES-1; i++){
    			if (!showControls && i >=Context.NUM_NON_CONTROL_VARIABLE_TYPES-1 ) break;
    			if (i == Context.ALIASES_INDEX_VARIABLE_TYPES && !types[i].aliases.isEmpty() && !isEmpty(types[i].aliases)) returnarray.add(types[i]);
    			else if (!types[i].variables.isEmpty()) returnarray.add(types[i]);
    		}
    		returnarray.addAll(context.calledFrom);
    		return returnarray.toArray();
    	}
    	if (parentElement instanceof Call){
    		return ((Call)parentElement).childrenCalls.toArray();
    	}
    	if (parentElement instanceof VariableType){
    		if (!((VariableType)parentElement).aliases.isEmpty() && 
    				!isEmpty(((VariableType)parentElement).aliases)){
    			ArrayList<Alias> aliasList= new ArrayList<Alias>();
    			for (Alias a : ((VariableType)parentElement).aliases){
    				if (hasChildren(a)) aliasList.add(a);
    			}
    			return aliasList.toArray();
    		}
    		return ((VariableType)parentElement).variables.toArray();
    	}
    	if (parentElement instanceof Variable){
    		ArrayList<Object> returnlist = new ArrayList<Object>();;
    		if (((Variable)parentElement).fields.variables.size() >0){ // fields are present
    			returnlist.addAll(((Variable)parentElement).fields.variables);
    		}
    		returnlist.addAll(((Variable)parentElement).values);
    		return returnlist.toArray();
    	}
    	if (parentElement instanceof Alias){
    		ArrayList<AliasType> types = new ArrayList<AliasType>();
    		if (((Alias)parentElement).must.variables.size() > 0) types.add(((Alias)parentElement).must);
    		if (((Alias)parentElement).may.variables.size() > 0) types.add(((Alias)parentElement).may);
    		return types.toArray(); 		
    	}
    	if (parentElement instanceof AliasType){
    		return ((AliasType)parentElement).variables.toArray();	
    	}
    	if (parentElement instanceof Taint){
    		return ((Taint)parentElement).taints.toArray();
    	}
    	return new Object[] {};
    }

    @Override
    public Object getParent(Object element) {
    	if (element instanceof Variable){
    		return ((Variable)element).parentType;
    	}
    	if (element instanceof VariableType){
    		if (((VariableType)element).parentContext != null){
    			Context parent = (Context)((VariableType)element).parentContext;
    			if (parent.parent.contexts.size() == 1) return parent.parent;
    			else return ((VariableType)element).parentContext;
    		}
    		return ((VariableType)element).parentVariable;
    	}
    	if (element instanceof Value){
    		return ((Value)element).parent;
    	}
    	if (element instanceof Alias){
    		return ((Alias)element).parentType;
    	}
    	if (element instanceof AliasType){
    		return ((AliasType)element).parentAlias;
    	}
    	if (element instanceof Call){
    		if (((Call)element).parentCall != null) return ((Call)element).parentCall;
    		if (((Call)element).parent != null) {
    			Context parent = (Context)((Call)element).parent;
    			if (parent.parent.contexts.size() == 1) return parent.parent;
    			return ((Call)element).parent;
    		}
    	}
    	if (element instanceof Context){
    		return ((Context)element).parent;
    	}
    	if (element instanceof TaintValue){
    		return ((TaintValue)element).parent;
    	}
    	return null;
    }

    @Override
    public boolean hasChildren(Object element) {
    	if (element instanceof ProgramPoint){
    		return (!((ProgramPoint)element).contexts.isEmpty());
    	}
    	if (element instanceof Context){
    		if (before){
    			for (int i = 0; i<=Context.NUM_VARIABLE_TYPES-1; i++){
    				if (!showControls && i >= Context.NUM_NON_CONTROL_VARIABLE_TYPES-1) break;
    				if (hasChildren(((Context)element).before[i])) return true;
    			}
    		}
    		if (!before){
    			for (int i = 0; i<=Context.NUM_VARIABLE_TYPES-1; i++){
    				if (!showControls && i >= Context.NUM_NON_CONTROL_VARIABLE_TYPES-1) break;
    				if (hasChildren(((Context)element).after[i])) return true;
    			}
    		}
    		if (!((Context)element).calledFrom.isEmpty()) return true;
    		return false;
    	}
    	if (element instanceof VariableType){
    		return (!((VariableType)element).variables.isEmpty() ||
    				(!((VariableType)element).aliases.isEmpty()) && !isEmpty(((VariableType)element).aliases));
    	}
    	if (element instanceof Variable){
    		return (!((Variable)element).fields.variables.isEmpty() ||
    				!((Variable)element).values.isEmpty());
    	}
    	if (element instanceof Alias){
    		return (!((Alias)element).may.variables.isEmpty() ||
    				!((Alias)element).must.variables.isEmpty());
    	}
    	if (element instanceof AliasType){
    		return (!((AliasType)element).variables.isEmpty() ||
    				!((AliasType)element).variables.isEmpty());
    	}
    	if (element instanceof Call){
    		return (!((Call)element).childrenCalls.isEmpty());
    	}
    	if (element instanceof Taint){
    		return (!((Taint)element).taints.isEmpty());
    	}
      return false;
    }
    
    /**
     * Checks whether a list of Aliases is empty - that means all the aliases in the list have no children.
     * 
     * @param list		a list to check
     * @return			true if the list only contains Aliases with no children
     */
    private boolean isEmpty(ArrayList<Alias> list){
    	for (Alias alias : list){
    		if (hasChildren(alias)) return false;
    	}
    	return true;
    }

}