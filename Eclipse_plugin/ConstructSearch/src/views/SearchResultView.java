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

import org.eclipse.core.runtime.CoreException;
import org.eclipse.jface.viewers.TreeViewer;
import org.eclipse.swt.SWT;
import org.eclipse.swt.graphics.Image;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.ui.part.ViewPart;

import representation.ConstructsInfo;
import constructsearch.EditorListener;
import constructsearch.Highlighter;

/**
 * A view that shows the construct occurrences search result provided by the SearchPage.
 *
 * @author Natalia Tyrpakova
 * @see SearchPage
 */
public class SearchResultView extends ViewPart {

	private TreeViewer viewer;
	private ConstructsInfo searchresult = new ConstructsInfo();
	private ArrayList<String> roots;
	private Highlighter highlighter;

	/**
	 * Adds a TreeViewer to this view.
	 * 
	 * @see		TreeViewer
	 */
	@Override
	public void createPartControl(Composite parent) {
		viewer = new TreeViewer(parent, SWT.MULTI | SWT.H_SCROLL | SWT.V_SCROLL);
		try {
			CreateView();
		} catch (CoreException e) {
			e.printStackTrace();
		}	
	}
	
	/**
	 * Adds an EditorListener to the editors and tries to create the TreeViewer using Creator_SearchResult.
	 * 
	 * @throws CoreException
	 * @see		EditorListener
	 * @see		TreeViewer
	 * @see		Creator_SearchResultView
	 */
	private void CreateView() throws CoreException{
		
		if (highlighter != null) {
			highlighter.highlightOpenEditors();
			EditorListener listener = new EditorListener(highlighter);
			getSite().getWorkbenchWindow().getPartService().addPartListener(listener);
		}
		Creator_SearchResultView.createView(viewer, roots, searchresult);	
	}
	
	/**
	 * {@inheritDoc}
	 */
	@Override
	public void setFocus() {
		viewer.getControl().setFocus();
	}
	
	/**
	 * Sets the input for the TreeViewer in this view and creates a new corresponding Highlighter.
	 * 
	 * @param info		search result as a ConstructsInfo
	 * @param paths		roots for the TreeViewer
	 * @throws CoreException
	 * @see		TreeViewer
	 * @see		Highlighter
	 */
	public void setInput(ConstructsInfo info, ArrayList<String> paths) throws CoreException{
		searchresult = info;
		roots = paths;
		if (highlighter != null) highlighter.remove();
		highlighter = new Highlighter(searchresult);
		CreateView();
		
	}
	
	/**
	 * {@inheritDoc}
	 * In this implementation, the annotations are removed from editors.
	 */
	@Override
	public void dispose(){
		if (highlighter != null)
			try {
				highlighter.remove();
			} catch (CoreException e) {
				e.printStackTrace();
			}
		
	}
	
	@Override
	public Image getTitleImage() {
	    return general.IconProvider.getViewIcon().createImage();
	}

}