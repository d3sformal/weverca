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
import org.eclipse.core.commands.Command;
import org.eclipse.core.commands.ExecutionEvent;
import org.eclipse.core.commands.ExecutionException;
import org.eclipse.core.commands.State;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.PlatformUI;

import views.ViewSimple;

/**
 * Handler for the command in ViewSimple that determines whether to search
 * directories recursively 
 * 
 * @author 		Natalia Tyrpakova
 */
public class Recursion extends AbstractHandler {
	
	/** 
	 * {@inheritDoc}
	 * This implementation sends an information to View Simple when recursion
	 * option is changed.
	 *
	 * @see 			ExecutionEvent
	 */
	@Override
	public Object execute(ExecutionEvent event) throws ExecutionException {
		Command com = event.getCommand();
		State state = com.getState("org.eclipse.ui.commands.toggleState");
		boolean oldstate = (boolean)state.getValue();
		State newstate = new State();
		newstate.setValue(!oldstate);
		com.addState("org.eclipse.ui.commands.toggleState", newstate);
		IViewPart findView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("ratingandquantity.view2");
		if (findView != null) {
			try {
				((ViewSimple)findView).setRecursion(!oldstate);
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
		return null;
	}

	

}