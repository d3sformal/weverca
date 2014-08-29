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

import org.eclipse.core.runtime.CoreException;
import org.eclipse.jface.text.BadLocationException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.viewers.TreeViewer;
import org.eclipse.swt.SWT;
import org.eclipse.swt.custom.CaretEvent;
import org.eclipse.swt.custom.CaretListener;
import org.eclipse.swt.custom.StyledText;
import org.eclipse.swt.graphics.Image;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Control;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IPartListener;
import org.eclipse.ui.IWorkbenchPage;
import org.eclipse.ui.IWorkbenchPart;
import org.eclipse.ui.IWorkbenchWindow;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.dialogs.FilteredTree;
import org.eclipse.ui.part.ViewPart;
import org.eclipse.ui.texteditor.AbstractTextEditor;
import org.eclipse.ui.texteditor.ITextEditor;

import representation.highlevel.File;
import staticanalysisvariables.VariablesPatternFilter;
import fileselectors.ActiveEditor;

/**
 * This view shows the filtered tree of variables that result from the static analysis.
 * 
 * @author Natalia Tyrpakova
 * @see TreeViewer
 * @see FilteredTree
 */
public class VariablesView extends ViewPart {
	private TreeViewer viewer;
	private ArrayList<File> roots = new ArrayList<File>();
	private boolean before = false;
	private String editorPath;
	private int line;
	private FilteredTree tree;
	private boolean newInput = false;
	
	/**
	 * Determines whether to show local and global controls and fields without parent objects
	 */
	private boolean showControls = true;

	/**
	 * This feature is not compatible with Eclipse versions 3.4 and older
	 */
	private CaretListener caretListener = new CaretListener() {
		@Override
		public void caretMoved(CaretEvent event) {
			int offset = event.caretOffset;
			IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
			if (window != null) {
				IEditorPart activeEditor = window.getActivePage().getActiveEditor();
				if (activeEditor instanceof ITextEditor){
					ITextEditor activeTextEditor = (ITextEditor)activeEditor;
			    	IDocument document = activeTextEditor.getDocumentProvider().getDocument(activeTextEditor.getEditorInput());
			    	try {
			    		int newLine = document.getLineOfOffset(offset)+1;
			    		String newEditorPath = ActiveEditor.getCurrentEditor(false);
						if (newInput || newLine != line || !newEditorPath.equals(editorPath)) {
							newInput = false;
							line = newLine;
							editorPath = newEditorPath;
							Creator_VariablesView.createView(viewer,tree, roots, line, editorPath,before, showControls);
						}
						} catch (BadLocationException e) {
						e.printStackTrace();
					}
				}		
			}
		}
	};
	
	/**
	 * A listener for the active editor. Adds a CaretListener to an editor whenever it is opened
	 * and removes it when it is closed.
	 */
	private IPartListener editorListener = new IPartListener(){
		@Override
		public void partOpened(IWorkbenchPart part) {
			if (part instanceof IEditorPart){
				AbstractTextEditor e = (AbstractTextEditor)(IEditorPart)part;
			    ((StyledText)e.getAdapter(Control.class)).addCaretListener(caretListener);
			}
		}
		@Override
		public void partActivated(IWorkbenchPart part) {
			if (part instanceof IEditorPart){
				AbstractTextEditor e = (AbstractTextEditor)(IEditorPart)part;
			    ((StyledText)e.getAdapter(Control.class)).addCaretListener(caretListener);
			}
		}
		@Override
		public void partClosed(IWorkbenchPart part) {
			if (part instanceof IEditorPart){
				AbstractTextEditor e = (AbstractTextEditor)(IEditorPart)part;
			    ((StyledText)e.getAdapter(Control.class)).removeCaretListener(caretListener);
			}
		}
		@Override
		public void partDeactivated(IWorkbenchPart part) {}	 
		@Override
		public void partBroughtToTop(IWorkbenchPart part) {}
	 };
	
	 /**
	  * {@inheritDoc}
	  * 
	  * In this implementation, a TreeViewer is created and caret listener is added to the editors.
	  */
	@Override
	public void createPartControl(Composite parent) {
		VariablesPatternFilter filter = new VariablesPatternFilter();
		
		tree = new FilteredTree(parent, SWT.MULTI | SWT.H_SCROLL| SWT.V_SCROLL, filter, true);
		viewer = tree.getViewer();
		addCaretListener();	
	}

	@Override
	public void setFocus() {
		viewer.getControl().setFocus();
	}
	
	/**
	 * Sets the roots field which is an input for the TreeViewer held by this view.
	 * 
	 * @param analysisResult	list of Files containing all the information
	 * @throws CoreException 
	 */
	public void setInput(ArrayList<File> files) throws CoreException{
		roots = files;
		newInput = true;
	}
	
	/**
	 * {@inheritDoc}
	 * 
	 * In this implementation, the caret listener is removed.
	 */
	public void dispose() {
		super.dispose();
		removeCaretListener();
	}
	
	/**
	 * Adds a caret listener to all the open editors
	 * 
	 * @see CaretListener
	 */
	private void addCaretListener(){
		IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
	     if (window != null) {
			 IWorkbenchPage[] pages = window.getPages();
			 for (int i = 0; i < pages.length; i++) {
				 IWorkbenchPage p = pages[i];
				 p.addPartListener(editorListener);
			 }
	     }
	}
	
	/**
	 * Removes the caret listener from all the open editors.
	 * 
	 * @see CaretListener
	 */
	private void removeCaretListener(){
		IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
	     if (window != null) {
			 IWorkbenchPage[] pages = window.getPages();
			 for (int i = 0; i < pages.length; i++) {
				 IWorkbenchPage p = pages[i];
				 p.removePartListener(editorListener);
			 }
	     }
	}
	
	/**
	 * Sets the before value, which determines whether to show variables 
	 * computed before the line had been processed, or after it.
	 * 
	 * @param before		boolean to set
	 * @throws CoreException
	 */
	public void setBefore(boolean before) throws CoreException{
		this.before = before;
		Creator_VariablesView.createView(viewer,tree, roots, line, editorPath,before, showControls);
	}
	
	/**
	 * Gets the current before value.
	 * 
	 * @return		boolean before
	 */
	public boolean getBefore(){
		return before;
	}
	
	
	@Override
	public Image getTitleImage() {
	    return general.IconProvider.getViewIcon().createImage();
	}

}