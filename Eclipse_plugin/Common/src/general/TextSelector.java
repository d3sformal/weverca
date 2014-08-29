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

import java.io.File;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.resources.IWorkspace;
import org.eclipse.core.resources.ResourcesPlugin;
import org.eclipse.core.runtime.IPath;
import org.eclipse.core.runtime.Path;
import org.eclipse.jface.text.BadLocationException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.text.IRegion;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IWorkbenchPage;
import org.eclipse.ui.PartInitException;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.ide.IDE;
import org.eclipse.ui.texteditor.ITextEditor;

/**
 * This class selects and reveals specified text range in given editor. The range might be defined
 * in various ways.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class TextSelector {

	/**
	 * Selects and reveals the text specified by first and last offset in the file determined by a file path.
	 * 
	 * @param path			file to select and reveal text in
	 * @param firstOffset	first offset of the text to select and reveal
	 * @param lastOffset	last offset of the text to select and reveal
	 */
	public static void showText(String path, int firstOffset, int lastOffset){
		ITextEditor editor = getEditor(path);
		editor.selectAndReveal(firstOffset,lastOffset-firstOffset+1);
	}
	
	/**
	 * Selects and reveals the text specified by first and last line
	 * and first and last column in the file determined by a file path.
	 * 
	 * @param path			file to select and reveal text in
	 * @param firstLine		first line of the text to select and reveal
	 * @param lastLine		last line of the text to select and reveal
	 * @param firstColumn	first column of the text to select and reveal
	 * @param lastColumn	last column of the text to select and reveal
	 */
	public static void showText(String path, int firstLine, int lastLine, int firstColumn, int lastColumn){
		ITextEditor editor = getEditor(path);
		if (editor == null) return;
		    	 IDocument document = editor.getDocumentProvider().getDocument(editor.getEditorInput());
    			 if (document != null) {
	    			 IRegion firstlineInfo;
					try {
						firstlineInfo = document.getLineInformation(firstLine -1);
						IRegion lastlineInfo = document.getLineInformation(lastLine-1);
		    			int firstOffset = firstlineInfo.getOffset() + firstColumn -1;
		    			int lastOffset = lastlineInfo.getOffset() + lastColumn;
		    			editor.selectAndReveal(firstOffset,lastOffset-firstOffset);
					} catch (BadLocationException e) {
						return;
					}
		   }
	}
	
	/**
	 * Selects and reveals the text specified by first and last line
	 * in the file determined by a file path.
	 * 
	 * @param path			file to select and reveal text in
	 * @param firstLine		first line of the text to select and reveal
	 * @param lastLine		last line of the text to select and reveal
	 */
	public static void showTextLines(String path, int firstLine, int lastLine){
		ITextEditor editor = getEditor(path);
		if (editor == null) return;
	
		IDocument document = editor.getDocumentProvider().getDocument(editor.getEditorInput());
		if (document != null) {
			IRegion firstlineInfo;
			try {
				firstlineInfo = document.getLineInformation(firstLine -1);
				IRegion lastlineInfo = document.getLineInformation(lastLine);
				int firstOffset = firstlineInfo.getOffset();
				int lastOffset = lastlineInfo.getOffset();
				editor.selectAndReveal(firstOffset,lastOffset-firstOffset-1);
			} catch (BadLocationException e) {
				return;
			}
		}
	}
	
	/**
	 * Returns an ITextEditor specified by a path
	 * 
	 * @param path	path of the editor to get
	 * @return		editor
	 * @see			ITextEditor
	 */
	private static ITextEditor getEditor(String path){
		File file = new File(path);
		if (file.exists() && file.isFile()){
			IWorkbenchPage page = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage();	
		    IWorkspace workspace= ResourcesPlugin.getWorkspace();    
		    IPath location= Path.fromOSString(file.getAbsolutePath()); 
		    IFile ifile= workspace.getRoot().getFileForLocation(location);
		    	try {
		    		IEditorPart editorPart = IDE.openEditor(page, ifile);
					if (editorPart instanceof ITextEditor){
						ITextEditor editor = (ITextEditor)editorPart;
						return editor;
					}		
				} catch (PartInitException e) {
					return null;
				}
		}
		return null;
	}
}