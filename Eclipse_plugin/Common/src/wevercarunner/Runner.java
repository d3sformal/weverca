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


package wevercarunner;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.ArrayList;

import exceptions.AnalyzerNotFoundException;


/**
 * This class serves as a connection to the Weverca analyzer. 
 * 
 * @author Natalia Tyrpakova
 *
 */
public class Runner {
	private static boolean osDetermined = false;
	private static boolean isWindows = false;
	private static boolean isWindows() {
		if (osDetermined) return isWindows;
		isWindows = System.getProperty("os.name").startsWith("Windows");
		osDetermined = true;
		return isWindows;
	}
	private Process process;
	
	public void runWeverca(ArrayList<String> params, OutputProcessor output) throws  AnalyzerNotFoundException{
		if (params.get(1).equalsIgnoreCase("-staticanalysis")) runWevercaStaticAnalysis(params, output); 
		else runWevercaMetrics(params, output);
	}
	
	private synchronized void runWevercaMetrics(ArrayList<String> params, OutputProcessor output) throws  AnalyzerNotFoundException {
		doRunWeverca(params, output, "_____wevercaoutput_metrics.txt");
	}
	
	private synchronized void runWevercaStaticAnalysis(ArrayList<String> params, OutputProcessor output) throws  AnalyzerNotFoundException {
		doRunWeverca(params, output, "_____wevercaoutput_static_analysis.txt");
	}
	
	private void doRunWeverca(ArrayList<String> params, OutputProcessor output, String fileName) throws  AnalyzerNotFoundException {	
		// Get output file
		String pathToOutputFile = activator.Activator.getDefault().getPreferenceStore().getString("PATH");
		pathToOutputFile = pathToOutputFile.replace("\\", "/");
		File outputFile = new File(pathToOutputFile);
		if (!outputFile.isDirectory()) {
			if (pathToOutputFile.endsWith("Weverca.exe")) {
				pathToOutputFile = pathToOutputFile.substring(0, pathToOutputFile.indexOf("Weverca.exe")-1);
			}
		}
		if (pathToOutputFile.charAt(pathToOutputFile.length()-1) != '/') pathToOutputFile = pathToOutputFile + "/";
		pathToOutputFile = pathToOutputFile + fileName;
		outputFile = new File(pathToOutputFile);
		
		
		//Get path to Weverca analyzer
		String pathToWeverca = activator.Activator.getDefault().getPreferenceStore().getString("PATH");	
		pathToWeverca = pathToWeverca.replace("\\", "/");
		
		File file = new File(pathToWeverca);
		if (file.isDirectory()) {
			if (pathToWeverca.charAt(pathToWeverca.length()-1) != '/') pathToWeverca = pathToWeverca + "/";
			pathToWeverca = pathToWeverca + "Weverca.exe";
		}
		file = new File(pathToWeverca);
		
		if (!file.exists()) {
			if (activator.Activator.getDefault().getPreferenceStore().getBoolean("HIDE_ERROR")) 
				return;
			else throw new AnalyzerNotFoundException();
		}
		
		ArrayList<String> parameters = new ArrayList<String>();
		if (!isWindows()) parameters.add("mono");
		parameters.add(pathToWeverca);
		parameters.addAll(params);
		parameters.add(pathToOutputFile);
		
		
		try {
			process = new ProcessBuilder(parameters).start();
			//new ProcessBuilder();
		} catch (IOException e) {
			if (activator.Activator.getDefault().getPreferenceStore().getBoolean("HIDE_ERROR")) 
				return;
			//else throw new AnalyzerNotFoundException();
		}
	
		try {
			process.waitFor();
		} catch (InterruptedException e) {
			return;
		}
		if (process.exitValue() != 0) return;
		
		
		
		// Process output
		InputStream is;
		try {
			is = new FileInputStream(outputFile);
			InputStreamReader isr = new InputStreamReader(is);
			BufferedReader br = new BufferedReader(isr);
			String line;	
			while ( (line = br.readLine()) != null) {
				output.processLine(line);
			}
		} catch (Exception e) {
			return;
		}	finally {
			outputFile.delete();
		}
		
		return;
				
	}
	
	
	
	/**
	 * Stops the process that is currently executing the Weverca analyzer
	 */
	public void stopProcess(){
		if (process != null) process.destroy();
	}
}