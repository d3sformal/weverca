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


package staticanalysiswarnings;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import org.eclipse.core.runtime.CoreException;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.PlatformUI;

import representation.Warning;
import representation.WarningsParser;
import views.WarningsView;
import wevercarunner.StaticAnalysisParser;

/**
 * Displays the warnings.
 * 
 * This class is accessed from the StaticAnalysis plug-in
 * 
 * @author Natalia Tyrpakova
 * @author David Hauzar
 *
 */
public class AnalysisWarningsDisplayer {
	/**
	 * Shows the result in the WarningsView.
	 * 
	 * @param warnings		the list of warnings to display.
	 */
	public static void displayWarnings(ArrayList<Warning> warnings){
		try {
			IViewPart ResultView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().showView("StaticAnalysisWarnings.view1");
			((WarningsView)ResultView).setInput(warnings);
			if (warnings.size() == 0) ((WarningsView)ResultView).setInput("No warnings found");
		} catch (CoreException e) {
			e.printStackTrace();
		}	
	}
	
	
}