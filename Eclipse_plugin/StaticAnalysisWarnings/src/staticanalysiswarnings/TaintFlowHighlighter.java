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

import representation.points.FirstFlowPoint;
import representation.points.FlowPoint;
import representation.points.Point;
import representation.points.SplitPoint;

/**
 * Highlighter for the taint flows
 * 
 * @author Natalia Tyrpakova
 *
 */
public class TaintFlowHighlighter {
	private ArrayList<Point> flow;
	private ArrayList<IAnnotationModel> iams;
	
	/**
	 * The constructor initializes the fields, particularly the list
	 * that stores the points of flow to be highlighted.
	 * 
	 * @param warnings	an ArrayList of warnings to be highlighted
	 */
	public TaintFlowHighlighter(ArrayList<Point> flow){
		this.flow = flow;
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
				if (type.equals("StaticAnalysisWarnings.specification_flow")){
						iam.removeAnnotation(an);
				}
			}
		}
	}
	
	/**
	 * Adds annotations to an editor.
	 * 
	 * @param editor	IEditorPart to be highlighted
	 * @throws 			CoreException
	 * @throws BadLocationException 
	 * @see				IEditorPart
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
			
			for(Point p : flow){
				if (p == null) continue;
				if (equalPath(editorPath,p.filePath)){	
					IMarker marker = file.createMarker("StaticAnalysisWarnings.warningsMarker");
					marker.setAttribute(IMarker.MESSAGE, "flow");
					SimpleMarkerAnnotation ma = new SimpleMarkerAnnotation("StaticAnalysisWarnings.specification_flow",marker);
					IRegion firstlineInfo = document.getLineInformation(p.firstLine -1);
	    			IRegion lastlineInfo = document.getLineInformation(p.lastLine-1);
	    			int firstOffset = firstlineInfo.getOffset() + p.firstCol -1;
	    			int lastOffset = lastlineInfo.getOffset() + p.lastCol;
					iamf.addAnnotation(ma,new Position(firstOffset,lastOffset-firstOffset));			
				}
			}
			iamf.disconnect(document);
			iams.add(iamf);
		}
	}
	
	/**
	 * Gets a set of points from the flow determined by a list of FlowPoints, FirstFlowPoint, 
	 * SplitPoints and Resources
	 * 
	 * @param 	list	flow as a list of FlowPoints and Resources
	 * @return	set of points from the flow 
	 */
	public static ArrayList<Point> getPoints(ArrayList<Object> list){
		ArrayList<Point> result = new ArrayList<Point>();
		for (Object o: list){
			if (o instanceof FirstFlowPoint){
				if (!result.contains(((FirstFlowPoint)o).point)) result.add(((FirstFlowPoint)o).point);
				ArrayList<Point> points = getPoints(((FirstFlowPoint)o).flows);
				for (Point p : points){
					if (!result.contains(p)) result.add(p);
				}
			}
			if (o instanceof SplitPoint){
				if (!result.contains(((SplitPoint)o).point)) result.add(((SplitPoint)o).point);
				ArrayList<Point> points = getPoints(((SplitPoint)o).flows);
				for (Point p : points){
					if (!result.contains(p)) result.add(p);
				}
			}
			if (o instanceof FlowPoint){
				if (!result.contains(((FlowPoint)o).point)) result.add(((FlowPoint)o).point);
			}
		}
		return result;
	}
	
	/**
	 * Determines whether the list of points contains a point from the given file
	 * 
	 * @param path		path to check for points
	 * @return			true if the file contains any point
	 */
	private boolean containsPath(String path){
		for (Point p: flow){
			if (p!= null && equalPath(path,p.filePath)) return true;
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