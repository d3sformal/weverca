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

import metrics.SelectionHandler;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.IStatus;
import org.eclipse.core.runtime.Status;
import org.eclipse.core.runtime.jobs.Job;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.jface.viewers.TableViewer;
import org.eclipse.swt.graphics.Image;
import org.eclipse.swt.layout.GridLayout;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Display;
import org.eclipse.ui.ISelectionListener;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.IWorkbenchPart;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.part.ViewPart;

import representation.MetricsParser;
import exceptions.AnalyzerNotFoundException;
import general.Messages;

/**
 * View for displaying aggregated metric information of selected files and folders.
 * ViewAggregated takes files and folders that were selected in either PHP explorer
 * or Project explorer and shows aggregated metric information about all included PHP files. 
 * It also gives all the metric information to ViewSimple if this is open.
 * 
 * @author Natalia Tyrpakova
 * @see ViewSimple
 */
public class ViewAggregated extends ViewPart {

	private TableViewer viewer;
	private ISelectionListener listener = new ISelectionListener() { //PHP and Project explorer listener
		public void selectionChanged(IWorkbenchPart sourcepart, ISelection selection) {
			ArrayList<String> paths = SelectionHandler.getPathsFromSelection(selection);
			
			//call Weverca analyzer in separate thread
			final MetricsParser weverca = new MetricsParser(paths);
			//final long startTime = System.currentTimeMillis();
			
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
									viewer.setInput(weverca.GetAggregatedRatingAndQuantity());
									//if ViewSimple exists, give it all the information
									IViewPart findView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("ratingandquantity.view2");
									if (findView != null) {
										((ViewSimple)findView).SetInputFromAggregated(weverca);
									}
									//final long endTime = System.currentTimeMillis();
									//long elapsedTime = endTime-startTime;
									//Messages.metricsTime(weverca.analysisTime,elapsedTime);
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
	};
	
	
	
	
	/**
	 * {@inheritDoc}
	 * 
	 * In this implementation, a TableViewer is created inside of this view.
	 */
	@Override
	public void createPartControl(Composite parent) {
		GridLayout layout = new GridLayout(2, false);
	    parent.setLayout(layout);
	    createViewer(parent);
	}
	
	/**
	 * creates the TableViewer for this view and adds a selection listener to
	 * Project Explorer and PHP Explorer to get the current selection
	 * 
	 * @param parent the parent control
	 * @see TableViewer
	 */
	private void createViewer(Composite parent) {
		
		viewer = Creator_ViewAggregated.createView(parent);
		
		getSite().setSelectionProvider(viewer);
		
		getSite().getWorkbenchWindow().getSelectionService().addSelectionListener("org.eclipse.ui.navigator.ProjectExplorer",listener);
		getSite().getWorkbenchWindow().getSelectionService().addSelectionListener("org.eclipse.php.ui.explorer",listener);
	
	}
	
	/**
	 * {@inheritDoc}
	 */
	@Override
	public void setFocus() {
		viewer.getControl().setFocus();
	}
	
	@Override
	public Image getTitleImage() {
	    return general.IconProvider.getViewIcon().createImage();
	}
	
	/**
	 * {@inheritDoc}
	 * 
	 * In this implementation the selection listeners are removed.
	 */
	public void dispose() {
		getSite().getWorkbenchWindow().getSelectionService().removeSelectionListener("org.eclipse.ui.navigator.ProjectExplorer",listener);
		getSite().getWorkbenchWindow().getSelectionService().removeSelectionListener("org.eclipse.php.ui.explorer",listener);
		super.dispose();
	}

}