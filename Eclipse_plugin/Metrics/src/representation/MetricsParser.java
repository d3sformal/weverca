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


import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import exceptions.AnalyzerNotFoundException;
import wevercarunner.CollectingOutputProcessor;
import wevercarunner.Runner;

/**
 * This class uses a Runner to connect to the Weverca analyzer
 * and serves as a parser and storage of metric information.
 * 
 * @author Natalia Tyrpakova
 *
 */

public class MetricsParser {
	/**
	 * List of paths to the files that were not successfully analyzed.
	 */
	public ArrayList<String> nonProcessedFiles;
	
	private ArrayList<String> sources;
	private ArrayList<MetricInformation> info;
	private ArrayList<MetricInformation> infoFiles;
	private ArrayList<MetricInformation> infoFolders;
	private MetricInformation sourceInformation;
	//public long analysisTime;
	
	
	/**
	 * The constructor initializes the fields, particularly a list of files to be analyzed.
	 * 
	 * @param sources	ArrayList of paths to be recursively analyzed
	 */
	public MetricsParser(ArrayList<String> sources){
		this.sources = sources;
		info = new ArrayList<MetricInformation>();
		infoFiles = new ArrayList<MetricInformation>();
		infoFolders = new ArrayList<MetricInformation>();
		nonProcessedFiles = new ArrayList<String>();
	}
	
	/**
	 * Processes all the source files using callWeverca procedure and stores the aggregated
	 * metric information.
	 * 
	 * @throws AnalyzerNotFoundException 
	 */
	public void run() throws AnalyzerNotFoundException {
	
		try {			
			sourceInformation = new MetricInformation();
			if (sources.size() == 1) sourceInformation.setFileName(sources.get(0));
			else sourceInformation.setFileName("Selected resources");
			
			for (int i = 0; i<sources.size();++i){
				callWeverca(sources.get(i));
			}
			
			for (MetricInformation mi:infoFolders){
				sourceInformation.addNewInformation(mi);
			}		
		} catch (IOException e1) {
			e1.printStackTrace();
		}
	}
	
	/**
	 * Calls the analyzer and properly saves all the information into the fields.
	 * 
	 * @param source			path to the file to be analyzed
	 * @param pathToWeverca		path to the analyzer
	 * @throws IOException
	 * @throws AnalyzerNotFoundException 
	 */
	private void callWeverca(String source) throws IOException, AnalyzerNotFoundException{
		ArrayList<String> parameters = new ArrayList<String>(Arrays.asList("-cmide","-quantity", source));
		
		Runner runner = new Runner();
		CollectingOutputProcessor out = new CollectingOutputProcessor();
		runner.runWeverca(parameters, out);
		List<String> output = out.getOutput();
		
		String delims = "[;]+";
		for (String line : output) {
			String substr = "";
			if (line.contains("Time:")){
				//analysisTime = Long.parseLong(line.substring(line.indexOf(':')+2));
				continue;
			}
			if (line.contains("Not processed: ")){
				nonProcessedFiles.add(line.substring(line.indexOf(':')+2));
				continue;
			}
			if (line.length()>6) substr = line.substring(0, 7);
			if (!substr.equals("Process") && line.length()>6) {
				String[] tokens = line.split(delims);
				String fileName = tokens[0].replace("\\","/");
				int pos = fileName.lastIndexOf('/');
				String directoryName = fileName.substring(0, pos);
				boolean inRecursion = true;
				if (sources.contains(directoryName) || sources.contains(fileName)) inRecursion = false;
				
				MetricInformation mi = new MetricInformation(line,inRecursion);
				info.add(mi);
				infoFiles.add(mi);
				boolean alreadyExists = false;
				for (MetricInformation folderMi: infoFolders){
					if (folderMi.fileName.equals(directoryName)) {
						folderMi.addNewInformation(mi);
						alreadyExists = true;
						break;
					}
				}
				if (!alreadyExists){
					MetricInformation folderInformation = new MetricInformation();
					folderInformation.inRecursion = inRecursion;
					folderInformation.setFileName(directoryName);
					folderInformation.addNewInformation(mi);
					infoFolders.add(folderInformation);
					info.add(folderInformation);
				}
			}
		} 
	}

	
	/**
	 * Provides the stored metric information of selected files and folders 
	 * depending on the parameters.
	 * 
	 * @param files			determines whether files information should be returned
	 * @param folders		determines whether folders information should be returned
	 * @param recursive		determines whether the information about files and folders 
	 * 						that were found recursively should be returned
	 * @return				metric information about selected files and folders
	 * @throws IOException
	 * @see MetricInformation
	 */
	public ArrayList<MetricInformation> GetRatingAndQuantity(boolean files, boolean folders, boolean recursive) throws IOException {
		if (recursive){
			if (files && folders) return info;
			if (files) return infoFiles;
			if (folders) return infoFolders;
		}
		else {
			ArrayList<MetricInformation> sourceList = new ArrayList<MetricInformation>();
			if (files && folders) sourceList = info;
			else if (files) sourceList = infoFiles;
			else if (folders) sourceList =  infoFolders;
			
			if (sourceList != null){
				ArrayList<MetricInformation> resultList = new ArrayList<MetricInformation>();
				for (MetricInformation mi : sourceList){
					if (!mi.inRecursion) resultList.add(mi);
				}
				return resultList;
			}
		}	
		return null;
	}
	
	
	/**
	 * Provides the stored aggregated metric information of all files and folders that were selected
	 * 
	 * @return		ArrayList of AggregatedMetricInformation
	 * @throws 		IOException
	 * @see 		AggregatedMetricInformation
	 */
	public ArrayList<AggregatedMetricInformation> GetAggregatedRatingAndQuantity() throws IOException {
		ArrayList<AggregatedMetricInformation> aggregated = new ArrayList<AggregatedMetricInformation>();
		if (sourceInformation == null) return null;
		for (int i = 0; i<=5; ++i){
			aggregated.add(new AggregatedMetricInformation(i,sourceInformation));
		}			
		return aggregated;	
	}

	

}