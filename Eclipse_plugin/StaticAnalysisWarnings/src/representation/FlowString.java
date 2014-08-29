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


package representation;

import java.util.ArrayList;

import representation.points.FlowPoint;
import representation.points.SplitPoint;

/**
 * Serves as a string and list representation of a flow.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class FlowString {
	private FlowPart commonStart = null;
	private FlowPart commonEnd = null;
	private ArrayList<FlowPart> middle = new ArrayList<FlowPart>();
	private boolean reverse;
	private ArrayList<Object> flowList;
	
	/**
	 * Stored taint flow as a String
	 */
	public String flow = "";
	/**
	 * TaintFlows instance which holds this FlowString
	 */
	public TaintFlows parent;
	
	/**
	 * The constructor initializes the fields and creates a String representation of this flow.
	 * 
	 * @param parent		parent TaintFlows
	 * @param commonStart	the common start of all stored flows
	 * @param commonEnd		the common end of all stored flows
	 * @param middle		the middle parts of the stored flows
	 * @param reverse		indicator of reversed flow traversal
	 */
	FlowString(TaintFlows parent, FlowPart commonStart, FlowPart commonEnd, ArrayList<FlowPart> middle, boolean reverse){
		this.parent = parent;
		this.commonStart = commonStart;
		this.commonEnd = commonEnd;
		this.middle = middle;
		this.reverse = reverse;
		
		getString();
	}
	
	/**
	 * Gets the String representation of stored flow
	 */
	private void getString(){
		StringBuilder sb = new StringBuilder();
		String lastPath = "";
		
		//common start as string
		if (commonStart != null){
			if (reverse) {
				sb.append(commonStart.end.toString(lastPath, reverse));
				lastPath = commonStart.start.point.filePath;
			}
			else {
				sb.append(commonStart.start.toString(lastPath, reverse));
				lastPath = commonStart.end.point.filePath;
			}
		}
		
		//middle as string
		if (middle.size() == 1){
			if (reverse) {
				sb.append(middle.get(0).end.toString(lastPath, reverse));
				lastPath = middle.get(0).start.point.filePath;
			}
			else {
				sb.append(middle.get(0).start.toString(lastPath, reverse));
				lastPath = middle.get(0).end.point.filePath;
			}
		}
		if (middle.size() > 1){	
			String commonPath = "";
			if (reverse) {
				commonPath = middle.get(0).start.point.filePath;
				for (FlowPart fp : middle){
					sb.append(" (");
					sb.append(fp.end.toString(lastPath, reverse));
					sb.append(") ");
					if (fp != middle.get(middle.size()-1)) sb.append("or");
					if (!commonPath.equals(fp.start.point.filePath)){
						commonPath = "";
					}
				}
			}
			else {
				commonPath = middle.get(0).end.point.filePath;
				for (FlowPart fp : middle){	
					sb.append(" (");
					sb.append(fp.start.toString(lastPath, reverse));
					sb.append(") ");
					if (fp != middle.get(middle.size()-1)) sb.append("or");
					if (!commonPath.equals(fp.end.point.filePath)){
						commonPath = "";
					}
				}
			}
			lastPath = commonPath;
		}
			
		
		//common end as string
		if (commonEnd != null){
			if (reverse) {
				sb.append(commonEnd.end.toString(lastPath, reverse));
			}
			else {
				sb.append(commonEnd.start.toString(lastPath, reverse));
			}
		}

		flow = sb.toString();
	}
	
	/**
	 * Gets the list representation of the stored flow/s (for displaying in TaintFlow view)
	 * 
	 * @return		ArrayList of Objects representing the flow/s
	 */
	public ArrayList<Object> getList(){
		if (flowList != null) return flowList;
		
		ArrayList<Object> result = new ArrayList<Object>();
		String lastPath = "";
		
		if (commonStart != null){
			if (reverse) {
				result.addAll(commonStart.end.toList(lastPath, reverse));
				lastPath = commonStart.start.point.filePath;
			}
			else {
				result.addAll(commonStart.start.toList(lastPath, reverse));
				lastPath = commonStart.end.point.filePath;
			}
		}
		
		if (middle.size() > 1){
			if (commonStart != null){ //create SplitPoint from the last point
				int lastPoint = result.size()-1;
				if (result.get(lastPoint) instanceof FlowPoint){
					SplitPoint newPoint = new SplitPoint((FlowPoint)result.get(lastPoint));
					newPoint.setFlows(middle, reverse, lastPath);
					result.set(lastPoint, newPoint);
				}
			}
			else { //create empty SplitPoint
				SplitPoint newPoint = new SplitPoint();
				newPoint.setFlows(middle, reverse, lastPath);
				result.add(newPoint);
			}
		}
		
		if (middle.size() == 1){
			if (reverse) {
				result.addAll(middle.get(0).end.toList(lastPath, reverse));
				lastPath = middle.get(0).start.point.filePath;
			}
			else {
				result.addAll(middle.get(0).start.toList(lastPath, reverse));
				lastPath = middle.get(0).end.point.filePath;
			}
		}
		
		//find the last path
		if (!middle.isEmpty()){
			if (reverse) {
				lastPath = middle.get(0).start.point.filePath;
				for (FlowPart fp : middle){
					if (!lastPath.equals(fp.start.point.filePath)){
						lastPath = "";
						break;
					}
				}
			}
			else {	
				lastPath = middle.get(0).end.point.filePath;
				for (FlowPart fp : middle){
					if (!lastPath.equals(fp.end.point.filePath)){
						lastPath = "";
						break;
					}
				}
			}
		}
		
		if (commonEnd != null){
			if (reverse) {
				result.addAll(commonEnd.end.toList(lastPath, reverse));
				lastPath = commonEnd.start.point.filePath;
			}
			else {
				result.addAll(commonEnd.start.toList(lastPath, reverse));
				lastPath = commonEnd.end.point.filePath;
			}
		}
		
		flowList = result;
		return flowList;
	}
	
	
}