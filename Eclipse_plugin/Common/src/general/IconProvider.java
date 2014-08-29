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


package general;

import org.eclipse.core.runtime.FileLocator;
import org.eclipse.core.runtime.Path;
import org.eclipse.core.runtime.Platform;
import org.eclipse.jface.resource.ImageDescriptor;
import org.osgi.framework.Bundle;

/**
 * This class provides the icons for the plug-ins. All the images used are downloaded 
 * from the site http://iconlet.com or www.iconarchive.com and their 
 * licence allows sharing and redistributing
 *
 * @author Natalia Tyrpakova
 */

public class IconProvider {
	
	/**
	 * Returns a variable icon.
	 * 
	 * @return variable icon as an ImageDescriptor
	 * @see		ImageDescriptor
	 */
	public static ImageDescriptor getVariableIcon() {
		 Bundle bundle = Platform.getBundle("Common");
		 ImageDescriptor myImage = ImageDescriptor.createFromURL(
	              FileLocator.find(bundle, new Path("icons/genericvariable_obj_16x16.gif"),null));
		 return myImage;
	}
	
	
	/**
	 * Returns a value icon.
	 * @return	value icon as an ImageDescriptor
	 * @see		ImageDescriptor
	 */
	public static ImageDescriptor getValueIcon() {
		 Bundle bundle = Platform.getBundle("Common");
		 ImageDescriptor myImage = ImageDescriptor.createFromURL(
	              FileLocator.find(bundle, new Path("icons/variable_tab_16x16.gif"),null));
		 return myImage;
	}
	
	/**
	 * Returns a variable type icon.
	 * @return	variable type icon as an ImageDescriptor
	 * @see		ImageDescriptor
	 */
	public static ImageDescriptor getVariableTypeIcon() {
		 Bundle bundle = Platform.getBundle("Common");
		 ImageDescriptor myImage = ImageDescriptor.createFromURL(
	              FileLocator.find(bundle, new Path("icons/ComponentFamilyType_16x16.gif"),null));
		 return myImage;
	}
	
	/**
	 * Returns an alias icon.
	 * @return	alias icon as an ImageDescriptor
	 * @see		ImageDescriptor
	 */
	public static ImageDescriptor getAliasIcon() {
		 Bundle bundle = Platform.getBundle("Common");
		 ImageDescriptor myImage = ImageDescriptor.createFromURL(
	              FileLocator.find(bundle, new Path("icons/ValueExpressionDefault_16x16.gif"),null));
		 return myImage;
	}
	
	/**
	 * Returns a call icon.
	 * @return	call icon as an ImageDescriptor
	 * @see		ImageDescriptor
	 */
	public static ImageDescriptor getCallIcon() {
		 Bundle bundle = Platform.getBundle("Common");
		 ImageDescriptor myImage = ImageDescriptor.createFromURL(
	              FileLocator.find(bundle, new Path("icons/arrow-turn-090_16x16.png"),null));
		 return myImage;
	}
	
	/**
	 * Returns a flow icon.
	 * @return	flow icon as an ImageDescriptor
	 * @see		ImageDescriptor
	 */
	public static ImageDescriptor getFlowIcon() {
		 Bundle bundle = Platform.getBundle("Common");
		 ImageDescriptor myImage = ImageDescriptor.createFromURL(
	              FileLocator.find(bundle, new Path("icons/Arrows-Right-icon.png"),null));
		 return myImage;
	}
	
	/**
	 * Returns a dead code icon.
	 * @return	dead code icon as an ImageDescriptor
	 * @see		ImageDescriptor
	 */
	public static ImageDescriptor getDeadCodeIcon() {
		 Bundle bundle = Platform.getBundle("Common");
		 ImageDescriptor myImage = ImageDescriptor.createFromURL(
	              FileLocator.find(bundle, new Path("icons/cross_icon.png"),null));
		 return myImage;
	}
	
	/**
	 * Returns a view icon.
	 * @return	view icon as an ImageDescriptor
	 * @see		ImageDescriptor
	 */
	public static ImageDescriptor getViewIcon() {
		 Bundle bundle = Platform.getBundle("Common");
		 ImageDescriptor myImage = ImageDescriptor.createFromURL(
	              FileLocator.find(bundle, new Path("icons/view_icon.png"),null));
		 return myImage;
	}
	
	/**
	 * Returns a construct icon.
	 * @return	construct icon as an ImageDescriptor
	 * @see		ImageDescriptor
	 */
	public static ImageDescriptor getConstructIcon() {
		 Bundle bundle = Platform.getBundle("Common");
		 ImageDescriptor myImage = ImageDescriptor.createFromURL(
	              FileLocator.find(bundle, new Path("icons/genericvariable_obj_16x16.gif"),null));
		 return myImage;
	}
	
}