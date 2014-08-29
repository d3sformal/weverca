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
import org.eclipse.jface.layout.TreeColumnLayout;
import org.eclipse.jface.text.BadLocationException;
import org.eclipse.jface.viewers.ColumnWeightData;
import org.eclipse.jface.viewers.DecoratingStyledCellLabelProvider;
import org.eclipse.jface.viewers.DoubleClickEvent;
import org.eclipse.jface.viewers.IDoubleClickListener;
import org.eclipse.jface.viewers.IStructuredSelection;
import org.eclipse.jface.viewers.TreeViewer;
import org.eclipse.jface.viewers.TreeViewerColumn;
import org.eclipse.swt.SWT;
import org.eclipse.swt.graphics.Image;
import org.eclipse.swt.layout.GridData;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Tree;
import org.eclipse.swt.widgets.TreeColumn;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IPartListener;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.IWorkbenchPart;
import org.eclipse.ui.IWorkbenchWindow;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.part.ViewPart;

import representation.points.FlowPoint;
import representation.points.Point;
import staticanalysiswarnings.TaintFlowHighlighter;
import staticanalysiswarnings.WarningsHighlighter;
import views.contentproviders.ContentProvider_TaintFlowView;
import views.labelproviders.LabelProviderCol1_TaintFlowView;
import views.labelproviders.LabelProviderCol2_TaintFlowView;
import general.TextSelector;

/**
 * This view shows information about a specific taint flow
 * 
 * @author Natalia Tyrpakova
 *
 */
public class TaintFlowView extends ViewPart {
	private ArrayList<Point> flow;
	private TreeViewer viewer;
	private IPartListener listener;
	private TaintFlowHighlighter highlighter;
	private WarningsHighlighter warningsHighlighter;
	
	@Override
	public void createPartControl(Composite parent) {
		createViewer(parent);
		addListener();
	}

	@Override
	public void setFocus() {
		viewer.getControl().setFocus();
	}
	
	
	/**
	 * {@inheritDoc}
	 * 
	 * In this implementation highlight and part listener are removed.
	 */
	@Override
	public void dispose() {
		if (listener != null) getSite().getWorkbenchWindow().getPartService().removePartListener(listener);
		listener = null;
		removeHighlight();
	}
	
	/**
	 * Sets the TreeViewer input to show in this view
	 * 
	 * @param input		input to show in the view
	 */
	void setInput(ArrayList<Object> input, WarningsHighlighter highlighter){
		warningsHighlighter = highlighter;
		removeHighlight();
		if (input != null) {
			flow = TaintFlowHighlighter.getPoints(input);
			viewer.setInput(input);
		}
		highlight();
	}
	
	/**
	 * Removes the highlight
	 */
	private void removeHighlight(){
		if (highlighter != null)
			try {
				warningsHighlighter.enable();
				highlighter.remove();
			} catch (CoreException e) {
				e.printStackTrace();
			}
		highlighter = null;
	}
	
