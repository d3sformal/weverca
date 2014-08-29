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
import representation.points.Point;

/**
 * This class parses and holds the taint flows - both merged and separated.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class TaintFlows {
	private ArrayList<Point> existingPoints = new ArrayList<Point>();
	private ArrayList<FlowPoint> sources = new ArrayList<FlowPoint>();
	private ArrayList<FlowPoint> sinks = new ArrayList<FlowPoint>();
	/**
	 * Represents common beginning of all the flows
	 */
	private FlowPart commonStart = null;
	/**
	 * Represents common end of all the flows
	 */
	private FlowPart commonEnd = null;
	/**
	 * Represent the middle of the flows that is not common to all of them
	 */
	private ArrayList<FlowPart> middle = new ArrayList<FlowPart>();
	/**
	 * The Warning that holds this TaintFlows information
	 * @see Warning
	 */
	public Warning parent;
	/**
	 * Indicator whether the flows should be shown as merged
	 */
	boolean merge = false;
	/**
	 * Indicator whether the flows should be shown as reversed
	 */
	boolean reverse = false;
	
	private ArrayList<FlowString> basic = null;
	private ArrayList<FlowString> reversed = null;
	private ArrayList<FlowString> merged = null;
	private ArrayList<FlowString> mergedAndReversed = null;
	
	
	/**
	 * Adds a new flow - only a pointer to the first FlowPoint is saved. 
	 * All the FlowPoints share the Point references.
	 * 
	 * @param lines		lines from the Weverca analyzer containing the flow to be added
	 */
	void addFlow(ArrayList<String> lines){
		String currentPath = "";
		ArrayList<String> variables = new ArrayList<String>();
		FlowPoint last = null;
		for (String line : lines)
		{
			if (line.length()>5 && line.substring(0,6).equals("File: ")){
				currentPath = line.substring(6, line.length());
				continue;
			}
			if (line.length()>5 && line.substring(0,6).equals("From: ")){
				String vars = line.substring(6, line.length());
				String delim = ",";
				String[] tokens = vars.split(delim);
				for (int i = 0; i < tokens.length; i++){
					variables.add(tokens[i]);
				}
			}
			if (line.length()>3 && line.substring(0,3).equals("-->")){
				String delim = "-->";
				String[] tokens = line.split(delim);
				 for (String token : tokens){
					 if (!token.contains(" at position ")) continue;
					 Point newPoint = new Point(token.substring(13, token.length()));
					 newPoint.setFilePath(currentPath);
					 
					 if (last != null && last.point.firstLine == newPoint.firstLine &&
							 last.point.filePath.equals(newPoint.filePath)){ //merge points
						 Point newNewPoint = new Point(newPoint);
						 newNewPoint.firstCol = Math.min(last.point.firstCol, newPoint.firstCol);
						 newNewPoint.lastCol = Math.max(last.point.lastCol, newPoint.lastCol);
						 newNewPoint.lastLine = Math.max(last.point.lastLine, newPoint.lastLine);
						 
						 int index = existingPoints.indexOf(newNewPoint);
						 if (index != -1) newNewPoint = existingPoints.get(index);
						 else existingPoints.add(newNewPoint);
						 
						 last.point = newNewPoint;
						 
						 last.addVariables(variables);
						 variables = new ArrayList<String>();
					 }
					 else { //create new point
						 int index = existingPoints.indexOf(newPoint);
						 if (index != -1) newPoint = existingPoints.get(index);
						 else existingPoints.add(newPoint);
						 
						 FlowPoint newFlowPoint = new FlowPoint(newPoint);
						 newFlowPoint.parentTaintFlows = this;
						 if (last != null){
							// last.nextPoints.add(newFlowPoint);
							 last.nextPoint = newFlowPoint;
							 newFlowPoint.prevPoint = last;
							 last = newFlowPoint;
						 }
						 else{
							 sources.add(newFlowPoint);
							 last = newFlowPoint;
						 }
						 newFlowPoint.variables = variables;
						 variables = new ArrayList<String>();
					 }
				 }
			}
		}
		if (last != null) sinks.add(reverseFlow(last));
	}
	
	/**
	 * Gets the reversed flow from a given FlowPoint
	 * 
	 * @param last		last FlowPoint of o flow that is being reversed
	 * @return			first FlowPoint of reversed flow
	 */
	private FlowPoint reverseFlow(FlowPoint last){
		FlowPoint newPoint = new FlowPoint(last.point);
		newPoint.variables = last.variables;
		newPoint.parentTaintFlows = last.parentTaintFlows;	
		
		if (last.prevPoint != null) newPoint.nextPoint = reverseFlow(last.prevPoint);
		if (newPoint.nextPoint != null) newPoint.nextPoint.prevPoint = newPoint;
		
		return newPoint;
	}
	
	/**
	 * Gets the FlowStrings representation of all the flows according to the merge and reverse indicators.
	 * The representations are stored, so that they are only computed once.
	 * 
	 * @return	representation of all the flows as a list of FlowString
	 */
	public ArrayList<FlowString> taintFlowsAsStrings(){
		
		FlowPart start = null;
		FlowPart end = null;
		
		if (reverse){
			if (commonEnd != null) start = commonEnd;
			if (commonStart != null) end = commonStart;
			
			if (merge){
				if (mergedAndReversed != null) return mergedAndReversed;
				mergedAndReversed = new ArrayList<FlowString>();
				mergedAndReversed.add(new FlowString(this,start,end,middle,reverse));
				return mergedAndReversed;
			}
			else {
				reversed = new ArrayList<FlowString>();
				for (FlowPoint sink : sinks){
					ArrayList<FlowPart> reversedFlow = new ArrayList<FlowPart>();
					reversedFlow.add(new FlowPart(sink,findLast(sink,false)));
					reversed.add(new FlowString(this,null,null,reversedFlow,false));
				}		
				return reversed;
			}	
		}
		else {
			if (commonEnd != null) end = commonEnd;
			if (commonStart != null) start = commonStart;
			
			if (merge){
				if (merged != null) return merged;
				merged = new ArrayList<FlowString>();
				merged.add(new FlowString(this,start,end,middle,reverse));
				return merged;
			}
			else {
				basic = new ArrayList<FlowString>();
				for (FlowPoint source : sources){
					ArrayList<FlowPart> basicFlow = new ArrayList<FlowPart>();
					basicFlow.add(new FlowPart(source,findLast(source,false)));
					basic.add(new FlowString(this,null,null,basicFlow,false));
				}			
				return basic;
			}	
		}
	}
	
	/**
	 * Splits the stored flows into common start, common and and separate middle parts of the flows
	 */
	void splitFlows(){
		getCommonStart();
		getCommonEnd();
		if (commonEnd != null && commonStart != null && commonEnd.start.point == commonStart.start.point) commonEnd = null; //only one flow
		getMiddle();
	}
	
	/**
	 * Stores the common start of the stored flows
	 */
	private void getCommonStart(){
		commonStart = findStart(sources, false);
	}
	
	/**
	 * Stores the common end of the stored flows
	 */
	private void getCommonEnd(){
		commonEnd = findStart(sinks, true);
	}
	
	/**
	 * Finds the common start of the list of FlowPoints. 
	 * Reverse indicator determines whether the list of FlowPoints is reversed.
	 * 
	 * @param flows		list of FlowPoints to find the common start for
	 * @param reverse	indicator that the list is reversed
	 * @return			FlowPart defining the flow
	 */
	private FlowPart findStart(ArrayList<FlowPoint> flows, boolean reverse){
		FlowPoint end;
		FlowPoint start;
		
		FlowPoint first = getCommonPoint(flows);	
		FlowPoint second = findLast(first, reverse);
		
		if (first == null || second == null) return null;
		
		if (reverse) {
			end = first;
			start = second;
		}
		else {
			start = first;
			end = second;
		}
		return new FlowPart(start,end);
	}
	
	/**
	 * Recursively gets the common FlowPoints from the beginning of given list of FlowPoint
	 * 
	 * @param flows		list of FlowPoints to get the common beginning from
	 * @return			flow of common FlowPoints
	 */
	private FlowPoint getCommonPoint(ArrayList<FlowPoint> flows){
		if (flows.isEmpty() || flows.get(0) == null) return null;
		Point point = flows.get(0).point;
		ArrayList<FlowPoint> newList = new ArrayList<FlowPoint>();
		
		FlowPoint newPoint = new FlowPoint(point);
		
		for (FlowPoint fp : flows){
			if (fp == null || fp.point != point) return null;
			newPoint.addVariables(fp.variables);
			newList.add(fp.nextPoint);
		}
		
		FlowPoint nextPoint = getCommonPoint(newList);
		newPoint.prevPoint = nextPoint;
		if (nextPoint != null) nextPoint.nextPoint = newPoint;
		
		return newPoint;
	}
	
	/**
	 * Finds the last FlowPoint of a given flow
	 * 
	 * @param fp		starting FlowPoint of the flow to find the last point from
	 * @param reverse	indicator that the flow is reversed
	 * @return			the last FlowPoint of the flow
	 */
	private FlowPoint findLast(FlowPoint fp, boolean reverse){
		if (fp == null) return null;
		if (!reverse){
			if (fp.nextPoint == null) return fp;
			return  findLast(fp.nextPoint, reverse);
		}
		else {
			if (fp.prevPoint == null) return fp;
			return  findLast(fp.prevPoint, reverse);
		}
	}
	
	/**
	 * stores the middle (not common) part of the stored flows
	 */
	private void getMiddle(){
		middle = new ArrayList<FlowPart>();
		
		for (FlowPoint fp : sources){
			middle.add(getMiddle(fp));
		}
	}
	
	/**
	 * Returns a middle part of given flow
	 * 
	 * @param fp	flow to get the middle part of
	 * @return		middle part of the flow
	 */
	private FlowPart getMiddle(FlowPoint fp){
		FlowPoint it = fp;
		FlowPoint start;
		FlowPoint end;
		
		if (commonStart != null){
			while (it.point != commonStart.end.point){
				it = it.nextPoint;
			}
			it = it.nextPoint;
		}
		
		if (it == null) return null;
		
		start = new FlowPoint(it.point);
		start.variables = it.variables;
		
		FlowPoint nextPoint = start;
		
		it = it.nextPoint;
		
		while(it != null && (commonEnd == null || it.point != commonEnd.start.point)){		
			FlowPoint newPoint = new FlowPoint(it.point);
			newPoint.variables = it.variables;
			nextPoint.nextPoint = newPoint;
			newPoint.prevPoint = nextPoint;		
			nextPoint = newPoint;
			it = it.nextPoint;
		}
		
		end = nextPoint;
		
		return new FlowPart(start,end);
	}
		
}