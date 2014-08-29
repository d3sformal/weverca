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

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;

import org.eclipse.jface.resource.JFaceResources;
import org.eclipse.jface.viewers.ColumnLabelProvider;
import org.eclipse.jface.viewers.DelegatingStyledCellLabelProvider.IStyledLabelProvider;
import org.eclipse.jface.viewers.StyledString;
import org.eclipse.swt.graphics.RGB;

import representation.points.FlowPoint;
import representation.points.Point;

/**
 * Styled column label provider for the second column of FlowDialog
 * 
 * @author Natalia Tyrpakova
 */
public class LabelProviderCol2_TaintFlowView extends ColumnLabelProvider
		implements IStyledLabelProvider {

	@Override
	public StyledString getStyledText(Object element) {		
		if (element instanceof FlowPoint){
			FlowPoint p = ((FlowPoint)element);
			if (p.point == null) return new StyledString("");
			try {
				String code = getLines(p.point);
				StyledString result = new StyledString(code);
				JFaceResources.getColorRegistry().put("TAINT_COLOUR",new RGB(204,0,0));
				JFaceResources.getColorRegistry().put("TAINT_BGCOLOUR",new RGB(255,255,100));
				
				for (String var : p.variables)
				{
					String variable = "$" + var;
					int lastIndex = 0;
					
					while (lastIndex > -1){
						int newIndex = code.indexOf(variable, lastIndex);
						lastIndex = newIndex;
						if (newIndex != -1){
							lastIndex++;
							char nextChar = code.charAt(newIndex + variable.length());
							if (Character.isLetterOrDigit(nextChar) || nextChar == '_') continue; //part of variable name
							if (newIndex < code.indexOf('=')) continue;
							result.setStyle(newIndex, variable.length(),StyledString.createColorRegistryStyler("TAINT_COLOUR", null));
						}
					}
				}
				return result;
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		return new StyledString("");
	}
	
	@Override
	public String getText(Object element){
		return "";
	}
	
	/**
	 * Gets the text from file according to the point position information
	 * 
	 * @param p		Point containing position of the text
	 * @return		text
	 * @throws IOException
	 */
	private String getLines(Point p) throws IOException{
		
		BufferedReader r = new BufferedReader(new FileReader(p.filePath));
		StringBuilder result = new StringBuilder();
		for (int i = 1; i <= p.lastLine; i++)
		{
		   String line = r.readLine();
		   if (i>=p.firstLine) {
			   String trimmedLine = line.replace(String.valueOf((char) 160), " ").trim();
			   result.append(trimmedLine);
			   result.append(" ");
		   }
		}
		r.close();
		return result.toString();		
	}

}