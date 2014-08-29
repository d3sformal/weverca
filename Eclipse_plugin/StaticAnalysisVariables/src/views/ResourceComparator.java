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

import representation.highlevel.ProgramPoint;

/**
 * Comparator for rows in DeadCodeView. It is used to reorder the elements.
 * 
 * @author Natalia Tyrpakova
 */

class ResourceComparator extends ViewerComparator{
		
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
		void clicked() {
		      direction = 1 - direction;
		}
		
		/**
		 * {@inheritDoc}
		 * 
		 * In this implementation resource names are compared.
		 */
	  @Override
	  public int compare(Viewer viewer, Object e1, Object e2) {
		int rc = 0;
		ProgramPoint p1 = null;
		ProgramPoint p2 = null;
		if (e1 instanceof ProgramPoint) p1 = (ProgramPoint) e1;
		if (p1 == null) rc = -1;
		if (e2 instanceof ProgramPoint) p2 = (ProgramPoint) e2;
		if (p2 == null) rc = 1;	

	    if (p1 != null && p2 != null) 
	    	rc = (p1.resource.toLowerCase()).compareTo(p2.resource.toLowerCase());	 
	    // If descending order, flip the direction
	    if (direction == DESCENDING) {
	      rc = -rc;
	    }
	    return rc;
	  }
}