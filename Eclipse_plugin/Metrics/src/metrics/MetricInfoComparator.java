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


package metrics;

import org.eclipse.jface.viewers.Viewer;
import org.eclipse.jface.viewers.ViewerComparator;
import org.eclipse.swt.SWT;

import representation.MetricInformation;

/**
 * Comparator for rows in ViewSimple. It is used to reorder the elements.
 * 
 * @author Natalia Tyrpakova
 */
public class MetricInfoComparator extends ViewerComparator {
	
	private int propertyIndex;
	private static int DESCENDING = 1;
	private int direction = DESCENDING;
	
	/**
	 * Constructor sets the index of sorted column to zero.
	 */
	public MetricInfoComparator() {
		propertyIndex = 0;
	}
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
	 * Sets the column that the table is supposed to be sorted by.
	 * @param column zero-based index of column
	 */
	public void setColumn(int column) {
	    if (column-1 == this.propertyIndex) { // Same column as last sort
	      direction = 1 - direction;
	    } else { // New column
	      this.propertyIndex = column-1;
	      direction = DESCENDING;
	    }
	}
	/**
	* {@inheritDoc}
	* 
	* In this implementation, MetricInformations are compared 
	* and the result depends on the column which defines 
	* whether the file name or a certain property is compared.
	*/
	  @Override
	  public int compare(Viewer viewer, Object e1, Object e2) {
	    MetricInformation mi1 = (MetricInformation) e1;
	    MetricInformation mi2 = (MetricInformation) e2;
	    int rc = 0;
	    if (propertyIndex == -1) rc = (mi1.fileName.toLowerCase()).compareTo(mi2.fileName.toLowerCase());	 
	    else  {
	    	rc = (mi1.metricValues.get(propertyIndex)).compareTo(mi2.metricValues.get(propertyIndex));	
	    	if (rc == 0) rc = (mi1.fileName.toLowerCase()).compareTo(mi2.fileName.toLowerCase()); //secondary comparing
	    }
	    // If descending order, flip the direction
	    if (direction == DESCENDING) {
	      rc = -rc;
	    }
	    return rc;
	  }

}