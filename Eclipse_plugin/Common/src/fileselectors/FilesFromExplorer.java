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


package fileselectors;

import general.Messages;
import general.General;

import java.io.File;
import java.util.ArrayList;
import java.util.Iterator;

import org.eclipse.core.resources.IResource;
import org.eclipse.core.runtime.IAdaptable;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.jface.viewers.IStructuredSelection;
import org.eclipse.ui.IWorkbenchWindow;
import org.eclipse.ui.PlatformUI;

/**
 * Class providing the PHP file paths from the file explorer selection.
 * 
 * @author Natalia Tyrpakova
 */
public class FilesFromExplorer {
	
	/**
	 * Recursively finds path to all the selected PHP files in a selection 
	 * from specified file explorer.
	 * 
	 * @param explorer		an ID of the file explorer to get the selection from
	 * @return				list of paths to the selected PHP files
	 */
	public static ArrayList<String> getFilesToAnalyze(String explorer){
		ArrayList<String> filesToAnalyze = getSelectedPHPFiles(explorer);
		
		if (filesToAnalyze == null) {
			Messages.noFileSelected();
			return null;
		}
		
		if (filesToAnalyze.isEmpty()){
			Messages.noPHPFileSelected();
			return null;
		}
		
		return filesToAnalyze;	
	}
	
	/**
	 * Gets the selected PHP files from an explorer selection. If a folder is selected,
	 * it is searched recursively.
	 *  
	 * @param explorer	ID of the file explorer to get the selection from
	 * @return			list of PHP files to analyze
	 */
	// returns null when no file is selected
	private static ArrayList<String> getSelectedPHPFiles(String explorer){
		ArrayList<String> phpFiles = new ArrayList<String>();
		
		IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
		ISelection selection = window.getSelectionService().getSelection(explorer);
		if (selection == null) return null;
		
		IStructuredSelection structured = (IStructuredSelection)selection;
		
		Iterator it = structured.iterator();
		while (it.hasNext()){
			Object obj = it.next();
			IResource resource;
			
			if (obj instanceof IResource){
				resource = (IResource)obj;
			}
			else if (obj instanceof IAdaptable){
				IAdaptable ad = (IAdaptable)obj;
				resource = (IResource)ad.getAdapter(IResource.class);
			} else {
				continue;
			}
			
			String path = "";
			if (resource.getLocation() != null) resource.getLocation().toString();
			
			File file = new File(path);
			if (file.exists() && file.isFile() && General.isPHPFile(path)){
				phpFiles.add(path);
			}
			if (file.exists() && file.isDirectory()){
				phpFiles.addAll(getPHPFilesFromDirectory(file));
			}
		}	
		return phpFiles;
	}
	
	/**
	 * Recursively finds all the PHP files in a given directory.
	 * 
	 * @param directory		the directory to search in
	 * @return				list of PHP files from the directory
	 * @see					File
	 */
	private static ArrayList<String> getPHPFilesFromDirectory(File directory){
		ArrayList<String> phpFiles = new ArrayList<String>();
		
		for (File file : directory.listFiles()){
			if (file.isDirectory()) phpFiles.addAll(getPHPFilesFromDirectory(file));
			String fileName = file.getAbsolutePath();
			if (file.isFile() && General.isPHPFile(fileName)) phpFiles.add(fileName); 
		}
		
		return phpFiles;	
	}
	

}