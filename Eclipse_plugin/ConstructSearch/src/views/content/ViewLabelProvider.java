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

import org.eclipse.jface.viewers.LabelProvider;
import org.eclipse.swt.graphics.Image;
import org.eclipse.ui.ISharedImages;
import org.eclipse.ui.PlatformUI;

import representation.tree.TreeConstruct;
import representation.tree.TreeFile;
import representation.tree.TreeFolder;
import views.SearchResultView;

/**
 * Label provider for the SearchResultView
 * 
 * @author Natalia Tyrpakova
 * @see		SearchResultView
 */
public class ViewLabelProvider extends LabelProvider {
    /**
     * {@inheritDoc}
     * In this implementation, if element is an instance of TreeFolder or TreeFile,
     * its name is returned. If element is an instance of TreeConstruct, its position and type are returned.
     * 
     * @see TreeFile
     * @see TreeFolder
     * @see TreeConstruct
     */
	@Override
    public String getText(Object element) {
    	if (element instanceof TreeFolder){
    		return ((TreeFolder)element).truncatedPath;
    	}
    	if (element instanceof TreeFile){
    		return ((TreeFile)element).truncatedFilePath;
    	}
    	if (element instanceof TreeConstruct){
    		int firstline = ((TreeConstruct)element).firstLine;
    		int lastline = ((TreeConstruct)element).lastLine;
    		String result = "construct type " + ((TreeConstruct)element).construct;
    		if (firstline == lastline){
    			result = result + " at line " + firstline;
    		}
    		else result = result + " at lines " + firstline + " to " + lastline;
    		return result;
    	}
    	if (element instanceof String){
    		return (String)element;
    	}
    	return "";
    }
	
	/**
     * {@inheritDoc}
     * In this implementation, if element is an instance of TreeFolder, the folder icon from ISharedImages is returned,
     * if element is an instance of TreeFile, the file icon from ISharedImages is returned, if element is 
     * an instance of TreeConstruct, the element icon form ISharedImages is returned.
     * 
     * @see TreeFile
     * @see TreeFolder
     * @see TreeConstruct
     * @see ISharedImages
     */
    @Override
    public Image getImage(Object element) {
    	   if (element instanceof TreeFile) {
    		   return PlatformUI.getWorkbench().getSharedImages().getImage(ISharedImages.IMG_OBJ_FILE);
    	   }
    	   if (element instanceof TreeFolder) {
    		   return PlatformUI.getWorkbench().getSharedImages().getImage(ISharedImages.IMG_OBJ_FOLDER);
    	   }
    	   if (element instanceof TreeConstruct) {
    		   return general.IconProvider.getConstructIcon().createImage();
    	   }
    	   
    	   return null;
    	}
  }