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


package representation.points;

import java.util.ArrayList;

import representation.Resource;

/**
 * FirstFlowPoint is a first FlowPoint descendant of a FlowPoint with multiple descendants. 
 * It is necessary for a TreeViewer to be stored this way, because the following flow points are
 * shown as its descendants.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class FirstFlowPoint extends FlowPoint {
	/**
	 * List of Resources and FlowPoints representing the flow from this FlowPoint
	 */
	public ArrayList<Object> flows = new ArrayList<Object>();
	
	/**
	 * Constructor creates an instance form a FlowPoint
	 * 
	 * @param flowPoint		FlowPoint to create FirstFlowPoint from
	 */
	FirstFlowPoint(FlowPoint flowPoint){
		super(flowPoint);
	}
	
	/**
	 * Stores the flow from this point that will be shown as it descendant.
	 * 
	 * @param p				FlowPoint representing the flow
	 * @param lastPath		path of the last FlowPoint
	 * @param reverse		indicator that the flow is reversed
	 */
	void setFlow(FlowPoint p, String lastPath, boolean reverse){
		if (p != null) flows.addAll(p.toList(lastPath, reverse));
		for (Object o : flows){
			if (o instanceof Resource) ((Resource)o).parent = this;
			if (o instanceof FlowPoint) ((FlowPoint)o).parent = this;
		}
	}

}