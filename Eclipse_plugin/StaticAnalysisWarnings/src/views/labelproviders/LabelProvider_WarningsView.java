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

import org.eclipse.jface.viewers.ITableLabelProvider;
import org.eclipse.jface.viewers.LabelProvider;
import org.eclipse.swt.graphics.Image;
import org.eclipse.ui.ISharedImages;
import org.eclipse.ui.PlatformUI;

import representation.FlowString;
import representation.Warning;
import representation.points.FlowPoint;
import callstacks.Call;

/**
 * Table label provider for the WarningsView
 * 
 * @author Natalia Tyrpakova
 */
public class LabelProvider_WarningsView extends LabelProvider implements ITableLabelProvider{
	/**
	 * {@inheritDoc}
	 * In this implementation this only affects the first column.
	 * 
	 *  @param element		an instance of Warning, Call or FlowPoint representing a table row
	 *  @param columnIndex	the zero-based index of the column in which the label appears
	 *  @return 			Image or null if the columnIndex is not zero
	 *  @see 				Image
	 *  @see				ISharedImages	
	 */
	@Override
	public Image getColumnImage(Object element, int columnIndex) {
		if (columnIndex == 0 && element instanceof Warning) return PlatformUI.getWorkbench().getSharedImages().getImage(ISharedImages.IMG_OBJS_WARN_TSK);
		if (columnIndex == 0 && element instanceof Call) return general.IconProvider.getCallIcon().createImage();
		if (columnIndex == 0 && element instanceof FlowString) return general.IconProvider.getFlowIcon().createImage();
		return null;
	}

	/**
	 * {@inheritDoc}
	 * Depending on the column and the element, this returns a warning/call/ description, taint flow, resource name or a line number
	 * 
	 * @param element		an instance of Warning, Call or a FlowPoint representing a table row
	 * @param columnIndex	the zero-based index of the column in which the label appears
	 * @return 				String or null if element is null or not instance of Warning/Call/FlowPoint
	 * @see 				Warning
	 * @see					Call
	 * @see					FlowPoint
	 */
	@Override
	public String getColumnText(Object element, int columnIndex) {
		if (element instanceof String){
			if (columnIndex == 0) return (String)element;
		}
		if (element instanceof Call){
			Call c = (Call) element;
			String resource = c.truncatedFilePath;
			
			StringBuilder atLine = new StringBuilder(Integer.toString(c.firstLine));
			if (c.firstLine != c.lastLine) atLine.append("-"+Integer.toString(c.lastLine));
			
			if (columnIndex == 0) return "Called from:";
			if (columnIndex == 1) return resource;
			if (columnIndex == 2) return atLine.toString();
		}
		
		if (element instanceof FlowString){
			FlowString flow = (FlowString)element;
			if (columnIndex == 0) return "Possible taint flow:";
			if (columnIndex == 1) return flow.flow;
			if (columnIndex == 2) return "";
		}
		
		if (!(element instanceof Warning)) return null;
		Warning w = (Warning) element;
		String resource = w.resource;
		
		StringBuilder desc = new StringBuilder("");
		if (w.security) desc.append("Security warning: ");
		else desc.append("Warning: ");
		desc.append(w.description);	
		
		String priority = "low";
		if (w.highPriority) priority = "high";
		
		if (columnIndex == 0) return desc.toString();
		if (columnIndex == 1) return resource;
		if (columnIndex == 2) return Integer.toString(w.atLine);
		if (columnIndex == 3) return priority;

		return null;
	}

}