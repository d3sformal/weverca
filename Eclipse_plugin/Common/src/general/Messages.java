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


package general;

import java.util.ArrayList;

import org.eclipse.jface.viewers.ArrayContentProvider;
import org.eclipse.jface.viewers.LabelProvider;
import org.eclipse.swt.SWT;
import org.eclipse.swt.widgets.MessageBox;
import org.eclipse.swt.widgets.Shell;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.dialogs.ListDialog;

/**
 * This class provides various messages with warnings
 * 
 * @author Natalia Tyrpakova
 *
 */
public class Messages {
	
	public static final String INCORRECT_ANALYZER_PATH_WARNING = "Path to the Weverca analyzer is incorrect. Please set the correct path in the Weverca Analyzer preferecne page and perform the operation again.";
	
	
	/**
	 * Shows a MessageBox with warning that no file has been selected.
	 * 
	 * @see MessageBox
	 */
	public static void noFileSelected(){
		Shell shell = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getShell();
		MessageBox dialog = new MessageBox(shell, SWT.ICON_ERROR | SWT.OK| SWT.CANCEL);
		dialog.setText("Analysis failed");
		dialog.setMessage("No file is selected");
		dialog.open();
	}
	
	/**
	 * Shows a MessageBox with warning that no PHP file has been selected.
	 * 
	 * @see MessageBox
	 */
	public static void noPHPFileSelected(){
		Shell shell = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getShell();
		MessageBox dialog = new MessageBox(shell, SWT.ICON_ERROR| SWT.OK| SWT.CANCEL);
		dialog.setText("Analysis failed");
		dialog.setMessage("No PHP file is selected");
		dialog.open();
	}
	
	/**
	 * Shows a MessageBox with warning that the resource must be saved.
	 * 
	 * @see MessageBox
	 */
	public static void dirtyEditor(){
		Shell shell = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getShell();
		MessageBox dialog = new MessageBox(shell, SWT.ICON_INFORMATION | SWT.OK);
		dialog.setText("Unsaved resource");
		dialog.setMessage("The resource must be saved to perform analysis.");
		dialog.open();
	}
	
	/**
	 * Shows a MessageBox with warning that static analysis has failed ona specific file.
	 * 
	 * @param file	path of the file that caused the failure
	 */
	public static void staticAnalysisFailed(String file){
		// TODO: this does not works because getActiveWorkbenchWindow returns null if called from non-UI thread.
		Shell shell = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getShell();
		MessageBox dialog = new MessageBox(shell, SWT.ICON_ERROR | SWT.OK| SWT.CANCEL);
		dialog.setText("Analysis failed");
		String message = "Static analysis failed";
		if (!file.isEmpty()) message = message + " on file " + file;
		dialog.setMessage(message);
		dialog.open();
	}
	
	/**
	 * Shows a MessageBox with warning that the path to analyzer is incorrect.
	 * 
	 * @see MessageBox
	 */
	public static void incorrectAnalyzerPath(){
		Shell shell = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getShell();
		MessageBox dialog = new MessageBox(shell, SWT.ICON_ERROR | SWT.OK| SWT.CANCEL);
		dialog.setText("Incorrect path");
		dialog.setMessage(INCORRECT_ANALYZER_PATH_WARNING);
		dialog.open();
	}
	
	/*public static void metricsTime(long analysisTime, long overallTime){
		Shell shell = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getShell();
		MessageBox dialog = new MessageBox(shell, SWT.ICON_INFORMATION | SWT.OK| SWT.CANCEL);
		dialog.setText("Performance");
		dialog.setMessage("Weverca analyzer time: " + General.toTime(analysisTime) + "\n" +
				 "Overall time: " + General.toTime(overallTime));
		dialog.open();
	}*/
	
	/**
	 * Shows a dialog box with all the files that were not successfully analyzed
	 * 
	 * @param files		non-analyzed files
	 */
	public static void nonProcessedFiles(ArrayList<String> files){
		if (files.isEmpty()) return;
		Shell shell = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getShell();
		
		ListDialog.setDialogHelpAvailable(false);
		ListDialog listDialog = new ListDialog(shell);
		listDialog.setWidthInChars(100);

		listDialog.setTitle("Non-analyzed files");
		listDialog.setMessage("Files that were not successfully analyzed");
		listDialog.setContentProvider(new ArrayContentProvider());
		listDialog.setLabelProvider(new LabelProvider());
		listDialog.setInput(files);
		
		listDialog.open();
		
	}
	
}