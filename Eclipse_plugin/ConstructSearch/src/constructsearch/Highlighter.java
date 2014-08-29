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


package constructsearch;

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

import representation.Construct;
import representation.ConstructsInfo;

/**
 * This class is dedicated to highlight the construct occurrences in editors.
 * Highlighting is done using the IMarker and SimpleMarkerAnnotations that are stored in an IAnnotationModel.
 * 
 * @author 	Natalia Tyrpakova
 * @see		SimpleMarkerAnnotation
 * @see		IMarker
 * @see		IAnnotationModel
 *
 */
public class Highlighter {
	private ConstructsInfo searchResult;
	private ArrayList<IAnnotationModel> iams;
	
	/**
	 * The constructor initializes the fields, particularly the ConstructsInfo
	 * that stores all the construct occurrences to be highlighted.
	 * 
	 * @param ci	the ConstructsInfo with construct occurrences
	 */
	public Highlighter(ConstructsInfo ci){
		searchResult = ci;
		iams = new ArrayList<IAnnotationModel>();
	}
	
	/**
	 * Removes all the annotations created by this plug-in
	 * 
	 * @throws CoreException
	 */
	public void remove() throws CoreException{
		for (IAnnotationModel iam : iams){
			Iterator it = iam.getAnnotationIterator();
			while (it.hasNext()){
				Annotation an = (Annotation) it.next();
				String type = an.getType();
				String substr = type.substring(0, 15);
				if (substr.equals("ConstructSearch")){
						iam.removeAnnotation(an);
				}
			}
		}
	}
	
	/**
	 * Highlights the construct occurrences in all the open editors.
	 * 
	 * @throws CoreException
	 */
	public void highlightOpenEditors() throws CoreException{
		IEditorReference[] editors = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().getEditorReferences();
		for (IEditorReference ref : editors){
			IEditorPart editor = (IEditorPart)ref.getEditor(true);
			highlightEditor(editor);	
		}
	}
	
	/**
	 * Adds annotations to an editor. The type of annotation depends on the construct.
	 * 
	 * @param editor	IEditorPart to be highlighted
	 * @throws 			CoreException
	 * @see				IEditorPart
	 */
	void highlightEditor(IEditorPart editor) {
		IFileEditorInput fileEditorInput = (IFileEditorInput)editor.getEditorInput();
		IFile file = fileEditorInput.getFile();
		String editorPath = new String(file.getRawLocation().toString());
		editorPath = editorPath.replace("/", "\\");
		
		if (searchResult.getAllFiles().contains(editorPath)){	
			if (!(editor instanceof ITextEditor)) return;
			IDocumentProvider idp = ((ITextEditor) editor).getDocumentProvider();
			IDocument document = idp.getDocument(editor.getEditorInput());

			IAnnotationModel iamf = idp.getAnnotationModel(editor.getEditorInput());
			iamf.connect(document);
			
			for(Construct fi : searchResult.getAllFilesInfo()){
				if (fi.path.equals(editorPath)){					
					try {
						IMarker marker = file.createMarker("ConstructSearch.searchresultmarker");
						marker.setAttribute(IMarker.MESSAGE, fi.construct);
						SimpleMarkerAnnotation ma = getAnnotation(fi.construct, marker);
						iamf.addAnnotation(ma,new Position(fi.position[1],fi.position[3]-fi.position[1]+1));
					} catch (CoreException e) {
						continue;
					}			
				}
			}
			iamf.disconnect(document);
			iams.add(iamf);
		}
	}
	
	/**
	 * Gets the corresponding annotation for a given construct type
	 * 
	 * @param construct		the construct type to get the annotation for
	 * @param marker		marker for the annotation
	 * @return				annotation as a SimpleMarkerAnnotation
	 * @see					IMarker
	 * @see					SimpleMarkerAnnotation
	 */
	private SimpleMarkerAnnotation getAnnotation(String construct, IMarker marker){
        if (construct.equals("SQL")) return new SimpleMarkerAnnotation("ConstructSearch.specification_sql",marker);
        if (construct.equals("Sessions"))  return new SimpleMarkerAnnotation("ConstructSearch.specification_sessions",marker);
		if (construct.equals("Autoload")) return new SimpleMarkerAnnotation("ConstructSearch.specification_autoload",marker);
        if (construct.equals("Magic methods")) return new SimpleMarkerAnnotation("ConstructSearch.specification_magic",marker);
        if (construct.equals("Class presence")) return new SimpleMarkerAnnotation("ConstructSearch.specification_class",marker);
        if (construct.equals("Aliasing")) return new SimpleMarkerAnnotation("ConstructSearch.specification_aliasing",marker);
        if (construct.equals("Inside function declaration")) return new SimpleMarkerAnnotation("ConstructSearch.specification_insidefunct",marker);
        if (construct.equals("Use of super global variable")) return new SimpleMarkerAnnotation("ConstructSearch.specification_superglobalvar",marker);
        if (construct.equals("Dynamic call")) return new SimpleMarkerAnnotation("ConstructSearch.specification_dyncall",marker);
        if (construct.equals("Dynamic dereference")) return new SimpleMarkerAnnotation("ConstructSearch.specification_dynderef",marker);
        if (construct.equals("Dynamic include")) return new SimpleMarkerAnnotation("ConstructSearch.specification_dyninclude",marker);
        if (construct.equals("Eval")) return new SimpleMarkerAnnotation("ConstructSearch.specification_eval",marker);
        if (construct.equals("Passing by reference at call side"))  return new SimpleMarkerAnnotation("ConstructSearch.specification_passingbyref",marker);

        else return null;
	}

}