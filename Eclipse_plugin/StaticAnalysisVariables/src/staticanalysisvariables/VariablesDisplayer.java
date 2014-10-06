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


package staticanalysisvariables;

import java.util.ArrayList;

import org.eclipse.core.runtime.CoreException;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.PlatformUI;

import representation.VariablesParser;
import representation.highlevel.File;
import representation.highlevel.ProgramPoint;
import views.DeadCodeView;
import views.VariablesView;
import wevercarunner.StaticAnalysisParser;

/**
 * Displays variables.
 * 
 * This class is accessed from the StaticAnalysis plug-in after the static analysis had been called.
 * 
 * @author Natalia Tyrpakova
 * @author David Hauzar
 *
 */
public class VariablesDisplayer {
	/**
	 * Processes the static analysis result and pushes the parsed information 
	 * to the VariablesView and DeadCodeView
	 * 
	 * @param analysisResult		result of the static analysis from Weverca analyzer
	 */
	public static void displayVariables(ArrayList<File> variables, ArrayList<ProgramPoint> deadCode){
		try {
			//show variables in VariablesView
			IViewPart resultView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("StaticAnalysisVariables.view1");
			if (resultView == null) resultView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().showView("StaticAnalysisVariables.view1");
			PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().activate(resultView);
			((VariablesView)resultView).setInput(variables);
			//close existing DeadCodeView
			IViewPart existingView2 = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("StaticAnalysisVariables.view2");
			if (existingView2 != null) PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().hideView(existingView2);
			//if there is any dead code, show it in DeadCodeView
			if (!deadCode.isEmpty()){
				//IViewPart resultView2 = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().showView("StaticAnalysisVariables.view2");
				//((DeadCodeView)resultView2).setInput(deadCode);
			}
		} catch (CoreException e) {
			e.printStackTrace();
		}
	}
}