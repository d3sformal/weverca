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


package handlers;

import org.eclipse.core.commands.AbstractHandler;
import org.eclipse.core.commands.ExecutionEvent;
import org.eclipse.core.commands.ExecutionException;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.handlers.HandlerUtil;
import org.eclipse.ui.handlers.RadioState;

import views.WarningsView;

/**
 * Handler for the flow representation menu in WarningsView. 
 *  
 * @author Natalia Tyrpakova
 *
 */
public class FlowRepresentation extends AbstractHandler{

	/**
	 * {@inheritDoc}
	 * In this implementation the desired merging option is determined and pushed to the WarningsView
	 * 
	 */
	@Override
	public Object execute(ExecutionEvent event) throws ExecutionException {
	 
		try{
			if(HandlerUtil.matchesRadioState(event))
				return null;
			}
			catch (ExecutionException e){
				return null;
			}
	    
		String currentState = event.getParameter(RadioState.PARAMETER_ID);

		boolean merged;
		boolean reversed;
		if (currentState.equals("separate")) {
			merged = false;
			reversed = false;
		}
		else if (currentState.equals("merged")) {
			merged = true;
			reversed = false;
		}
		else if (currentState.equals("reversed")) {
			merged = false;
			reversed = true;
		}
		else if (currentState.equals("mergedReversed")) {
			merged = true;
			reversed = true;
		}
		else return null;
	
		IViewPart findView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("StaticAnalysisWarnings.view1");
		if (findView != null) {
			((WarningsView)findView).setOutput(merged,reversed);
		}	
		
		HandlerUtil.updateRadioState(event.getCommand(), currentState);
		return null;
	}

}