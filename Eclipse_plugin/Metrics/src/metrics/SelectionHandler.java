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


package metrics;

import java.util.ArrayList;
import java.util.Iterator;

import org.eclipse.core.resources.IResource;
import org.eclipse.core.runtime.IAdaptable;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.jface.viewers.IStructuredSelection;

/**
 * Handles the selection from file explorer
 * 
 * @author Natalia Tyrpakova
 *
 */
public class SelectionHandler {
	
	/**
	 * Gets a selection from file explorer and returns a list of the corresponding file paths.
	 * 
	 * @param 		selection
	 * @return		list of selected file paths
	 */
	public static ArrayList<String> getPathsFromSelection(ISelection selection){
		ArrayList<String> paths = new ArrayList<String>();
		IStructuredSelection structured = (IStructuredSelection) selection;
		Iterator it = structured.iterator();
		while (it.hasNext()){
			Object obj = it.next();
			IResource resource = null;
			if (obj instanceof IResource){
				resource = (IResource)obj;
			}
			else if (obj instanceof IAdaptable){
				IAdaptable ad = (IAdaptable)obj;
				resource = (IResource)ad.getAdapter(IResource.class);
			} 
			else continue;
			if (resource.getLocation() != null) paths.add(resource.getLocation().toString());
		}	
		return paths;	
	}	
}