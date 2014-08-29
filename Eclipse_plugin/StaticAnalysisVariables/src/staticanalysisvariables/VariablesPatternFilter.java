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


package staticanalysisvariables;

import org.eclipse.jface.viewers.AbstractTreeViewer;
import org.eclipse.jface.viewers.ILabelProvider;
import org.eclipse.jface.viewers.ITreeContentProvider;
import org.eclipse.jface.viewers.StructuredViewer;
import org.eclipse.jface.viewers.Viewer;
import org.eclipse.ui.dialogs.PatternFilter;

/**
 * This class extends PatternFilter, a class used for creating a FilteredTree. 
 * If an element matches the pattern, its children are set to match it too.
 * The result is that the element's children are showed too.
 * 
 * @author Natalia Tyrpakova
 * @see PatternFilter
 */
public class VariablesPatternFilter extends PatternFilter {
	
	/**
	 * Checks if an element is a child match - it has an ancestor that matches the filter text.
	 * 
	 * @param viewer		Viewer that holds the elements
	 * @param element		the tree element to check
	 * @return				true if the given element has an ancestor that matches the filter text
	 */
	private boolean isChildMatch(Viewer viewer, Object element) {
		ITreeContentProvider itcProvider = (ITreeContentProvider)((AbstractTreeViewer)viewer).getContentProvider();
		Object parent = itcProvider.getParent(element);

	  if(parent!=null){
		  if (isLeafMatch(viewer, parent)) return true;
		  return isChildMatch(viewer,parent);
	  }
	  return false;
	 
	 }
	
	/**
	 * Checks if the current (leaf) element is a match with the filter text. In this implementation
	 * elements is a match also if it has an ancestor that is a match with the filter text.
	 */
	@Override
	protected boolean isLeafMatch(Viewer viewer, Object element) {
		ILabelProvider ilProvider = (ILabelProvider)((StructuredViewer)viewer).getLabelProvider();
		String labelText = ilProvider.getText(element);
		
		if (labelText == null) return false;
		if (wordMatches(labelText)) return true;
		return isChildMatch(viewer, element);
	}
}