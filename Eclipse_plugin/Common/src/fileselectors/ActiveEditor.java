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

import org.eclipse.core.resources.IFile;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IFileEditorInput;
import org.eclipse.ui.IWorkbenchWindow;
import org.eclipse.ui.PlatformUI;

/**
 * The class only provides one static function that gets the current active editor.
 * 
 * @author Natalia Tyrpakova
 */
public class ActiveEditor {
	
	/**
	 * Gets the path of the active editor. 
	 * Returns null if there is no active editor or if saved indicator is set to true 
	 * and the active editor is not saved.
	 * 
	 * @param saved		determines whether the editor must be saved
	 * @return 			path to the active editor
	 */
	public static String getCurrentEditor(boolean saved){
		IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
		if (window != null) {
			IEditorPart activeEditor = window.getActivePage().getActiveEditor();
		    if (activeEditor != null) {
		    	if (saved && activeEditor.isDirty()) {
		    		Messages.dirtyEditor();
		    		return null;
		    	}
		    	IFileEditorInput fileEditorInput = (IFileEditorInput)activeEditor.getEditorInput();
				IFile file = fileEditorInput.getFile();
				String ipath = new String(file.getRawLocation().toString());
				if (General.isPHPFile(ipath)) return ipath;
		    }
		}
		return null;
	}
}