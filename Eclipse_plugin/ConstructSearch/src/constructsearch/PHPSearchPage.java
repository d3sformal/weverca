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


package constructsearch;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Iterator;

import org.eclipse.core.resources.IResource;
import org.eclipse.core.resources.ResourcesPlugin;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.core.runtime.IAdaptable;
import org.eclipse.jface.dialogs.DialogPage;
import org.eclipse.jface.viewers.CheckboxTableViewer;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.jface.viewers.IStructuredSelection;
import org.eclipse.search.ui.ISearchPage;
import org.eclipse.search.ui.ISearchPageContainer;
import org.eclipse.swt.SWT;
import org.eclipse.swt.layout.GridData;
import org.eclipse.swt.layout.GridLayout;
import org.eclipse.swt.layout.RowLayout;
import org.eclipse.swt.widgets.Button;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Group;
import org.eclipse.swt.widgets.Label;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.IWorkbenchWindow;
import org.eclipse.ui.PlatformUI;

import representation.ConstructsInfo;
import representation.ConstructsParser;
import views.SearchResultView;

/**
 * Contribution to a search page extension that provides search for the selected PHP constructs. 
 * 
 * @author Natalia Tyrpakova
 * @see		ISearchPage
 *
 */
public class PHPSearchPage extends DialogPage implements ISearchPage {
	private Button WorkspaceButton;
	private Button SelectedResourceButton;
	private CheckboxTableViewer viewer1;
	private CheckboxTableViewer viewer2;
	private CheckboxTableViewer viewer3; 
	private ArrayList<String> selected = new ArrayList<String>();
	private static String[] GENERALCOSTRUCTTYPES = new String[] {"SQL","Sessions","Autoload","Magic methods","Class presence","Aliasing","Inside function declaration","Use of super global variable"};
	private static String[] DYNAMICCONSTRUCTTYPES = new String[] {"Dynamic call","Dynamic dereference","Dynamic include","Eval"};
	private static String[] PROBLEMATICCONSTRUCTTYPES = new String[] {"Passing by reference at call side"};
	
	/**
	 * {@inheritDoc}	
	 * In this implementation Runner is called to search the selected files for PHP construct occurrences
	 * and the result is pushed to the SearchResultView.
	 * 
	 * @see		ConstructsParser
	 * @see		SearchResultView
	 */
	public boolean performAction() {
		int i;	
 		//Get General constructs
 		for (i=0 ; i < GENERALCOSTRUCTTYPES.length;i++) {
 			if (viewer1.getChecked(GENERALCOSTRUCTTYPES[i])) {
 				String name = GENERALCOSTRUCTTYPES[i];
 				selected.add(name);
 			}
 		}
 		//Get Dynamic constructs
 		for (i=0 ; i < DYNAMICCONSTRUCTTYPES.length;i++) {
 			if (viewer2.getChecked(DYNAMICCONSTRUCTTYPES[i])) {
 				selected.add(DYNAMICCONSTRUCTTYPES[i]);
 			}
 		}
 		//Get Problematic constructs
 		for (i=0 ; i < PROBLEMATICCONSTRUCTTYPES.length;i++) {
 			if (viewer3.getChecked(PROBLEMATICCONSTRUCTTYPES[i])) {
 				selected.add(PROBLEMATICCONSTRUCTTYPES[i]);
 			}
 		}
 		//Get selected resources
 		 ArrayList<String> paths = new ArrayList<String>();
 		if (WorkspaceButton.getSelection()) {
 			paths.add(ResourcesPlugin.getWorkspace().getRoot().getLocation().toFile().toString());
 		}
 		else if (SelectedResourceButton.getSelection()) {
 			IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
 			ISelection selection = window.getSelectionService().getSelection("org.eclipse.php.ui.explorer");
 		    if (selection == null) selection = window.getSelectionService().getSelection("org.eclipse.ui.navigator.ProjectExplorer");
 			
 		   IStructuredSelection structured = (IStructuredSelection)selection;
 		   Iterator it = structured.iterator();
 		   while (it.hasNext()){
				Object obj = it.next();
				IResource resource = null;
				if (obj instanceof IResource){
					resource = (IResource)obj;
				}
				else if (obj instanceof IAdaptable){
					IAdaptable ad = (IAdaptable)obj;
					resource = (IResource)ad.getAdapter(IResource.class);
				} else {
					return true;
				}
				if (resource.getRawLocation() != null) paths.add(resource.getLocation().toString());
			}
 		}	
 		//Call weverca
 		ConstructsInfo result = new ConstructsInfo();
 		ConstructsParser weverca = new ConstructsParser();
 		try {
			result = weverca.GetConstructOccurances(paths,selected);
		} catch (IOException e) {
			return true;
		}
 		
 		//Show result view
 		try {
 			if (result != null){
 				IViewPart ResultView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().showView("ConstructSearch.view1");
 				((SearchResultView)ResultView).setInput(result,paths);
 			}
 		} catch (CoreException e) {
			return true;
		}

        return true;
	}

