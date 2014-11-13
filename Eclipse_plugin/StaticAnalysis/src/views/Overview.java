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

import org.eclipse.jface.viewers.ArrayContentProvider;
import org.eclipse.jface.viewers.TableViewer;
import org.eclipse.swt.SWT;
import org.eclipse.swt.graphics.Image;
import org.eclipse.swt.layout.GridData;
import org.eclipse.swt.layout.GridLayout;
import org.eclipse.swt.widgets.Button;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Table;
import org.eclipse.ui.part.ViewPart;

import general.General;

/**
 * View displayig the static analysis status, overview and providing a button to stop the static analysis
 * 
 * @author Natalia Tyrpakova
 */
public class Overview extends ViewPart {
	/**
	 * Button located in this view that allows to stop the static analysis
	 */
	public Button stopAnalysisButton;
	private TableViewer viewer;

	/**
	 * {@inheritDoc}
	 * 
	 * In this implementation a button and a table are created inside of this view
	 */
	@Override
	public void createPartControl(Composite parent) {	
		GridLayout layout = new GridLayout(1, false);
		parent.setLayout(layout);
		
		stopAnalysisButton =  new Button(parent, SWT.PUSH);
		stopAnalysisButton.setText("Stop Static Analysis");
		stopAnalysisButton.setVisible(true);
		stopAnalysisButton.setEnabled(false);
		
		viewer = new TableViewer(parent, SWT.MULTI | SWT.H_SCROLL
		        | SWT.V_SCROLL | SWT.FULL_SELECTION);
		
		GridData data = new GridData(SWT.FILL, SWT.TOP, true, false);
		
		Table table = viewer.getTable();
		table.setLayoutData(data); 
		table.setHeaderVisible(false);
		table.setLinesVisible(false);
		
		viewer.setContentProvider(new ArrayContentProvider());
		viewer.setLabelProvider(new LabelProvider_Overview());	
	}
	
	/**
	 * Sets the input of running analysis
	 */
	public void analysisRunning(){	
		stopAnalysisButton.setEnabled(true);
		ArrayList<String> input = new ArrayList<String>(); 
		input.add("Analysis is running");
		viewer.setInput(input);
	}
	
	/**
	 * sets the input of correctly exited analysis
	 */
	public void done(long elapsedTime, int warningsNum, int warningsFirstPhaseNum, int warningsSecondPhaseNum, long wevercaTime, long firstPhase, long secondPhase, int pPoints, int pLines){
		stopAnalysisButton.setEnabled(false);
		ArrayList<String> input = new ArrayList<String>(); 
		input.add("Analysis is done");
		input.add("Elapsed time: " + General.toTime(elapsedTime));
		input.add("First phase analysis time: " + General.toTime(firstPhase));
		input.add("Taint analysis time: " + General.toTime(secondPhase));
		input.add("Overall Weverca analyzer time: " + General.toTime(wevercaTime));
		input.add("Number of processed program points: " + pPoints);
		input.add("Number of processed lines: " + pLines);
		input.add("Total number of warnings: " + warningsNum);
		input.add("Number of warnings in the first phase: " + warningsFirstPhaseNum);
		input.add("Number of warnings in the second phase: " + warningsSecondPhaseNum);
		viewer.setInput(input);
	}
	
	/**
	 * sets the input of manually exited analysis
	 */
	public void stopped(long elapsedTime){
		stopAnalysisButton.setEnabled(false);
		ArrayList<String> input = new ArrayList<String>(); 
		input.add("Analysis was stopped");
		input.add("Elapsed time: " + General.toTime(elapsedTime));
		viewer.setInput(input);
	}
	
	@Override
	public Image getTitleImage() {
	    return general.IconProvider.getViewIcon().createImage();
	}

	@Override
	public void setFocus() {
	}
	
	

}