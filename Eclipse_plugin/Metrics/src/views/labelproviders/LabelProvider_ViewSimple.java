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

import java.io.File;

import org.eclipse.jface.viewers.ITableLabelProvider;
import org.eclipse.jface.viewers.LabelProvider;
import org.eclipse.swt.graphics.Image;
import org.eclipse.ui.ISharedImages;
import org.eclipse.ui.PlatformUI;

import representation.MetricInformation;
import general.General;

/**
 * Table label provider for View Simple
 * 
 * @author Natalia Tyrpakova
 */
public class LabelProvider_ViewSimple extends LabelProvider implements ITableLabelProvider {
	/**
	 * {@inheritDoc}
	 * In this implementation it only affects the first column.
	 * 
	 *  @param element		an instance of MetricInformation representing a table row
	 *  @param columnIndex	the zero-based index of the column in which the label appears
	 *  @return 			Image or null if the columnIndex is not zero
	 *  @see 				Image
	 *  @see				ISharedImages	
	 */
	public Image getColumnImage(Object element, int columnIndex) {
		if (columnIndex == 0 && element instanceof MetricInformation ){
			File file = new File(((MetricInformation)element).fileName); 
			if (file.exists() && file.isFile()) return PlatformUI.getWorkbench().getSharedImages().getImage(ISharedImages.IMG_OBJ_FILE);
			if (file.exists() && file.isDirectory()) return PlatformUI.getWorkbench().getSharedImages().getImage(ISharedImages.IMG_OBJ_FOLDER);
		}	
		return null;
	}
	
	/**
	 * {@inheritDoc}
	 * Depending on the column, this can be a file name or property value.
	 * 
	 * @param element		an instance of MetricInformation representing a table row
	 * @param columnIndex	the zero-based index of the column in which the label appears
	 * @return 				String or null if element is null or not instance of MetricInformation
	 * @see 				MetricInformation
	 */
	public String getColumnText(Object element, int columnIndex) {
		MetricInformation i = (MetricInformation) element;
		if (columnIndex == 0){
			return i.truncatedFileName;
		}
		else return General.floatToString(i.metricValues.get(columnIndex-1));
	}
}