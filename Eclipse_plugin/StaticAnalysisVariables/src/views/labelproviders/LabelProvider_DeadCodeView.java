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

import representation.highlevel.ProgramPoint;
import callstacks.Call;

/**
 * Label provider for the DeadCodeView.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class LabelProvider_DeadCodeView extends LabelProvider implements ITableLabelProvider{
	@Override
	public Image getColumnImage(Object element, int columnIndex) {
		if (columnIndex != 0) return null;
		if ( element instanceof Call) return general.IconProvider.getCallIcon().createImage();
		if ( element instanceof ProgramPoint) return general.IconProvider.getDeadCodeIcon().createImage();
		return null;
	}

	@Override
	public String getColumnText(Object element, int columnIndex) {
		if (element instanceof ProgramPoint){
			if (columnIndex == 0){
				return ((ProgramPoint)element).resource;
			}
			if (columnIndex == 1){
				if (((ProgramPoint)element).point.firstLine == ((ProgramPoint)element).point.lastLine)
					return ("Line " + ((ProgramPoint)element).point.firstLine);
				else
					return ("Lines " + ((ProgramPoint)element).point.firstLine + " - " + ((ProgramPoint)element).point.lastLine);
			}
		}
		if (element instanceof Call){
			if (columnIndex == 0){
				Call c = (Call) element;
				String resource = c.filePath.substring(c.filePath.lastIndexOf('/')+1,c.filePath.length()); 
				
				StringBuilder atLine = new StringBuilder(Integer.toString(c.firstLine));
				if (c.firstLine != c.lastLine) atLine.append("-"+Integer.toString(c.lastLine));
				
				return ("Called from: " + resource + " " + atLine.toString());
			}
		}
		return "";
	}
	
	
}