	/**
	 * Highlights current editor and adds listener to other editors 
	 */
	private void highlight(){
		if (flow == null || flow.isEmpty()) return;
		try {
			warningsHighlighter.disable();
		} catch (CoreException e1) {
			e1.printStackTrace();
		}
		removeHighlight();
		highlighter = new TaintFlowHighlighter(flow);
		IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
		addListener();
		//highlight current active editor if TaintFlowView is active
		if (window != null) {
			IViewPart taintFlowView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("StaticAnalysisWarnings.view2");
			if (isActiveView(taintFlowView)){
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
	}
	
	/**
	 * Adds listener to the editors, so they are highlighted when opened. This listener also
	 * checks if another view activated and removes the highlight if so
	 */
	private void addListener(){
		if (listener == null) listener = new IPartListener(){
			@Override
			public void partOpened(IWorkbenchPart part) {
				if (part instanceof IEditorPart){
					highlightPart((IEditorPart)part);
				}
			}			
			@Override
			public void partActivated(IWorkbenchPart part) {
				if (part instanceof IEditorPart){
					highlightPart((IEditorPart)part);
				}
				if (part instanceof IViewPart){
					IViewPart taintFlowView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("StaticAnalysisWarnings.view2");
					if (part == taintFlowView) highlight();
					if (part != taintFlowView) removeHighlight();
				}
			}
			@Override
			public void partDeactivated(IWorkbenchPart part) {}	
			@Override
			public void partBroughtToTop(IWorkbenchPart part) {}
			@Override
			public void partClosed(IWorkbenchPart part) {}			
		};
		getSite().getWorkbenchWindow().getPartService().addPartListener(listener);
	}
	
	/**
	 * Highlights the editor if this view is active
	 * 
	 * @param part  IEditorPart to highlight
	 */
	private void highlightPart(IEditorPart part){
		IViewPart taintFlowView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("StaticAnalysisWarnings.view2");
		if (isActiveView(taintFlowView)){
			try {
				warningsHighlighter.disable();
				if (highlighter != null) highlighter.highlightEditor((IEditorPart)part);
			} catch (CoreException | BadLocationException e) {
				e.printStackTrace();
			}	
		}
	}
	
	/**
	 * Checks whether the given view is active
	 * 
	 * @param view		view to check
	 * @return			true if the view is active
	 */
	private boolean isActiveView(IViewPart view) {
		IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
	    IWorkbenchPart activeView = window.getActivePage().getActivePart();
	    if (activeView == null) return false;
	    else return activeView.equals(view);
	}

	
	/**
	 * Creates a TreeViewer that will show the taint flow information
	 * 
	 * @param parent		the parent control
	 * @see					TreeViewer
	 */
	private void createViewer(Composite parent){
		Composite composite = new Composite(parent, SWT.NONE);
		composite.setLayoutData(new GridData(SWT.FILL, SWT.FILL, true, true, 1, 1));
		
		//Add TableColumnLayout
		TreeColumnLayout layout = new TreeColumnLayout();
		composite.setLayout(layout);
		viewer = new TreeViewer(composite, SWT.MULTI | SWT.H_SCROLL | SWT.V_SCROLL |SWT.FULL_SELECTION);
		
		Tree tree = viewer.getTree();
		tree.setHeaderVisible(true);
		tree.setLinesVisible(false);
		
		TreeViewerColumn treeViewerColumn_1 = new TreeViewerColumn(viewer, SWT.NONE);
		TreeColumn positionColumn = treeViewerColumn_1.getColumn();
		layout.setColumnData(positionColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		positionColumn.setText("Position");
		treeViewerColumn_1.setLabelProvider(new LabelProviderCol1_TaintFlowView());
		
		TreeViewerColumn treeViewerColumn_2 = new TreeViewerColumn(viewer, SWT.NONE);
		TreeColumn previewColumn = treeViewerColumn_2.getColumn();
		layout.setColumnData(previewColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		previewColumn.setText("Preview");
		treeViewerColumn_2.setLabelProvider(new DecoratingStyledCellLabelProvider(
				new LabelProviderCol2_TaintFlowView(), null, null));
		
		
		viewer.setContentProvider(new ContentProvider_TaintFlowView());
		
		viewer.addDoubleClickListener(new IDoubleClickListener(){
			@Override
			public void doubleClick(DoubleClickEvent event) {
				IStructuredSelection thisSelection = (IStructuredSelection) event.getSelection(); 
				Object selectedRow = thisSelection.getFirstElement();
				if (selectedRow instanceof FlowPoint){
					Point p = ((FlowPoint)selectedRow).point;
					TextSelector.showText(p.filePath, p.firstLine, p.lastLine, p.firstCol, p.lastCol);
				}
			}
			
		});
	}
	
	@Override
	public Image getTitleImage() {
	    return general.IconProvider.getViewIcon().createImage();
	}

}