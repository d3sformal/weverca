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

import representation.AggregatedMetricInformation;
import general.General;

/**
 * Table label provider for View Aggregated 
 * 
 * @author Natalia Tyrpakova
 *
 */

public class LabelProvider_ViewAggregated extends LabelProvider implements ITableLabelProvider {
	/**
	 * {@inheritDoc}
	 * In this implementation it only affects the first column.
	 * 
	 *  @param element		an instance of AggregatedMetricInformation representing a table row
	 *  @param columnIndex	the zero-based index of the column in which the label appears
	 *  @return 			Image or null if the columnIndex is not zero
	 *  @see 				Image
	 *  @see				ISharedImages	
	 */
	public Image getColumnImage(Object element, int columnIndex) {
		if (columnIndex == 0)
		return PlatformUI.getWorkbench().getSharedImages().getImage(ISharedImages.IMG_OBJ_FOLDER);
		else return null;
	}
	/**
	 * {@inheritDoc}
	 * Depending on the column, this can be a file name, property name or value.
	 * 
	 * @param element		an instance of AggregatedMetricInformation representing a table row
	 * @param columnIndex	the zero-based index of the column in which the label appears
	 * @return 				String or null if element is null or not instance of AggregatedMetricInformation
	 * @see 				AggregatedMetricInformation
	 */
	public String getColumnText(Object element, int columnIndex) {
		if (element instanceof AggregatedMetricInformation && element != null) {
			AggregatedMetricInformation i = (AggregatedMetricInformation) element;
			String result = "";
			switch(columnIndex){
			case 0:
				result = i.truncatedFileName;
				break;
			case 1:
				result = i.property;
				break;
			case 2:
				result = General.floatToString(i.value);
				break;
			default:
				result = "";
			}
			return result;
		}
		return null;	
	}
	
}