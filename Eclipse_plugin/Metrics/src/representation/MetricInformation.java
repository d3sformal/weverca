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
import general.FilePath;

/**
 * Data structure for storing php file's or folder's metric information. 
 * It stores all the metric information of a file or all aggregated metric 
 * information of a folder.   
 * 
 * @author Natalia Tyrpakova
 */
public class MetricInformation {
	/**
	 * Full path to the file this information belongs to.
	 */
	public String fileName = "";
	/**
	 * Truncated path to the file this information belongs to. This path is relative to the workspace.
	 */
	public String truncatedFileName = "";
	/**
	 * List of metric values
	 */
	public ArrayList<Float> metricValues = new ArrayList<Float>();	
	/**
	 * Indicator whether this information belongs to a file that was found recursively 
	 * and therefore should not be shown if recursion is turned off.
	 */
	boolean inRecursion;
	
	/**
	 * Constructor of MetricInformation. Sets the file name, recursion indicator and
	 * metric information as instances of metricValues.
	 * 
	 * @param line		one result line from Weverca analyzer, includes file name and metrics with values
	 * @param rec		recursion indicator
	 * @see 			metricValues
	 */
	public MetricInformation(String line,boolean rec) {
		inRecursion = rec;
		String delims = "[;]+";
		String[] tokens = line.split(delims);
		fileName = tokens[0].replace("\\","/");
		truncatedFileName = FilePath.truncate(fileName);
		
		for (int i=1; i<=6; ++i ){		
			String decimalPointReplaced = tokens[i].replace(',','.'); 		
			metricValues.add(Float.parseFloat(decimalPointReplaced));
		}
	}
	
	/**
	 * Sets the metric information file path and truncated file path
	 * 
	 * @param name	full file path
	 */
	public void setFileName(String name){
		fileName = name.replace("\\","/");
		truncatedFileName = FilePath.truncate(fileName);
	}
	
	/**
	 * Constructor of MetricInformation. Sets an empty file name, false recursion 
	 * indicator and sets all the metric information as instances of metricValues to zero.
	 * 
	 * @see 			metricValues
	 */
	public MetricInformation() {
		fileName = new String("");
		inRecursion = false;
		for (int i=0; i<6;++i){
			metricValues.add((float) 0);
		}
	}
	
	/**
	 * This is supposed to be used to create aggregated metric information. 
	 * Adds new file information to this instance of MetricInformation.
	 * 
	 * @param mi	a MetricInformation to be added to this
	 */
	void addNewInformation(MetricInformation mi){
		for (int i=0; i<2;++i){
			metricValues.set(i, metricValues.get(i)+mi.metricValues.get(i));
		}
		for (int i=2; i<6;++i){
			metricValues.set(i, Math.max(metricValues.get(i),mi.metricValues.get(i)));
		}
		
	}
	
	/**
	 * Gets the number of lines.
	 * 
	 * @return	the number of lines
	 */	
	public int getNumberOfLines(){
		return metricValues.get(0).intValue();
	}
	
	/**
	 * Gets the number of sources.
	 * 
	 * @return	the number of sources
	 */
	public int getNumberOfSources(){
		return metricValues.get(1).intValue();
	}
	
	/**
	 * Gets the maximum inheritance depth.
	 * 
	 * @return	the maximum inheritance depth
	 */
	public float getMaxInheritanceDepth(){
		return metricValues.get(2);
	}
	
	/**
	 * Gets the maximum method overriding depth.
	 * 
	 * @return	the maximum method overriding depth
	 */
	public float getMaxMethodOverridingDepth(){
		return metricValues.get(3);
	}
	
	/**
	 * Gets the class coupling.
	 * 
	 * @return	the class coupling
	 */
	public float getClassCoupling(){
		return metricValues.get(4);
	}
	
	/**
	 * Gets the PHP functions coupling.
	 * 
	 * @return	the PHP functions coupling
	 */
	public float getPHPFunctionsCoupling(){
		return metricValues.get(5);
	}
	
	
}