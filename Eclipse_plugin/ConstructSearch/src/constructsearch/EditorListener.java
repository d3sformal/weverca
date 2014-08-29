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

import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IPartListener;
import org.eclipse.ui.IWorkbenchPart;

/**
 * Listener for the editor opening. If an editor is opened, Highlighter is called
 * to highlight the construct occurrences.
 * 
 * @author 	Natalia Tyrpakova
 * @see		Highlighter
 */
public class EditorListener implements IPartListener {
	private Highlighter highlighter;
	
	/**
	 * Constructor sets the Highlighter.
	 * 
	 * @param h		Highlighter to be used for highlighting.
	 * @see			Highlighter
	 */
	public EditorListener(Highlighter h){
		highlighter = h;
	}
	
	@Override
	public void partActivated(IWorkbenchPart part) {
	}
	
	@Override
	public void partBroughtToTop(IWorkbenchPart part) {	
	}

	@Override
	public void partClosed(IWorkbenchPart part) {		
	}

	@Override
	public void partDeactivated(IWorkbenchPart part) {		
	}

	/**
	 * {@inheritDoc}
	 * In this implementation, if part is an instance of IEditorPart,
	 * the construct occurrences that were found are highlighted using the Highlighter.
	 * 
	 *  @see	Highlighter
	 */
	@Override
	public void partOpened(IWorkbenchPart part) {	
		if (part instanceof IEditorPart)
		highlighter.highlightEditor((IEditorPart)part);
	}

}