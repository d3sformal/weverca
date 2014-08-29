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


package views.content;

import java.util.ArrayList;

import org.eclipse.jface.viewers.ArrayContentProvider;
import org.eclipse.jface.viewers.ITreeContentProvider;

import representation.tree.TreeConstruct;
import representation.tree.TreeFile;
import representation.tree.TreeFolder;
import views.SearchResultView;

/**
 * ITreeContentProvider for the SearchResultView.
 * 
 * @author Natalia Tyrpakova
 * @see		ITreeContentProvider
 * @see		SearchResultView
 */
public class ViewContentProvider extends ArrayContentProvider implements ITreeContentProvider {
    
    /**
     * Sets the input elements as the roots of the TreeViewer.
     * 
     * @param inputElement		the root elements
     * @return					array of input elements
     */
    @Override
    public Object[] getElements(Object inputElement) {
    	if (inputElement instanceof ArrayList) return ((ArrayList<Object>) inputElement).toArray();
    	else return new Object[] {inputElement};
    }

    
    @Override
    public Object[] getChildren(Object parentElement) {
    	if (parentElement instanceof TreeFolder){
    		Object[] folders = ((TreeFolder)parentElement).childrenFolders.toArray();
    		Object[] files = ((TreeFolder)parentElement).childrenFiles.toArray();
    		Object[] returnarray = new Object[folders.length+files.length];
    		System.arraycopy(folders, 0, returnarray, 0, folders.length);
    		System.arraycopy(files, 0, returnarray, folders.length, files.length);
    		return returnarray;
    	}
    	else if (parentElement instanceof TreeFile){
    		return ((TreeFile)parentElement).childrenConstructs.toArray();
    	}
    	else return new Object[] {};
    }

    @Override
    public Object getParent(Object element) {
    	if (element instanceof TreeFolder){
    		return ((TreeFolder)element).parent;
    	}
    	if (element instanceof TreeFile){
    		return ((TreeFile)element).parent;
    	}
    	if (element instanceof TreeConstruct){
    		return ((TreeConstruct)element).parent;
    	}
    	return null;
    }

    @Override
    public boolean hasChildren(Object element) {
    	if (element instanceof TreeFolder || element instanceof TreeFile){
    		return (getChildren(element).length >0);
    	}
      return false;
    }

  }