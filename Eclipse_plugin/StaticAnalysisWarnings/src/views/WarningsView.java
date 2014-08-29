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
import org.eclipse.jface.viewers.TreeViewer;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.graphics.Image;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.TreeColumn;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IPartListener;
import org.eclipse.ui.ISharedImages;
import org.eclipse.ui.IWorkbenchPart;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.part.ViewPart;

import representation.Warning;
import staticanalysiswarnings.WarningsHighlighter;

/**
 * This view shows the warnings, their call strings and possible taint flows that result 
 * from the static analysis.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class WarningsView extends ViewPart {
	private TreeViewer viewer;
	private WarningsHighlighter highlighter = new WarningsHighlighter();
	private ArrayList<Warning> warnings;
	private boolean merge = false;
	private boolean reverse = false;
	private static WarningsComparator comparator;
	
	
	/**
	 * {@inheritDoc}
	 * 
	 * In this implementation, a TreeViewer is created and part listener is added to each editor.
	 */
	@Override
	public void createPartControl(Composite parent) {
		
		viewer = Creator_WarningsView.createView(parent, highlighter);
		
		comparator = new WarningsComparator();
		viewer.setComparator(comparator);
		int[] indices = new int[] {1,3}; 
		for(int index : indices){
			TreeColumn resourceColumn = viewer.getTree().getColumn(index);
			resourceColumn.addSelectionListener(getSelectionAdapter(resourceColumn, index, viewer));
		}
			
		getSite().getWorkbenchWindow().getPartService().addPartListener(new IPartListener(){

			@Override
			public void partOpened(IWorkbenchPart part) {
				if (part instanceof IEditorPart)
					try {
						if (highlighter != null) highlighter.highlightEditor((IEditorPart)part);
					} catch (CoreException e) {
						e.printStackTrace();
					}	
			}
			
			@Override
			public void partActivated(IWorkbenchPart part) {}
			@Override
			public void partBroughtToTop(IWorkbenchPart part) {}
			@Override
			public void partClosed(IWorkbenchPart part) {}
			@Override
			public void partDeactivated(IWorkbenchPart part) {}				
		});

	}

	/**
	 * {@inheritDoc}
	 */
	@Override
	public void setFocus() {
		viewer.getControl().setFocus();
	}
	
	/**
	 * Gets data for the view and sets the input to the TreeViewer
	 * 
	 * @param analysisResult	list of warnings to show
	 * @throws CoreException 
	 */
	public void setInput(ArrayList<Warning> analysisResult) throws CoreException{
		highlighter.clean();
		highlighter.setWarnings(analysisResult);
		highlighter.highlightOpenEditors();
		warnings = analysisResult;
		for (Warning warning : warnings){
			warning.setMerge(merge, reverse);
		}
		viewer.setInput(analysisResult);
	}
	
	/**
	 * Gets data for the view and sets the input to the TreeViewer
	 * 
	 * @param analysisResult	analysis result
	 * @throws CoreException 
	 */
	public void setInput(String analysisResult) throws CoreException{
		highlighter.clean();
		warnings = new ArrayList<Warning>();
		viewer.setInput(analysisResult);
	}
	
	
	
	@Override
	public void dispose(){
		try {
			highlighter.clean();
		} catch (CoreException e) {
			e.printStackTrace();
		}
	}
	
	/**
	 * Sets the output form - whether the taint flows should be merged or not
	 * 
	 * @param merge		merge indicator
	 */
	public void setOutput(boolean merge, boolean reverse){
		this.merge = merge;
		this.reverse = reverse;
		for (Warning warning : warnings){
			warning.setMerge(merge, reverse);
		}
		Object[] expandedElements = new Object[0];
		if (viewer != null) expandedElements = viewer.getExpandedElements();
		viewer.setInput(warnings);
		viewer.setExpandedElements(expandedElements);
	}
	
	
	/**
	 * This method sorts the TreeViewer by specified column using the ResourceComparator.
	 * 
	 * @param column  	a column to be sorted by
	 * @param index		zero-based index of a column to be sorted by
	 * @param viewer	TableViewer which is to be sorted
	 * @return			SelectionAdapter
	 * @see				TreeViewer
	 * @see				WarningsComparator
	 */
	private SelectionAdapter getSelectionAdapter(final TreeColumn column,final int index,final TreeViewer viewer) {
		SelectionAdapter selectionAdapter = new SelectionAdapter() {
	      @Override
	      public void widgetSelected(SelectionEvent e) {
	        comparator.setColumn(index);
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
	    return PlatformUI.getWorkbench().getSharedImages().getImage(ISharedImages.IMG_OBJS_WARN_TSK);
	}
}