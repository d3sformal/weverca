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

import representation.FlowPart;
import representation.Resource;

/**
 * SplitPoint represents the last point of the common beginning of all the flows. 
 * It is necessary for the TreeViewer to be stored this way, for this point has multiple flows
 * as its descendants.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class SplitPoint extends FlowPoint {
	/**
	 * List of Resources and FirstFlowPoints representing the flow from this FlowPoint
	 */
	public ArrayList<Object> flows = new ArrayList<Object>();
	
	public SplitPoint(FlowPoint point){
		super(point);
	}
	
	public SplitPoint(){
		super();
	}
	
	/**
	 * Stores  the following flows
	 * 
	 * @param flows		flows to store as descendants
	 * @param reverse	indicator that the flows are reversed
	 * @param lastPath	file path of the last FlowPoint
	 */
	public void setFlows(ArrayList<FlowPart> flows, boolean reverse, String lastPath){
		for (FlowPart flow : flows){
			if (reverse){
				if (!lastPath.equals(flow.end.point.filePath)) {
					Resource res = new Resource(flow.end.point.getResourceName());
					res.parent = this;
					this.flows.add(res);
				}
			
				FirstFlowPoint firstPoint = new FirstFlowPoint(flow.end);
				firstPoint.prevPoint = null;
				firstPoint.nextPoint = null;
				firstPoint.parent = this;
				firstPoint.setFlow(flow.end.prevPoint, flow.end.point.filePath, reverse);
				this.flows.add(firstPoint);
			}
			else{
				if (!lastPath.equals(flow.start.point.filePath)) {
					Resource res = new Resource(flow.start.point.getResourceName());
					res.parent = this;
					this.flows.add(res);
				}
				FirstFlowPoint firstPoint = new FirstFlowPoint(flow.start);
				firstPoint.prevPoint = null;
				firstPoint.nextPoint = null;
				firstPoint.parent = this;
				firstPoint.setFlow(flow.start.nextPoint, flow.start.point.filePath, reverse);
				this.flows.add(firstPoint);
			}
		}
	}
}