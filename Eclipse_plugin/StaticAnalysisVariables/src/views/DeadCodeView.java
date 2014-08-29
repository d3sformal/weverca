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
import org.eclipse.jface.viewers.TreeViewer;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.graphics.Image;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.TreeColumn;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IPartListener;
import org.eclipse.ui.IWorkbenchPart;
import org.eclipse.ui.IWorkbenchWindow;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.part.ViewPart;

import representation.highlevel.ProgramPoint;
import staticanalysisvariables.DeadCode_Highlighter;

/**
 * This view shows the lines of code that are unreachable - dead code
 * 
 * @author Natalia Tyrpakova
 *
 */

public class DeadCodeView extends ViewPart {
	private TreeViewer viewer;
	private ArrayList<ProgramPoint> deadCode = new ArrayList<ProgramPoint>();
	private DeadCode_Highlighter highlighter;
	private IPartListener listener;
	private static ResourceComparator comparator;

	@Override
	public void createPartControl(Composite parent) {
		viewer = Creator_DeadCodeView.createView(parent);
		comparator = new ResourceComparator();
		viewer.setComparator(comparator);
		int index = 0;
		TreeColumn resourceColumn = viewer.getTree().getColumn(index);
		resourceColumn.addSelectionListener(getSelectionAdapter(resourceColumn, index, viewer));
	}

	@Override
	public void setFocus() {
		viewer.getControl().setFocus();
	}
	
	/**
	 * Sets the input for the TreeViewer inside of the view
	 * 
	 * @param input	list of unreachable ProgramPoints
	 */
	public void setInput(ArrayList<ProgramPoint> input){
		removeHighlight();
		deadCode = input;
		if (viewer != null) viewer.setInput(input);
		highlight();
	}
	
	/**
	 * {@inheritDoc}
	 * 
	 * In this implementation, highlight is removed.
	 */
	@Override
	public void dispose(){
		removeHighlight();
	}
	
	/**
	 * Removes the highlight and the listener.
	 */
	private void removeHighlight(){
		if (listener != null) getSite().getWorkbenchWindow().getPartService().removePartListener(listener);
		if (highlighter != null)
			try {
				highlighter.remove();
			} catch (CoreException e) {
				e.printStackTrace();
			}
		listener = null;
		highlighter = null;
	}
	
	/**
	 * Removes the highlight if it already exists, or creates it otherwise.
	 */
	public void updateHighlight(){
		if (highlighter != null) removeHighlight();
		else highlight();
	}
	
	/**
	 * Highlights current editor and adds listener to other editors
	 */
	private void highlight(){
		if (deadCode.isEmpty()) return;
		highlighter = new DeadCode_Highlighter(deadCode);
		addListener();
		IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
		//highlight current active editor
		if (window != null) {
			IEditorPart activeEditor = window.getActivePage().getActiveEditor();
		    if (activeEditor != null) {
		    	try {
					highlighter.highlightEditor(activeEditor);
				} catch (CoreException | BadLocationException e) {
					e.printStackTrace();
				}
		    }
		}
	}
	
	
	
	/**
	 * Adds listener to the editors, so they are highlighted when opened
	 */
	private void addListener(){
		listener = new IPartListener(){
			@Override
			public void partOpened(IWorkbenchPart part) {
				if (part instanceof IEditorPart)
					try {
						if (highlighter != null) highlighter.highlightEditor((IEditorPart)part);
					} catch (CoreException | BadLocationException e) {
						e.printStackTrace();
					}	
			}			
			@Override
			public void partActivated(IWorkbenchPart part) {
				if (part instanceof IEditorPart)
					try {
						if (highlighter != null) highlighter.highlightEditor((IEditorPart)part);
					} catch (CoreException | BadLocationException e) {
						e.printStackTrace();
					}
			}
			@Override
			public void partBroughtToTop(IWorkbenchPart part) {}
			@Override
			public void partClosed(IWorkbenchPart part) {}
			@Override
			public void partDeactivated(IWorkbenchPart part) {}				
		};
		getSite().getWorkbenchWindow().getPartService().addPartListener(listener);
	}
	
	
	/**
	 * This method sorts the TreeViewer by specified column using the ResourceComparator.
	 * 
	 * @param column  	a column to be sorted by
	 * @param index		zero-based index of a column to be sorted by
	 * @param viewer	TableViewer which is to be sorted
	 * @return			SelectionAdapter
	 * @see				TreeViewer
	 * @see				ResourceComparator
	 */
	private SelectionAdapter getSelectionAdapter(final TreeColumn column,final int index,final TreeViewer viewer) {
		SelectionAdapter selectionAdapter = new SelectionAdapter() {
	      @Override
	      public void widgetSelected(SelectionEvent e) {
	        comparator.clicked();
	        int dir = comparator.getDirection();
	        viewer.getTree().setSortDirection(dir);
	        viewer.getTree().setSortColumn(column);
	    
	        viewer.refresh();
	      }
	    };
	    return selectionAdapter;
	  }
	
	@Override
	public Image getTitleImage() {
	    return general.IconProvider.getDeadCodeIcon().createImage();
	}

}