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

import representation.highlevel.ProgramPoint;
import callstacks.Call;
import views.contentproviders.ContentProvider_DeadCodeView;
import views.labelproviders.LabelProvider_DeadCodeView;
import general.TextSelector;

class Creator_DeadCodeView {
	/**
	 * Creates the TreeViewer to be shown in the DeadCodeView.
	 * 
	 * @param parent	the parent control
	 * @return			TreeViewer that has been created
	 */
	static TreeViewer createView(Composite parent){		
		
		//Create the composite
		Composite composite = new Composite(parent, SWT.NONE);
		composite.setLayoutData(new GridData(SWT.FILL, SWT.FILL, true, true, 1, 1));
		
		//Add TableColumnLayout
		TreeColumnLayout layout = new TreeColumnLayout();
		composite.setLayout(layout);
		TreeViewer viewer = new TreeViewer(composite, SWT.MULTI | SWT.H_SCROLL | SWT.V_SCROLL);
		
		Tree tree = viewer.getTree();
		tree.setHeaderVisible(true);
		tree.setLinesVisible(false);
		
		TreeViewerColumn treeViewerColumn_1 = new TreeViewerColumn(viewer, SWT.NONE);
		TreeColumn descriptionColumn = treeViewerColumn_1.getColumn();
		layout.setColumnData(descriptionColumn, new ColumnWeightData(3, ColumnWeightData.MINIMUM_WIDTH, true));
		descriptionColumn.setText("Resource");
		
		TreeViewerColumn treeViewerColumn_2 = new TreeViewerColumn(viewer, SWT.NONE);
		TreeColumn resourceColumn = treeViewerColumn_2.getColumn();
		layout.setColumnData(resourceColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		resourceColumn.setText("Line number");

		viewer.setContentProvider(new ContentProvider_DeadCodeView());
		viewer.setLabelProvider(new LabelProvider_DeadCodeView());
				
		addListener(viewer);	
		return viewer;

	}
	
	/**
	 * Adds an IDoubleClickListener to a TreeViewer that opens an editor with
	 * selected dead code or call and highlights the corresponding line
	 * 	
	 * @param viewer		TableViewer the listener is supposed to be added to
	 * @see TableViewer
	 * @see IDoubleClickListener
	 */
	private static void addListener(TreeViewer viewer){
		viewer.addDoubleClickListener(new IDoubleClickListener(){
			public void doubleClick(DoubleClickEvent event) {
				IStructuredSelection thisSelection = (IStructuredSelection) event.getSelection(); 
				Object selectedRow = thisSelection.getFirstElement();
				if (selectedRow instanceof ProgramPoint){
					ProgramPoint pPoint = (ProgramPoint)selectedRow;
					TextSelector.showTextLines(pPoint.parentFile.filePath, pPoint.point.firstLine, pPoint.point.lastLine);
				}
				if (selectedRow instanceof Call){
					Call call = (Call)selectedRow;
					TextSelector.showText(call.filePath,call.firstLine,call.lastLine,call.firstCol,call.lastCol);	
				}
				
			}

	});	
	}
}