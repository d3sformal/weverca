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

import java.io.IOException;
import java.util.ArrayList;

import metrics.MetricInfoComparator;
import metrics.SelectionHandler;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.IStatus;
import org.eclipse.core.runtime.Status;
import org.eclipse.core.runtime.jobs.Job;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.jface.viewers.TableViewer;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.graphics.Image;
import org.eclipse.swt.layout.GridLayout;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Display;
import org.eclipse.swt.widgets.TableColumn;
import org.eclipse.ui.ISelectionListener;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.IWorkbenchPart;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.part.ViewPart;

import representation.MetricsParser;
import exceptions.AnalyzerNotFoundException;
import general.Messages;

/**

 * View for displaying metric information of selected php files and folders. This view
 * gets the metric information from ViewAggregated, or listens to changes in Project
 * Explorer and PHP Explorer and directly calls Weverca_Runner if ViewAggregated is not open.
 * 
 * @author Natalia Tyrpakova
 * @see ViewAggregated
 * @see MetricsParser
 */
public class ViewSimple extends ViewPart {

	private TableViewer viewer;
	private static MetricInfoComparator comparator;
	private boolean showFiles;
	private boolean showFolders;
	private MetricsParser weverca;
	private boolean recursion;
	
	private ISelectionListener listener = new ISelectionListener() {
		public void selectionChanged(IWorkbenchPart sourcepart, ISelection selection) {
			IViewPart findView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("ratingandquantity.view1");
			if (findView == null) {
				ArrayList<String> paths = SelectionHandler.getPathsFromSelection(selection);

				//call Weverca analyzer in separate thread
				weverca = new MetricsParser(paths);
				Job job = new Job("calling analyzer"){
					protected IStatus run(IProgressMonitor monitor) {
						try {
							weverca.run();
						} catch (AnalyzerNotFoundException e1) {
							return new Status(Status.ERROR,"Metrics",Messages.INCORRECT_ANALYZER_PATH_WARNING);
						}
						Display.getDefault().asyncExec(new Runnable() {
						      public void run() {
						    	  try {
										viewer.setInput(weverca.GetRatingAndQuantity(showFiles,showFolders,recursion));
										Messages.nonProcessedFiles(weverca.nonProcessedFiles);
						    	  } catch (IOException e) {
										e.printStackTrace();
									}
						      }
						    });
						return Status.OK_STATUS;
					}
				};
				job.schedule();	
			}	
		}
	};
	
	/**
	 * The constructor initializes the optional values - both files and folders are
	 * showed in the view and folders are searched recursively.
	 */	
	public ViewSimple() {
		showFiles = true;
		showFolders = true;
		recursion = true;
	}
	
	/**
	 * This method sets the recursion value and updates the data showed in the view
	 *
	 * @param rec indicator whether folders should be searched recursively
	 * @throws IOException
	 */
	public void setRecursion(boolean rec) throws IOException{
		if (rec) recursion = true;
		else recursion = false;
		viewer.setInput(weverca.GetRatingAndQuantity(showFiles,showFolders,recursion));
	}
	
	/**
	 * This method sets whether files or folders should be shown in the view
	 * and updates the data showed.
	 * 
	 * @param file		true if files should be shown
	 * @param folder	true if folders should be shown
	 * @throws IOException
	 */
	public void setOutputType(boolean file,boolean folder) throws IOException{
		if (file != showFiles || folder != showFolders){
			showFiles = file;
			showFolders = folder;
			viewer.setInput(weverca.GetRatingAndQuantity(showFiles,showFolders,recursion));
		}
	}

	/**
	 * {@inheritDoc}
	 */
	@Override
	public void createPartControl(Composite parent) {
		GridLayout layout = new GridLayout(2, false);
	    parent.setLayout(layout);
	    createViewer(parent);
	    comparator = new MetricInfoComparator();
	    viewer.setComparator(comparator);
	}
	
	/**
	 * Gets data for the view and sets the TableViewer input
	 * 
	 * @param weverca		the Weverca_Runner which provides the metric information
	 * @throws IOException
	 * @see MetricsParser
	 */
	void SetInputFromAggregated(MetricsParser weverca) throws IOException {
		this.weverca = weverca;
		viewer.setInput(weverca.GetRatingAndQuantity(showFiles,showFolders,recursion));
	}
	
	/**
	 * creates the TableViewer for this view and adds a selection listener to
	 * Project Explorer and PHP Explorer to get the current selection in case View Aggregated
	 * is not open. It also adds a sorter to each table column.
	 * 
	 * @param parent the parent control
	 * @see TableViewer
	 */
	private void createViewer(Composite parent) {
		viewer = Creator_ViewSimple.createView(parent);
		
		getSite().setSelectionProvider(viewer);
				
		getSite().getWorkbenchWindow().getSelectionService().addSelectionListener("org.eclipse.ui.navigator.ProjectExplorer",listener);
		getSite().getWorkbenchWindow().getSelectionService().addSelectionListener("org.eclipse.php.ui.explorer",listener);
		
		int colIndex = 0;
		for (TableColumn column : viewer.getTable().getColumns()){
			column.addSelectionListener(getSelectionAdapter(column, colIndex,  viewer));
			colIndex++;
		}
	}
	
	
	/**
	 * This method sorts the TableViewer by specified column using the MetricInfoComparator.
	 * 
	 * @param column  	a column to be sorted by
	 * @param index		zero-based index of a column to be sorted by
	 * @param viewer	TableViewer which is to be sorted
	 * @return			SelectionAdapter
	 * @see				TableViewer
	 * @see				MetricInfoComparator
	 */
	private SelectionAdapter getSelectionAdapter(final TableColumn column,final int index, final TableViewer viewer) {
	    SelectionAdapter selectionAdapter = new SelectionAdapter() {
	      @Override
	      public void widgetSelected(SelectionEvent e) {
	        comparator.setColumn(index);
	        int dir = comparator.getDirection();
	        viewer.getTable().setSortDirection(dir);
	        viewer.getTable().setSortColumn(column);
	    
	        viewer.refresh();
	      }
	    };
	    return selectionAdapter;
	  }
		

	/**
	 * {@inheritDoc}
	 */
	@Override
	public void setFocus() {
		viewer.getControl().setFocus();
	}
	/**
	 * {@inheritDoc}
	 * 
	 * In this implementation, the selection listener is removed.
	 */
	public void dispose() {
		getSite().getWorkbenchWindow().getSelectionService().removeSelectionListener(listener);
		super.dispose();
	}
	
	@Override
	public Image getTitleImage() {
	    return general.IconProvider.getViewIcon().createImage();
	}
	

}