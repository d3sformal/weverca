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

import org.eclipse.jface.layout.TreeColumnLayout;
import org.eclipse.jface.viewers.ColumnWeightData;
import org.eclipse.jface.viewers.DoubleClickEvent;
import org.eclipse.jface.viewers.IDoubleClickListener;
import org.eclipse.jface.viewers.IStructuredSelection;
import org.eclipse.jface.viewers.TableViewer;
import org.eclipse.jface.viewers.TreeViewer;
import org.eclipse.jface.viewers.TreeViewerColumn;
import org.eclipse.swt.SWT;
import org.eclipse.swt.layout.GridData;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Tree;
import org.eclipse.swt.widgets.TreeColumn;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.PartInitException;
import org.eclipse.ui.PlatformUI;

import callstacks.Call;
import representation.FlowString;
import representation.Warning;
import representation.points.FlowPoint;
import staticanalysiswarnings.WarningsHighlighter;
import views.contentproviders.ContentProvider_WarningsView;
import views.labelproviders.LabelProvider_WarningsView;
import general.TextSelector;

/**
 * This class creates the TreeViewer that is shown in WarningsView.
 * 
 * @author 	Natalia Tyrpakova
 * @see 	TreeViewer
 */
class Creator_WarningsView {
	
	/**
	 * Creates the TreeViewer with columns to be shown in the WarningsView.
	 * 
	 * @param parent	the parent control
	 * @return			TreeViewer that has been created
	 */
	static TreeViewer createView(Composite parent, WarningsHighlighter highlighter){
			
		//Create the composite
		Composite composite = new Composite(parent, SWT.NONE);
		composite.setLayoutData(new GridData(SWT.FILL, SWT.FILL, true, true, 1, 1));
		
		//Add TableColumnLayout
		TreeColumnLayout layout = new TreeColumnLayout();
		composite.setLayout(layout);
		TreeViewer viewer = new TreeViewer(composite, SWT.MULTI | SWT.H_SCROLL | SWT.V_SCROLL);
		
		Tree tree = viewer.getTree();
		tree.setHeaderVisible(true);
		tree.setLinesVisible(true);
		
		//create columns
		TreeViewerColumn treeViewerColumn_1 = new TreeViewerColumn(viewer, SWT.NONE);
		TreeColumn descriptionColumn = treeViewerColumn_1.getColumn();
		layout.setColumnData(descriptionColumn, new ColumnWeightData(5, ColumnWeightData.MINIMUM_WIDTH, true));
		descriptionColumn.setText("Description");
		
		TreeViewerColumn treeViewerColumn_2 = new TreeViewerColumn(viewer, SWT.NONE);
		TreeColumn resourceColumn = treeViewerColumn_2.getColumn();
		layout.setColumnData(resourceColumn, new ColumnWeightData(7, ColumnWeightData.MINIMUM_WIDTH, true));
		resourceColumn.setText("Resource/Taint Flow");
		
		TreeViewerColumn treeViewerColumn_3 = new TreeViewerColumn(viewer, SWT.NONE);
		TreeColumn lineColumn = treeViewerColumn_3.getColumn();
		layout.setColumnData(lineColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		lineColumn.setText("Line");
		
		TreeViewerColumn treeViewerColumn_4 = new TreeViewerColumn(viewer, SWT.NONE);
		TreeColumn priorityColumn = treeViewerColumn_4.getColumn();
		priorityColumn.setToolTipText("The priority is high only for security warnings with all possible flows tainted");
		layout.setColumnData(priorityColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		priorityColumn.setText("Priority");

		viewer.setContentProvider(new ContentProvider_WarningsView());
		viewer.setLabelProvider(new LabelProvider_WarningsView());
				
		addListener(viewer, highlighter);	
		return viewer;

	}
	
	/**
	 * Adds an IDoubleClickListener to a TableViewer that opens an editor with
	 * selected file and highlights the corresponding line, or, in case of TaintFlow, opens a dialog window
	 * with extended taint flow information
	 * 	
	 * @param viewer		TableViewer the listener is supposed to be added to
	 * @see TableViewer
	 * @see IDoubleClickListener
	 */
	private static void addListener(TreeViewer viewer, final WarningsHighlighter highlighter){
		viewer.addDoubleClickListener(new IDoubleClickListener(){
			public void doubleClick(DoubleClickEvent event) {
				IStructuredSelection thisSelection = (IStructuredSelection) event.getSelection(); 
				Object selectedRow = thisSelection.getFirstElement();
				if (selectedRow instanceof Warning){
					Warning w = (Warning)selectedRow;
					TextSelector.showText(w.filePath, w.firstOffset, w.lastOffset);
				}
				if (selectedRow instanceof FlowString){
					showWarningInNewView((FlowString)selectedRow, highlighter);
				}
				if (selectedRow instanceof Call){
					Call call = (Call)selectedRow;
					TextSelector.showText(call.filePath,call.firstLine,call.lastLine,call.firstCol,call.lastCol);
				}
				
			}

	});	
	}
	
	
	/**
	 * Opens a new view with taint flow information
	 * 
	 * @param flow		flow to show in a view
	 * @see				FlowPoint
	 */
	private static void showWarningInNewView(FlowString flow, WarningsHighlighter highlighter){
		ArrayList<Object> input = flow.getList();
		try {
			IViewPart resultView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("StaticAnalysisWarnings.view2");
			if (resultView == null) resultView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().showView("StaticAnalysisWarnings.view2");
			else PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().activate(resultView);
			((TaintFlowView)resultView).setInput(input, highlighter);
		} catch (PartInitException e) {
			e.printStackTrace();
		}
	}
	
}