	public void setContainer(ISearchPageContainer container) {
	}

	@Override
	public void createControl(Composite parent) {
		GridLayout layout = new GridLayout(1,true);
		layout.horizontalSpacing = 5;
		layout.verticalSpacing = 5;
		parent.setLayout(layout);
			    
	    Label generalconstlabel = new Label(parent, SWT.CENTER);
	    generalconstlabel.setText("General constructs");    
	    viewer1 = CheckboxTableViewer.newCheckList(parent, SWT.BORDER | SWT.MULTI | SWT.FULL_SELECTION | SWT.V_SCROLL);
	    viewer1.add(GENERALCOSTRUCTTYPES);
	    viewer1.getControl().setLayoutData(new GridData(GridData.FILL_HORIZONTAL)); 
	    
	    Label dynamicconstlabel = new Label(parent, SWT.CENTER);
	    dynamicconstlabel.setText("Dynamic constructs");    
	    viewer2 = CheckboxTableViewer.newCheckList(parent, SWT.BORDER | SWT.MULTI | SWT.FULL_SELECTION | SWT.V_SCROLL);
	    viewer2.add(DYNAMICCONSTRUCTTYPES);
	    viewer2.getControl().setLayoutData(new GridData(GridData.FILL_HORIZONTAL));
	    
	    Label problemconstlabel = new Label(parent, SWT.CENTER );
	    problemconstlabel.setText("Problematic constructs");	    
	    viewer3 = CheckboxTableViewer.newCheckList(parent, SWT.BORDER | SWT.MULTI | SWT.FULL_SELECTION | SWT.V_SCROLL);
	    viewer3.add(PROBLEMATICCONSTRUCTTYPES);
	    viewer3.getControl().setLayoutData(new GridData(GridData.FILL_HORIZONTAL));
	    
	    Group scope = new Group(parent, SWT.SHADOW_IN);
	    scope.setText("Scope");
	    scope.setLayout(new RowLayout(SWT.VERTICAL));
	    WorkspaceButton = new Button(scope,SWT.RADIO);
	    WorkspaceButton.setText("Workspace");
	    WorkspaceButton.setSelection(true);
	    SelectedResourceButton = new Button(scope,SWT.RADIO);
	    SelectedResourceButton.setText("Selected resource");
	    SelectedResourceButton.setEnabled(false);
	    IWorkbenchWindow window = PlatformUI.getWorkbench().getActiveWorkbenchWindow();
		
	    ISelection selection_phpexplorer = window.getSelectionService().getSelection("org.eclipse.php.ui.explorer");
	    ISelection selection_projectecxplorer = window.getSelectionService().getSelection("org.eclipse.ui.navigator.ProjectExplorer");
	    
	    if (selection_phpexplorer != null || selection_projectecxplorer != null) {
	    	 SelectedResourceButton.setEnabled(true);
	    } 
		
		setControl(parent);
	}
	
	@Override
	public void setVisible(boolean visible) {
		super.setVisible(visible);
	}
	
	
}