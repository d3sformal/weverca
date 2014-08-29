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
/**
 * Data structure for storing php files' or folder's aggregated metric information. 
 * It holds the value of one property. If the fileName is a path to folder, this should store the aggregated metric 
 * information from all php files in the folder (recursively). If the fileName stores 
 * paths to one or more php files, this should store the aggregated metric information 
 * from all these files. 
 * 
 * @author Natalia Tyrpakova
 */
public class AggregatedMetricInformation {
	/**
	 * Truncated path to the file this information belongs to. This path is relative to the workspace.
	 */
	public String truncatedFileName = "";
	/**
	 * Property name (e.g. Number of lines...)
	 */
	public String property = "";
	/**
	 * Property value
	 */
	public float value;
	
	/**
	 * The constructor of AggregatedMetricInformation. Sets the file name,
	 * property name and property value.
	 * 
	 * @param prop		order number of the property as it is stored in MetricInformation
	 * @param mi		MetricInformation source
	 * @see				MetricInformation
	 */
	
	AggregatedMetricInformation(int prop, MetricInformation mi){
		truncatedFileName = mi.truncatedFileName;
		value = mi.metricValues.get(prop);
		switch(prop){
		case 0:
			property = "Number of lines";
			break;
		case 1:
			property = "Number of sources";
			break;
		case 2:
			property = "Maximum Inheritance Depth";
			break;
		case 3:
			property = "Maximum Method Overriding Depth";
			break;
		case 4:
			property = "Class Coupling";
			break;
		case 5:
			property = "PHP Functions Coupling";
			break;
		default:
			property = "";
		} 
	}

}