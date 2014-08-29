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
import org.eclipse.core.runtime.CoreException;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.PlatformUI;

import views.VariablesView;

/**
 * Handler for the before toggle button. This button determines, 
 * which variables are shown - whether the ones that were computed before the line had been processed,
 * or those that were computed after it.
 * 
 * @author Natalia Tyrpakova
 * @see		AbstractHandler
 */
public class SetBeforeAfter extends AbstractHandler {

	/**
	 * {@inheritDoc}
	 * In this implementation the VariablesView is informed about the toggle button state change.
	 * @see		VariablesView
	 */
	@Override
	public Object execute(ExecutionEvent event) throws ExecutionException {
		IViewPart findView = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().findView("StaticAnalysisVariables.view1");
		if (findView != null) {
			try {
				boolean oldstate = ((VariablesView)findView).getBefore();
				((VariablesView)findView).setBefore(!oldstate);
			} catch (CoreException e) {
				e.printStackTrace();
			}
		}
		return null;
	}

}