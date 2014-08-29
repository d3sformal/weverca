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
import representation.TaintFlows;

/**
 * FlowPoint class represents one point of the taint flow
 * 
 * @author Natalia Tyrpakova
 *
 */
public class FlowPoint {
	/**
	 * Point representing the position of this FlowPoint.
	 * @see Point
	 */
	public Point point;
	/**
	 * List of the names of tainted variables occurring in the piece of code corresponding to this FlowPoint 
	 */
	public ArrayList<String> variables;
	/**
	 * The following FlowPoint
	 */
	public FlowPoint nextPoint;
	/**
	 * The previous FlowPoint
	 */
	public FlowPoint prevPoint;
	/**
	 * The parent FlowPoint in a tree structure. This field is usually null.
	 */
	public FlowPoint parent;
	/**
	 * The TaintFlows instance that holds this FlowPoint
	 * @see TaintFlows
	 */
	public TaintFlows parentTaintFlows;
	
	/**
	 * Creates and instance from the Point information
	 * 
	 * @param point	Point with the position
	 */
	public FlowPoint(Point point){
		this.point = point;
		variables = new ArrayList<String>();
	}
	
	/**
	 * Creates an Empty FlowPoint
	 */
	public FlowPoint(){
		variables = new ArrayList<String>();
	}
	
	/**
	 * Copy constructor
	 * @param p		 another FlowPoint
	 */
	FlowPoint(FlowPoint p){
		nextPoint = p.nextPoint;
		prevPoint = p.prevPoint;
		parent = p.parent;
		parentTaintFlows = p.parentTaintFlows;
		point = p.point;
		variables = p.variables;
	}	
	
	/**
	 * Returns a recursive String representation of FlowPoint, if the resource has changed,
	 * it is added to the representation
	 * 
	 * @param resource		last point's resource
	 * @param reverse		indicator that the flow is reversed
	 * @return				String representation of the flow
	 */
	public String toString(String resource, boolean reverse){
		String newResource = point.filePath;
			
		StringBuilder pointString = new StringBuilder();
		if (!resource.equals(newResource)){
			pointString.append(" -> (" + point.getResourceName() + ") ");
		}
		pointString.append(point.toString());
		
		if (reverse){
			if (prevPoint != null){
				return (pointString.toString() + prevPoint.toString(newResource, reverse));
			}
		}
		else{
			if (nextPoint != null){
				return (pointString.toString() + nextPoint.toString(newResource, reverse));
			}
		}
				
		return pointString.toString();
	}
	
	

	/**
	 * Creates a list of Resources and FlowPoints to be shown in TaintFlowView. 
	 * 
	 * @param resource resource of the previous FlowPoint
	 * @return	list of FlowPoints and Resources
	 */
	public ArrayList<Object> toList(String resource,boolean reverse){
		ArrayList<Object> result = new ArrayList<Object>();
		String newResource = point.filePath;
		
		if (!resource.equals(newResource)){
			result.add(new Resource(point.getResourceName()));
		}
		result.add(new FlowPoint(this));
		
		if (reverse){
			if (prevPoint != null){
				result.addAll(prevPoint.toList(newResource, reverse));
			}
		}
		else{
			if (nextPoint != null){
				result.addAll(nextPoint.toList(newResource, reverse));
			}
		}
		
		return result;
	}
	
	/**
	 * Adds new variables to this FlowPoint, avoids duplicate variables
	 * 
	 * @param otherVariables	variables to add
	 */
	public void addVariables(ArrayList<String> otherVariables){
		for (String var : otherVariables){
			if (!variables.contains(var)) variables.add(var);
		}
	}
}