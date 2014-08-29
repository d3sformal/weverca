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

import org.eclipse.jface.viewers.ColumnLabelProvider;

import representation.Resource;
import representation.points.FlowPoint;

/**
 * Column label provider for the first column of FlowDialog
 * 
 * @author Natalia Tyrpakova
 */
public class LabelProviderCol1_TaintFlowView extends ColumnLabelProvider {
	
	@Override
	public String getText(Object element){
		if (element instanceof Resource){
			return ((Resource)element).name;
		}
		if (element instanceof FlowPoint){
			FlowPoint p = ((FlowPoint)element);
			if (p.point != null) return p.point.toString();
			else return "Multiple possible flows";
		}
		return "";
	}

}