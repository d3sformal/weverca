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

import org.eclipse.jface.layout.TableColumnLayout;
import org.eclipse.jface.viewers.ArrayContentProvider;
import org.eclipse.jface.viewers.ColumnWeightData;
import org.eclipse.jface.viewers.TableViewer;
import org.eclipse.jface.viewers.TableViewerColumn;
import org.eclipse.swt.SWT;
import org.eclipse.swt.layout.GridData;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Table;
import org.eclipse.swt.widgets.TableColumn;

import views.labelproviders.LabelProvider_ViewAggregated;

/**
 * This class creates the TableViewer that is shown in ViewAggregated.
 * Its only method is a static method that creates the TableViewer
 * 
 * @author Natalia Tyrpakova
 * @see TableViewer
 */
class Creator_ViewAggregated {

	/**
	 * Creates a TableViewer to show in ViewAggregated and defines its columns.
	 * 
	 * @param parent 	the parent control
	 * @return			created TableViewer
	 * @see TableViewer
	 */
	static TableViewer createView( Composite parent){
		//Create the composite
		Composite composite = new Composite(parent, SWT.NONE);
		composite.setLayoutData(new GridData(SWT.FILL, SWT.FILL, true, true, 1, 1));
				
		//Add TableColumnLayout
		TableColumnLayout layout = new TableColumnLayout();
		composite.setLayout(layout);

		TableViewer viewer = new TableViewer(composite, SWT.MULTI | SWT.H_SCROLL
				| SWT.V_SCROLL | SWT.FULL_SELECTION | SWT.BORDER);

		Table table = viewer.getTable();
		table.setHeaderVisible(true);
		table.setLinesVisible(true);
				
		//create columns
		TableViewerColumn tableViewerColumn_1 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn fileNameColumn = tableViewerColumn_1.getColumn();
		layout.setColumnData(fileNameColumn, new ColumnWeightData(3, ColumnWeightData.MINIMUM_WIDTH, true));
		fileNameColumn.setText("File name");

		TableViewerColumn tableViewerColumn_2 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn propertyColumn = tableViewerColumn_2.getColumn();
		layout.setColumnData(propertyColumn, new ColumnWeightData(3, ColumnWeightData.MINIMUM_WIDTH, true));
		propertyColumn.setText("Property");
				
		TableViewerColumn tableViewerColumn_3 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn valueColumn = tableViewerColumn_3.getColumn();
		layout.setColumnData(valueColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		valueColumn.setText("Value");
					
		viewer.setContentProvider(new ArrayContentProvider());
		viewer.setLabelProvider(new LabelProvider_ViewAggregated());
				
		return viewer;
	}
}