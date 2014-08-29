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


package views.labelproviders;

import org.eclipse.jface.viewers.IFontProvider;
import org.eclipse.jface.viewers.LabelProvider;
import org.eclipse.swt.graphics.Font;
import org.eclipse.swt.graphics.Image;
import org.eclipse.ui.ISharedImages;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.dialogs.FilteredTree;
import org.eclipse.ui.dialogs.PatternFilter;

import representation.highlevel.Context;
import representation.highlevel.ProgramPoint;
import representation.variables.Alias;
import representation.variables.AliasType;
import representation.variables.Taint;
import representation.variables.TaintValue;
import representation.variables.Value;
import representation.variables.Variable;
import representation.variables.VariableType;
import representation.variables.TaintValue.Priority;
import callstacks.Call;


/**
 * LabelProvider and FontProvider for the VariablesView. The tree nodes to set the label and image for 
 * are instances of ProgramPoints, Contexts, Calls, VariableTypes, Variables, Aliases,
 * AliasTypes and Values.
 * 
 * @author Natalia Tyrpakova
 * @see LabelProvider
 * @see	IFontProvider
 */
public class LabelProvider_VariablesView extends LabelProvider implements IFontProvider {
	private FilteredTree filteredTree;
	/**
	 * Filter fol bold elements. It is different from the filter that determines whether to show a node.
	 * This filter is only used to determine which elements match the pattern and therefore should be bold.
	 */
	private PatternFilter filterForBoldElements = new PatternFilter();

	/**
	 * The constructor sets the FilteredTree.
	 * 
	 * @param filterTree	FilteredTree to determine which elements will be shown bold
	 */
	public LabelProvider_VariablesView(FilteredTree filterTree) {
	super();
	this.filteredTree = filterTree;
	}
	
	@Override
	 public String getText(Object element) {
			if (element instanceof String){
				return element.toString();
			}
	    	if (element instanceof Taint){
	    		return "Taint";
	    	}
			if (element instanceof Value){
	    		return ((Value)element).value;
	    	}
	    	if (element instanceof Variable){
	    		if (((Variable)element).parentType.type.equals("Fields")){
	    			String name = ((Variable)element).name;
	    			int index = name.lastIndexOf("->");
	    			if (index != -1){
	    				String endSubstr = name.substring(index+2,name.length());
			    		if (((Variable)element).parentType.parentContext != null) return ("Unknown object ->" + endSubstr);
			    		else return endSubstr;
	    			}
	    		}	
	    		return ((Variable)element).name;
	    	}
	    	if (element instanceof VariableType){
	    		return ((VariableType)element).type;
	    	}
	    	if (element instanceof Alias){
	    		return ((Alias)element).name;
	    	}
	    	if (element instanceof AliasType){
	    		return ((AliasType)element).type;
	    	}
	    	if (element instanceof Call){
	    		Call c = (Call) element;
				String resource = c.filePath.substring(c.filePath.lastIndexOf('/')+1,c.filePath.length());
				StringBuilder atLine = new StringBuilder(Integer.toString(c.firstLine));
				if (c.firstLine != c.lastLine) atLine.append("-"+Integer.toString(c.lastLine));
	    		return ("Called from: " + resource + " at line " + atLine);
	    	}
	    	if (element instanceof Context){
	    		if (((Context)element).deadCode) return ("Unreachable code");
	    		ProgramPoint parent = ((Context)element).parent;
	    		int index = 0;
	    		for (int i = 0; i< parent.contexts.size(); i++){
	    			index = i+1;
	    			if (((Context)element).equals(parent.contexts.get(i))) break;
	    		}
	    		return ("Context" + index);
	    	}
	    	if (element instanceof TaintValue){
	    		String priority = "";
	    		if (((TaintValue)element).priority == Priority.H) priority = " (all possible flows are dirty)";
	    		if (((TaintValue)element).priority == Priority.L) priority = " (clean flow possible)";
	    		return ((TaintValue)element).taint + priority;
	    	}
	    	
	    	return "";
	    }
	 
	/**
	 * {@inheritDoc}
	 * This implementation uses icons from the Common plug-in
	 */
	 @Override
	    public Image getImage(Object element) {
		 
		 if (element instanceof Variable){
			 return general.IconProvider.getVariableIcon().createImage();
		 } 
		 if (element instanceof Taint) return null;
		 if (element instanceof Value){
			 return general.IconProvider.getValueIcon().createImage();
		 }
		 if (element instanceof VariableType){
			 return general.IconProvider.getVariableTypeIcon().createImage();
		 }
		 if (element instanceof Alias){
			 return general.IconProvider.getAliasIcon().createImage();
		 }
		 if (element instanceof AliasType){
    		return general.IconProvider.getVariableTypeIcon().createImage();
    		}
		 if (element instanceof Call){
    		return general.IconProvider.getCallIcon().createImage();
    		}
		 if (element instanceof TaintValue){
			 return PlatformUI.getWorkbench().getSharedImages().getImage(ISharedImages.IMG_OBJS_WARN_TSK);
		 }
		 return null;
	 }

	 /**
	  * {@inheritDoc}
	  * This implementation sets the bold font for all the elements that match the provided Pattern.
	  */
	@Override
	public Font getFont(Object element) {
		return FilteredTree.getBoldFont(element, filteredTree, filterForBoldElements);
	}
}