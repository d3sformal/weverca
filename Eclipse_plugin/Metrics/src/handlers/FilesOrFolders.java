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

import java.io.IOException;

import org.eclipse.core.commands.AbstractHandler;
import org.eclipse.core.commands.ExecutionEvent;
import org.eclipse.core.commands.ExecutionException;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.handlers.HandlerUtil;
import org.eclipse.ui.handlers.RadioState;

import views.ViewSimple;

/**
 * Handler for the command in ViewSimple that determines whether to show files 
 * or folders. 
 * @author 		Natalia Tyrpakova
 * @see 		AbstractHandler
 */

public class FilesOrFolders extends AbstractHandler {

	/** 
	 * {@inheritDoc}
	 * This implementation sends an information to View Simple when files/folders
	 * option is changed.
	 *
	 * @see 			ExecutionEvent
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

		boolean files = true;
		boolean folders = true;
		if (currentState.equals("files")) folders = false;
		else if (currentState.equals("folders")) files = false;
	
		IViewPart findView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("ratingandquantity.view2");
		if (findView != null) {
			try {
				((ViewSimple)findView).setOutputType(files,folders);
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	
		HandlerUtil.updateRadioState(event.getCommand(), currentState);
		return null;
	}

}