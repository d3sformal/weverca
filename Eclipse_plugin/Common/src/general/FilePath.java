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


package general;

import org.eclipse.core.resources.IProject;
import org.eclipse.core.resources.IWorkspace;
import org.eclipse.core.resources.ResourcesPlugin;
import org.eclipse.core.runtime.IPath;
import org.eclipse.core.runtime.Platform;

/**
 * The class serves for file path processing
 * 
 * @author Natalia Tyrpakova
 */
public class FilePath {
	
	/**
	 * Takes a full path of a file and returns the path relative to the current workspace.
	 * 
	 * @param filePath	full file path
	 * @return			path relative to the current workspace
	 */
	public static String truncate(String filePath){
		if (filePath == null) return "";
		filePath = filePath.replace("\\","/");
		String workspace = Platform.getLocation().toString();
		String result = filePath;
		if (filePath.contains(workspace)) result = filePath.replace(workspace, "");
		else {		
			IWorkspace ws = ResourcesPlugin.getWorkspace();
			IProject[] projects = ws.getRoot().getProjects();
			for (IProject p : projects){
				if (p == null) continue;
				IPath path = p.getRawLocation();
				if (path != null && filePath.contains(path.toString())) {
					String projectLocation = p.getRawLocation().toString();
					String substr = projectLocation.substring(0,projectLocation.length()-2);
					projectLocation = projectLocation.substring(0, substr.lastIndexOf('/') + 1);
					result = filePath.replace(projectLocation, "");
					break;
				}
			}
		}
		return result;
	}
	
}