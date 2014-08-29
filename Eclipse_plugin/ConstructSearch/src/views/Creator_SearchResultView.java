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

import java.io.File;
import java.util.ArrayList;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.resources.ResourcesPlugin;
import org.eclipse.core.runtime.Path;
import org.eclipse.jface.viewers.DoubleClickEvent;
import org.eclipse.jface.viewers.IDoubleClickListener;
import org.eclipse.jface.viewers.IStructuredSelection;
import org.eclipse.jface.viewers.TreeViewer;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IWorkbenchPage;
import org.eclipse.ui.PartInitException;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.ide.IDE;
import org.eclipse.ui.texteditor.ITextEditor;

import representation.ConstructsInfo;
import representation.tree.TreeConstruct;
import representation.tree.TreeFile;
import representation.tree.TreeFolder;
import views.content.ViewContentProvider;
import views.content.ViewLabelProvider;

/**
 * This class creates a TreeViewer that is shown in the SearchResultView.
 * Its only method is a static method that creates the TreeViewer.
 * 
 * @author 	Natalia Tyrpakova
 * @see		TreeViewer
 */
class Creator_SearchResultView {
	
	/**
	 * Creates the input for a TreeViewer from a ConstructsInfo and a list of roots. 
	 * All the nodes are instances of TreeFolder, TreeFile or TreeConstruct.
	 * 
	 * @param viewer		TreeViewer to set the input 
	 * @param roots			root nodes of the tree
	 * @param searchresult	ConstructsInfo with the result of construct occurrence search
	 */
	static void createView(TreeViewer viewer, ArrayList<String> roots, ConstructsInfo searchresult){
		viewer.setContentProvider(new ViewContentProvider());
		viewer.setLabelProvider(new ViewLabelProvider());
		
		if (roots != null){	
			ArrayList<Object> rootsForViewer = new ArrayList<Object>();
			for (String root:roots){
				File rootfile = new File(root);
				if (rootfile.isDirectory()){
					TreeFolder folderRoot = new TreeFolder(rootfile, searchresult);
					rootsForViewer.add(folderRoot);
				}
				else {
					TreeFile fileRoot = new TreeFile(rootfile,searchresult);
					rootsForViewer.add(fileRoot);
				}
			}
			viewer.setInput(rootsForViewer);
			
			
			viewer.addDoubleClickListener(new IDoubleClickListener(){
				 public void doubleClick(DoubleClickEvent event) {
					    IStructuredSelection thisSelection = (IStructuredSelection) event.getSelection(); 
					    Object selectedNode = thisSelection.getFirstElement(); 
					    if (selectedNode instanceof TreeFile){
					    	IWorkbenchPage page = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage();
					    	File filetoopen = ((TreeFile)selectedNode).tFile;
					    	IFile file = ResourcesPlugin.getWorkspace().getRoot().getFileForLocation(Path.fromOSString(filetoopen.getAbsolutePath()));
					    	try {
					    		IDE.openEditor(page, file);
							} catch (PartInitException e) {
								return;
							}
	
					  }
					    else if (selectedNode instanceof TreeConstruct){
					    	TreeConstruct foundConstruct = (TreeConstruct)selectedNode;
					    	IWorkbenchPage page = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage();
					    	File filetoopen = ((TreeConstruct)selectedNode).parent.tFile;
					    	IFile file = ResourcesPlugin.getWorkspace().getRoot().getFileForLocation(Path.fromOSString(filetoopen.getAbsolutePath()));
					    	 try {
					    		 IEditorPart editorPart = IDE.openEditor(page, file);
					    		 if (editorPart instanceof ITextEditor){
					    			 ITextEditor editor = (ITextEditor)editorPart;
									 editor.selectAndReveal(foundConstruct.firstOffset,foundConstruct.lastOffset-foundConstruct.firstOffset+1);     		
					    		 }
							} catch (PartInitException e) {
								return;
							}
					    }
				 }
			
			});
		}
		else { 
			viewer.setInput("Use search to get the search results");
		}
	}
}