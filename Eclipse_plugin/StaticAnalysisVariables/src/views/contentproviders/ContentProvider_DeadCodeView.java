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
import callstacks.Call;

/**
 * Content provider for the DeadCodeView
 * 
 * @author Natalia Tyrpakova
 */
public class ContentProvider_DeadCodeView extends ArrayContentProvider implements ITreeContentProvider {

	/**
     * Sets the input elements as the roots of the TreeViewer.
     * 
     * @param inputElement		the root elements
     * @return					array of input elements
     */
    @Override
    public Object[] getElements(Object inputElement) {
    	if (inputElement instanceof ArrayList) return ((ArrayList<Object>) inputElement).toArray();
    	else return new Object[] {inputElement};
    }
	
	@Override
	public Object[] getChildren(Object parentElement) {
		if (parentElement instanceof ProgramPoint){
			ProgramPoint pPoint = (ProgramPoint)parentElement;
			ArrayList<Call> returnList = new ArrayList<Call>();
			for (Context context : pPoint.contexts){
				if (context.deadCode) returnList.addAll(context.calledFrom);
			}
			return returnList.toArray();
		}
		if (parentElement instanceof Call) return ((Call)parentElement).childrenCalls.toArray();
		return null;
	}

	@Override
	public Object getParent(Object element) {
		if (element instanceof Call && ((Call)element).parentCall != null) return ((Call)element).parentCall;
		if (element instanceof Call && ((Call)element).parent != null) return ((Call)element).parent;
		return null;
	}

	@Override
	public boolean hasChildren(Object element) {
		if (element instanceof ProgramPoint) return (hasCalls((ProgramPoint)element));
		if (element instanceof Call) return !((Call)element).childrenCalls.isEmpty();
		return false;
	}
	
	/**
	 * Checks whether there exist a context of the given ProgramPoint that has a call.
	 * 
	 * @param p		ProgramPoint to checks for contexts' calls
	 * @return		true if there is any context with a call
	 */
	private boolean hasCalls(ProgramPoint p){
		for (Context c : p.contexts){
			if (c.deadCode && !c.calledFrom.isEmpty()) return true;
		}
		return false;
	}

}