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

import org.eclipse.jface.viewers.Viewer;
import org.eclipse.jface.viewers.ViewerComparator;
import org.eclipse.swt.SWT;

import representation.Warning;

/**
 * Comparator for rows in WarningsView. It is used to reorder the elements.
 * 
 * @author Natalia Tyrpakova
 */

class WarningsComparator extends ViewerComparator{
		private int propertyIndex;
		private static int DESCENDING = 1;
		private int direction = DESCENDING;
		
		/**
		 * Gets the direction in which the column is sorted.
		 * 
		 * @return int direction
		 */
		public int getDirection() {
		    if (direction == 1) return SWT.DOWN;
		    else return SWT.UP;
		}

		/**
		 * Sets the direction.
		 * @param column zero-based index of column
		 */
		public void setColumn(int column) {
			if (column == this.propertyIndex) { // Same column as last sort
			      direction = 1 - direction;
			    } else { // New column
			      this.propertyIndex = column;
			      direction = DESCENDING;
			    }
		}
		
		/**
		 * {@inheritDoc}
		 * 
		 * Depending on the column, resource or priority is compared
		 */
	  @Override
	  public int compare(Viewer viewer, Object e1, Object e2) {
		int rc = 0;
		Warning w1 = null;
		Warning w2 = null;
		if (e1 instanceof Warning) w1 = (Warning) e1;
		if (w1 == null) rc = -1;
		if (e2 instanceof Warning) w2 = (Warning) e2;
		if (w2 == null) rc = 1;	
	    
		if (w1 != null && w2 != null){
			if (propertyIndex == 1) rc = (w1.resource.toLowerCase()).compareTo(w2.resource.toLowerCase());
			if (propertyIndex == 3) rc = (((Boolean)w1.highPriority).compareTo(w2.highPriority));
		}
	    		 
	    // If descending order, flip the direction
	    if (direction == DESCENDING) {
	      rc = -rc;
	    }
	    return rc;
	  }
}