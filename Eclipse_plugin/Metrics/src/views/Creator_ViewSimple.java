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

import org.eclipse.core.resources.IFile;
import org.eclipse.core.resources.IWorkspace;
import org.eclipse.core.resources.ResourcesPlugin;
import org.eclipse.core.runtime.IPath;
import org.eclipse.core.runtime.Path;
import org.eclipse.jface.layout.TableColumnLayout;
import org.eclipse.jface.viewers.ArrayContentProvider;
import org.eclipse.jface.viewers.ColumnWeightData;
import org.eclipse.jface.viewers.DoubleClickEvent;
import org.eclipse.jface.viewers.IDoubleClickListener;
import org.eclipse.jface.viewers.IStructuredSelection;
import org.eclipse.jface.viewers.TableViewer;
import org.eclipse.jface.viewers.TableViewerColumn;
import org.eclipse.swt.SWT;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.layout.GridData;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Menu;
import org.eclipse.swt.widgets.MenuItem;
import org.eclipse.swt.widgets.Table;
import org.eclipse.swt.widgets.TableColumn;
import org.eclipse.ui.IWorkbenchPage;
import org.eclipse.ui.PartInitException;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.ide.IDE;

import representation.MetricInformation;
import views.labelproviders.LabelProvider_ViewSimple;


/**
 * This class creates the TableViewer that is shown in ViewSimple.
 * 
 * @author 	Natalia Tyrpakova
 * @see		TableViewer
 */
class Creator_ViewSimple {
	
	/**
	 * Creates a TableViewer to show in ViewAggregated and defines its columns.
	 * @param parent	the parent control
	 * @return			created TableViewer
	 * @see 			TableViewer
	 */
	static TableViewer createView(Composite parent){
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
				
		Menu contextMenu = new Menu(viewer.getTable());
		viewer.getTable().setMenu(contextMenu);
				
		//create columns
		TableViewerColumn tableViewerColumn_1 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn fileNameColumn = tableViewerColumn_1.getColumn();
		layout.setColumnData(fileNameColumn, new ColumnWeightData(2, ColumnWeightData.MINIMUM_WIDTH, true));
		fileNameColumn.setText("File name");
		
		TableViewerColumn tableViewerColumn_2 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn numberOfLinesColumn = tableViewerColumn_2.getColumn();
		layout.setColumnData(numberOfLinesColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		numberOfLinesColumn.setText("Number of lines");		
				
		TableViewerColumn tableViewerColumn_3 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn numberOfSourcesColumn = tableViewerColumn_3.getColumn();
		layout.setColumnData(numberOfSourcesColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		numberOfSourcesColumn.setText("Number of sources");
				
		TableViewerColumn tableViewerColumn_4 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn maxInheritanceColumn = tableViewerColumn_4.getColumn();
		layout.setColumnData(maxInheritanceColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		maxInheritanceColumn.setText("Maximum inheritance depth");
				
		TableViewerColumn tableViewerColumn_5 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn maxMethodOverridingColumn = tableViewerColumn_5.getColumn();
		layout.setColumnData(maxMethodOverridingColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		maxMethodOverridingColumn.setText("Maximum method overriding");
				
		TableViewerColumn tableViewerColumn_6 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn classCouplingColumn = tableViewerColumn_6.getColumn();
		layout.setColumnData(classCouplingColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		classCouplingColumn.setText("Class coupling");
				
		TableViewerColumn tableViewerColumn_7 = new TableViewerColumn(viewer, SWT.NONE);
		TableColumn phpCouplingColumn = tableViewerColumn_7.getColumn();
		layout.setColumnData(phpCouplingColumn, new ColumnWeightData(1, ColumnWeightData.MINIMUM_WIDTH, true));
		phpCouplingColumn.setText("PHP functions coupling");
				
				
		for (TableColumn tableColumn : viewer.getTable().getColumns()){
			if (!tableColumn.equals(fileNameColumn))  createMenuItem(contextMenu, tableColumn);
		}
					
		viewer.setContentProvider(new ArrayContentProvider());
		viewer.setLabelProvider(new LabelProvider_ViewSimple());
				
		addListener(viewer);	
		return viewer;
	}
	
	/**
	 * Adds an IDoubleClickListener to a TableViewer that opens an editor with
	 * selected file if the selected row is an instance of MetricInformation.
	 * 	
	 * @param viewer		TableViewer the listener is supposed to be added to
	 * @see TableViewer
	 * @see IDoubleClickListener
	 * @see MetricInformation
	 */
	private static void addListener(TableViewer viewer){
		viewer.addDoubleClickListener(new IDoubleClickListener(){
			public void doubleClick(DoubleClickEvent event) {
				IStructuredSelection thisSelection = (IStructuredSelection) event.getSelection(); 
				Object selectedRow = thisSelection.getFirstElement();
				if (selectedRow instanceof MetricInformation){
					File file = new File(((MetricInformation)selectedRow).fileName);
					if (file.exists() && file.isFile()){
						IWorkbenchPage page = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage();	
					    IWorkspace workspace= ResourcesPlugin.getWorkspace();    
					    IPath location= Path.fromOSString(file.getAbsolutePath()); 
					    IFile ifile= workspace.getRoot().getFileForLocation(location);
					    try {
						    IDE.openEditor(page, ifile);
							} catch (PartInitException e) {
									return;
								}
					}
				}
			}

	});	
	}
	
	/**
	 * Creates a menu item for a column that is used to choose which columns to show.
	 * If column is not selected, its width is set to zero.
	 * 
	 * @param parent	the parent control
	 * @param column	column for which the MenuItem is created
	 * @see MenuItem
	 */
	private static void createMenuItem(Menu parent, final TableColumn column) {
		  final MenuItem itemName = new MenuItem(parent, SWT.CHECK);
		  itemName.setText(column.getText());
		  itemName.setSelection(column.getResizable());
		  itemName.addSelectionListener(new SelectionAdapter() {
		      @Override
		      public void widgetSelected(SelectionEvent e) {
		      if (itemName.getSelection()) {
		        column.setWidth(150);
		        column.setResizable(true);
		      } else {
		        column.setWidth(0);
		        column.setResizable(false);
		      }
		    }
		  });
	}
	
	
}