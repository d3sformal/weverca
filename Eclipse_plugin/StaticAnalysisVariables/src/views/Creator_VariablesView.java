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


package views;

import java.util.ArrayList;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.resources.IWorkspace;
import org.eclipse.core.resources.ResourcesPlugin;
import org.eclipse.core.runtime.IPath;
import org.eclipse.core.runtime.Path;
import org.eclipse.jface.text.BadLocationException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.text.IRegion;
import org.eclipse.jface.viewers.DoubleClickEvent;
import org.eclipse.jface.viewers.IDoubleClickListener;
import org.eclipse.jface.viewers.IStructuredSelection;
import org.eclipse.jface.viewers.TreeViewer;
import org.eclipse.swt.widgets.TreeItem;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IWorkbenchPage;
import org.eclipse.ui.PartInitException;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.dialogs.FilteredTree;
import org.eclipse.ui.ide.IDE;
import org.eclipse.ui.texteditor.ITextEditor;

import representation.highlevel.File;
import representation.highlevel.ProgramPoint;
import representation.variables.VariableType;
import callstacks.Call;
import views.contentproviders.ContentProvider_VariablesView;
import views.labelproviders.LabelProvider_VariablesView;

/**
 * This class sets the input to the TreeViewer that is shown in VariablesView and adds a listeners,
 * that shows the corresponding line when an element of Call type is double clicked.
 * 
 * @author 	Natalia Tyrpakova
 * @see 	TreeViewer
 */
class Creator_VariablesView {
	
	/**
	 * Sets input to a TreeViewer from a list of Files. The input is a ProgramPoint,
	 * corresponding to the line selected by the cursor.
	 * All the nodes are instances of ProgramPoint, Context, Call, VariableType, Alias,
	 * Variable, AliasType or Value
	 * 
	 * @param viewer		TreeViewer to set the input 
	 * @param tree			FilteredTree of elements that matches the user-defined pattern
	 * @param roots			list of Files with all the ProgramPoints
	 * @param line			line, pointed by a cursor
	 * @param editor		current active editor
	 * @param before		boolean determining whether to show the line input set or output set
	 * @param showControls	special parameter, determines whether to show local and global controls and fields with no parent object
	 */
	static void createView(TreeViewer viewer,FilteredTree tree, ArrayList<File> roots, int line, String editor,boolean before, boolean showControls){
		ContentProvider_VariablesView vcp = new ContentProvider_VariablesView();
		vcp.showControls = showControls;
		vcp.before = before;
		viewer.setContentProvider(vcp);
		viewer.setLabelProvider(new LabelProvider_VariablesView(tree));
		ProgramPoint pPoint = null;
		
		for (File file : roots){
			String filePath = file.filePath.replace("\\","/");
			if (filePath.equals(editor)){
				pPoint = file.programPoints.get(line);
			}
			if (pPoint != null) break;
		}
		
		if (pPoint != null){	
			viewer.setInput(pPoint);
		}
		else viewer.setInput(null);
		setDefaultExpandedNodes(viewer);
		addListener(viewer);
	}
	
	/**
	 * Sets the default expanded nodes - local and global variables
	 * 
	 * @param viewer	viewer to set the expanded nodes for
	 */
	private static void setDefaultExpandedNodes(TreeViewer viewer) {
        viewer.getControl().setRedraw(false);
        viewer.expandToLevel(2);
        for (TreeItem item : viewer.getTree().getItems()) {
        		setExpandTreeItem(item, viewer.getTree().getItems().length <= 4);
        }
        viewer.getControl().setRedraw(true);
	}
	
	/**
	 * Sets whether the specific treeItem should be expanded or not
	 * 
	 * @param treeItem	tree item to set expanded state for
	 */
	private static void setExpandTreeItem(final TreeItem treeItem, boolean expandGlobal) {
		if (treeItem.getData() != null) {
			if (treeItem.getData() instanceof VariableType) {
				if (((VariableType)treeItem.getData()).type.equals("Local variables")){
					treeItem.setExpanded(true);
					return;
				} else if (expandGlobal && ((VariableType)treeItem.getData()).type.equals("Global variables")){
					treeItem.setExpanded(true);
					return;
				}
			}
		}
		treeItem.setExpanded(false);
	}
	
	
	
	/**
	 * Adds a double click listener to the view. 
	 * The listener highlights a selected call point position in the corresponding editor.
	 * 
	 * @param viewer	viewer to add the listener to
	 */
	private static void addListener(TreeViewer viewer){
		viewer.addDoubleClickListener(new IDoubleClickListener(){
			public void doubleClick(DoubleClickEvent event) {
				IStructuredSelection thisSelection = (IStructuredSelection) event.getSelection(); 
				Object selectedRow = thisSelection.getFirstElement();
				if (selectedRow instanceof Call){
					java.io.File file = new java.io.File(((Call)selectedRow).filePath);
					if (file.exists() && file.isFile()){
						IWorkbenchPage page = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage();	
					    IWorkspace workspace= ResourcesPlugin.getWorkspace();    
					    IPath location= Path.fromOSString(file.getAbsolutePath()); 
					    IFile ifile= workspace.getRoot().getFileForLocation(location);
					    try {
					    	IEditorPart editorPart = IDE.openEditor(page, ifile);
					    	if (editorPart instanceof ITextEditor){
					    		ITextEditor editor = (ITextEditor)editorPart;
						    	 IDocument document = editor.getDocumentProvider().getDocument(editor.getEditorInput());
				    			 if (document != null) {
					    			 IRegion firstlineInfo = document.getLineInformation(((Call)selectedRow).firstLine -1);
					    			 IRegion lastlineInfo = document.getLineInformation(((Call)selectedRow).lastLine-1);
					    			 int firstOffset = firstlineInfo.getOffset() + ((Call)selectedRow).firstCol -1;
					    			 int lastOffset = lastlineInfo.getOffset() + ((Call)selectedRow).lastCol;
					    			 editor.selectAndReveal(firstOffset,lastOffset-firstOffset);
				    			 }
					    	}	    	
							} catch (PartInitException | BadLocationException e) {
									e.printStackTrace();
								}
					   
					}
				}
				
			}

	});	
	}
}