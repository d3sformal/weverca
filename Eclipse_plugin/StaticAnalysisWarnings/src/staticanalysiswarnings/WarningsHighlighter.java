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


package staticanalysiswarnings;

import java.util.ArrayList;
import java.util.Iterator;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.resources.IMarker;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.text.Position;
import org.eclipse.jface.text.source.Annotation;
import org.eclipse.jface.text.source.IAnnotationModel;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IEditorReference;
import org.eclipse.ui.IFileEditorInput;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.texteditor.IDocumentProvider;
import org.eclipse.ui.texteditor.ITextEditor;
import org.eclipse.ui.texteditor.SimpleMarkerAnnotation;

import representation.Warning;

/**
 * This class is supposed to highlight the analysis warnings and security warnings in editors.
 * Highlighting is done using the IMarker and SimpleMarkerAnnotations that are stored in an IAnnotationModel
 * 
 * @author 	Natalia Tyrpakova
 * @see		SimpleMarkerAnnotation
 * @see		IMarker
 * @see		IAnnotationModel
 * @see		Warning
 */
public class WarningsHighlighter {
	
	private ArrayList<Warning> warnings = new ArrayList<Warning>();
	private ArrayList<IAnnotationModel> iams = new ArrayList<IAnnotationModel>();;
	private boolean disabled = false;
	
	/**
	 * Sets the list of warnings to be highlighted
	 * 
	 * @param warnings	list of warnings
	 */
	public void setWarnings(ArrayList<Warning> warnings){
		this.warnings = warnings;
	}
	
	/**
	 * Removes the highlight and disables this highlighter
	 * 
	 * @throws CoreException
	 */
	public void disable() throws CoreException{
		disabled = true;
		remove();
	}
	
	/**
	 * Enables this Highlighter and highlights the open editors
	 * 
	 * @throws CoreException
	 */
	public void enable() throws CoreException{
		disabled = false;
		highlightOpenEditors();
	}
	
	/**
	 * Removes all the created annotations from IAnnotationModel
	 * 
	 * @throws CoreException
	 */
	private void remove() throws CoreException{
		for (IAnnotationModel iam : iams){
			Iterator it = iam.getAnnotationIterator();
			while (it.hasNext()){
				Annotation an = (Annotation) it.next();
				String type = an.getType();
				if (type.equals("StaticAnalysisWarnings.specification_warning") ||
						type.equals("StaticAnalysisWarnings.specification_securitywarning")){	
						iam.removeAnnotation(an);
				}
			}
		}
	}
	
	/**
	 * Removes the stored warnings and all the created annotations from IAnnotationModel
	 * @throws CoreException
	 */
	public void clean() throws CoreException{
		remove();
		warnings = new ArrayList<Warning>();
	}
	
	/**
	 * Adds annotations to all the open editors.
	 * 
	 * @throws CoreException
	 */
	public void highlightOpenEditors() throws CoreException{
		if (disabled) return;
		if (warnings.isEmpty()) return;
		IEditorReference[] editors = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().getEditorReferences();
		for (IEditorReference ref : editors){
			IEditorPart editor = (IEditorPart)ref.getEditor(true);
			highlightEditor(editor);	
		}
	}
	
	/**
	 * Adds annotations to an editor.
	 * 
	 * @param editor	IEditorPart to be highlighted
	 * @throws 			CoreException
	 * @see				IEditorPart
	 */
	public void highlightEditor(IEditorPart editor) throws CoreException{
		if (disabled) return;
		if (warnings.isEmpty()) return;
		IFileEditorInput fileEditorInput;
		try {
			fileEditorInput = (IFileEditorInput)editor.getEditorInput();
		} catch (Exception e) { return; }
		IFile file = fileEditorInput.getFile();
		String editorPath = new String(file.getRawLocation().toString());
		
		if (containsPath(editorPath)){	
			if (!(editor instanceof ITextEditor)) return;
			IDocumentProvider idp = ((ITextEditor) editor).getDocumentProvider();
			IDocument document = idp.getDocument(editor.getEditorInput());

			IAnnotationModel iamf = idp.getAnnotationModel(editor.getEditorInput());
			iamf.connect(document);
			
			for(Warning w : warnings){
				if (w.filePath.equals(editorPath)){	
					IMarker marker = file.createMarker("StaticAnalysisWarnings.warningsMarker");
					
					marker.setAttribute(IMarker.MESSAGE, w.description);
					SimpleMarkerAnnotation ma = getAnnotation(w, marker);
					iamf.addAnnotation(ma,new Position(w.firstOffset,w.lastOffset-w.firstOffset+1));			
				}
			}
			iamf.disconnect(document);
			iams.add(iamf);
		}
	}
	
	/**
	 * Determines whether the list of warnings contains a warning found in a file with a specific path.
	 * 
	 * @param path		path to check for warnings
	 * @return			true if there is any warning found in the file
	 */
	private boolean containsPath(String path){
		for (Warning w: warnings){
			if (w.filePath.equals(path)) return true;
		}
		return false;
	}
	
	/**
	 * Gets the corresponding annotation to a warning type.
	 * @param w				warning
	 * @param marker		marker for the annotation
	 * @return				SimpleMarkerAnnotation
	 * @see					IMarker
	 * @see					SimpleMarkerAnnotation
	 */
	private SimpleMarkerAnnotation getAnnotation(Warning w, IMarker marker){
        if (w.security) return new SimpleMarkerAnnotation("StaticAnalysisWarnings.specification_securitywarning",marker);
        else return new SimpleMarkerAnnotation("StaticAnalysisWarnings.specification_warning",marker);
	}
	
	
}