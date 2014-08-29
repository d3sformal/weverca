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


package staticanalysisvariables;

import java.util.ArrayList;
import java.util.Iterator;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.resources.IMarker;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.jface.text.BadLocationException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.text.IRegion;
import org.eclipse.jface.text.Position;
import org.eclipse.jface.text.source.Annotation;
import org.eclipse.jface.text.source.IAnnotationModel;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IFileEditorInput;
import org.eclipse.ui.texteditor.IDocumentProvider;
import org.eclipse.ui.texteditor.ITextEditor;
import org.eclipse.ui.texteditor.SimpleMarkerAnnotation;

import representation.highlevel.ProgramPoint;

/**
 * Highlighter for the dead code lines
 * 
 * @author Natalia Tyrpakova
 *
 */
public class DeadCode_Highlighter {
	private ArrayList<ProgramPoint> deadCode;
	private ArrayList<IAnnotationModel> iams;
	
	/**
	 * The constructor initializes the fields, particularly the list of ProgramPoints
	 * that stores all the dead code to be highlighted.
	 * 
	 * @param deadCode	an ArrayList of ProgramPoints of lines to be highlighted
	 */
	public DeadCode_Highlighter(ArrayList<ProgramPoint> deadCode){
		this.deadCode = deadCode;
		iams = new ArrayList<IAnnotationModel>();
	}
	
	/**
	 * Removes all the created annotations form IAnnotationModel
	 * 
	 * @throws CoreException
	 */
	public void remove() throws CoreException{
		for (IAnnotationModel iam : iams){
			Iterator it = iam.getAnnotationIterator();
			while (it.hasNext()){
				Annotation an = (Annotation) it.next();
				String type = an.getType();
				String substr = type.substring(0, 23);
				if (substr.equals("StaticAnalysisVariables")){
						iam.removeAnnotation(an);
				}
			}
		}
	}
	
	/**
	 * Adds annotations to an editor.
	 * 
	 * @param editor			IEditorPart to be highlighted
	 * @throws CoreException
	 * @throws BadLocationException
	 */
	public void highlightEditor(IEditorPart editor) throws CoreException, BadLocationException{
		if (!(editor instanceof ITextEditor)) return;
		IFileEditorInput fileEditorInput = (IFileEditorInput)editor.getEditorInput();
		IFile file = fileEditorInput.getFile();
		String editorPath = new String(file.getRawLocation().toString());
		
		if (containsPath(editorPath)){	
			IDocumentProvider idp = ((ITextEditor) editor).getDocumentProvider();
			IDocument document = idp.getDocument(editor.getEditorInput());

			IAnnotationModel iamf = idp.getAnnotationModel(editor.getEditorInput());
			iamf.connect(document);
			
			for(ProgramPoint p : deadCode){
				if (equalPath(editorPath,p.parentFile.filePath)){	
					IMarker marker = file.createMarker("StaticAnalysisVariables.deadCodeMarker");
					marker.setAttribute(IMarker.MESSAGE, "Unreachable code");
					SimpleMarkerAnnotation ma = new SimpleMarkerAnnotation("StaticAnalysisVariables.specification_deadCode",marker);
					IRegion firstlineInfo = document.getLineInformation(p.point.firstLine -1);
	    			IRegion lastlineInfo = document.getLineInformation(p.point.lastLine);
	    			int firstOffset = firstlineInfo.getOffset();
	    			int lastOffset = lastlineInfo.getOffset();
					iamf.addAnnotation(ma,new Position(firstOffset,lastOffset-firstOffset-1));			
				}
			}
			iamf.disconnect(document);
			iams.add(iamf);
		}
	}
	
	/**
	 * Determines whether the file contains any dead code
	 * 
	 * @param path		file path to check for dead code
	 * @return			true if there is any dead code in the file
	 */
	private boolean containsPath(String path){
		for (ProgramPoint p : deadCode){
			if (equalPath(path,p.parentFile.filePath)) return true;
		}
		return false;
	}
	
	/**
	 * Determines whether two file paths represent the same file
	 * 
	 * @param path			first file path
	 * @param otherPath		second file path
	 * @return				true if they represent the same file
	 */
	private boolean equalPath(String path, String otherPath){
		String path2 = path.replace("/", "\\");
		if (otherPath.equals(path)) return true;
		if (otherPath.equals(path2)) return true;
		return false;
	}
	
	
}