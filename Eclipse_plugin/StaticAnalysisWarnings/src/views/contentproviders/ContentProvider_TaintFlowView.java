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

import representation.Resource;
import representation.points.FirstFlowPoint;
import representation.points.FlowPoint;
import representation.points.SplitPoint;

/**
 * ITreeContentProvider for the TaintFlowView
 * 
 * @author 	Natalia Tyrpakova
 * @see		ITreeContentProvider
 */
public class ContentProvider_TaintFlowView extends ArrayContentProvider implements ITreeContentProvider{

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
		if (parentElement instanceof FirstFlowPoint){
			FirstFlowPoint ffp = (FirstFlowPoint)parentElement;
			return ffp.flows.toArray();
			
		}
		if (parentElement instanceof SplitPoint){
			SplitPoint sp = (SplitPoint)parentElement;
			return sp.flows.toArray();		
		}
		
		return null;
	}

	@Override
	public Object getParent(Object element) {
		if (element instanceof FlowPoint) return ((FlowPoint)element).parent;
		if (element instanceof Resource) return ((Resource)element).parent;
		return null;
	}

	@Override
	public boolean hasChildren(Object element) {
		if (element instanceof FirstFlowPoint) return !((FirstFlowPoint)element).flows.isEmpty();
		if (element instanceof SplitPoint) return !((SplitPoint)element).flows.isEmpty();
		return false;
	}

}