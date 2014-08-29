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


package warnings;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.resources.IMarker;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.text.Position;
import org.eclipse.jface.text.source.Annotation;
import org.eclipse.jface.text.source.IAnnotationModel;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IFileEditorInput;
import org.eclipse.ui.texteditor.IDocumentProvider;
import org.eclipse.ui.texteditor.ITextEditor;
import org.eclipse.ui.texteditor.SimpleMarkerAnnotation;

import representation.Construct;
import representation.ConstructsInfo;
import representation.FileInfo;

/**
 * This class manages the annotations representing the editor warnings.
 * 
 * @author Natalia Tyrpakova
 */
class WarningsMaker {
	private ConstructsInfo searchResult = new ConstructsInfo();
	private ArrayList<IAnnotationModel> iams;
	private Map<String,IAnnotationModel> editorToAnModel = new HashMap<String,IAnnotationModel>();
	
	/**
	 * The constructor initializes an ArrayList of IAnnotationModel that stores all the annotations
	 * 
	 * @see IAnnotationModel
	 */
	public WarningsMaker(){
		iams = new ArrayList<IAnnotationModel>();
	}
	
	/**
	 * Sets the ConstructsInfo to provide information about construct occurrences
	 * 
	 * @param ci	ConstructsInfo
	 * @see			ConstructsInfo
	 */
	public void setConstructInfo(ConstructsInfo ci){
		searchResult = ci;
	}
	
	/**
	 * Indicator whether stored ConstructsInfo is null.
	 * @return	boolean
	 */
	boolean constructInfoIsNull(){
		return (searchResult == null);
	}
	
	
	/**
	 * Adds warnings to the given editor
	 * 
	 * @param editor		the IEditorPart to add warnings to
	 * @throws CoreException
	 * @see		IEditorPart
	 */
	void addWarningsToEditor(IEditorPart editor) throws CoreException{
		if (!(editor instanceof ITextEditor)) return;
		IFileEditorInput fileEditorInput = (IFileEditorInput)editor.getEditorInput();
		IFile file = fileEditorInput.getFile();
		String editorPath = new String(file.getRawLocation().toString());
		editorPath = editorPath.replace("/", "\\");
		
		ArrayList<FileInfo> fileInfos = searchResult.GetAllFilesInfo();
		FileInfo fileInfo = null;
		for (FileInfo fi: fileInfos){
			if (fi.path.equals(editorPath)) {
				fileInfo = fi;
				break;
			}
		}
		
		if (fileInfo != null){	
			IDocumentProvider idp = ((ITextEditor) editor).getDocumentProvider();
			IDocument document = idp.getDocument(editor.getEditorInput());
			IAnnotationModel iamf;
			iamf = editorToAnModel.get(editorPath);
			if (iamf == null) {
				iamf = idp.getAnnotationModel(editor.getEditorInput());
				editorToAnModel.put(editorPath, iamf);
			}
			else { //remove all current annotations 
				Iterator it = iamf.getAnnotationIterator();
				while (it.hasNext()){
					Annotation an = (Annotation) it.next();
					String type = an.getType();
					String substr = type.substring(0, 12);
					if (substr.equals("PHP_warnings")){
							iamf.removeAnnotation(an);
					}
				}
			}

			iamf.connect(document);
				
			for(Construct c : fileInfo.constructs){
				IMarker marker = file.createMarker("PHP_warnings_constructMarker");
				marker.setAttribute(IMarker.MESSAGE, c.type);
				SimpleMarkerAnnotation ma = new SimpleMarkerAnnotation("PHP_warnings.constructSpecification",marker);
				iamf.addAnnotation(ma,new Position(c.firstOffset,c.lastOffset-c.firstOffset+1));			
			}
			iamf.disconnect(document);
			iams.add(iamf);
		}
	}
	
	/**
	 * Removes the warnings from given editor
	 * 
	 * @param editor	the IEditorPart to remove warnings from
	 * @see			IEditorPart
	 */
	void removeWarningsFromEditor(IEditorPart editor){
		IFileEditorInput fileEditorInput = (IFileEditorInput)editor.getEditorInput();
		IFile file = fileEditorInput.getFile();
		String editorPath = new String(file.getRawLocation().toString());
		editorPath = editorPath.replace("/", "\\");
		IAnnotationModel iamf;
		iamf = editorToAnModel.get(editorPath);
		if (iamf != null) {
			Iterator it = iamf.getAnnotationIterator();
			while (it.hasNext()){
				Annotation an = (Annotation) it.next();
				String type = an.getType();
				String substr = type.substring(0, 12);
				if (substr.equals("PHP_warnings")){
						iamf.removeAnnotation(an);
				}
			}
		}
		editorToAnModel.remove(editorPath);
	}

}