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
import java.util.Map;

/**
 * This class stores the construct information of files and folders.
 * It contains an ArrayList of all files that have their construct
 * information stored and ArrayList of all their folders. Each construct
 * is stored as a construct instance.
 * 
 * @author Natalia Tyrpakova
 * @see construct
 */
public class ConstructsInfo {
	private ArrayList<String> directories;
	private ArrayList<String> files;
	private ArrayList<Construct> fileinfo;
	
	/**
	 * The constructor initializes the ArrayLists
	 */
	public ConstructsInfo(){
		directories = new ArrayList<String>();
		files = new ArrayList<String>();
		fileinfo = new ArrayList<Construct>();
	}
	
	/**
	 * Parses one line from Weverca_Runner output and gets the construct information from it.
	 * @param line				one line from Weverca_Runner to be parsed
	 * @param tempFileToFile	map of temporary file names that were sent to analyzer
	 * 							to actual file names 
	 */
	void processLine(String line, Map<String,String> tempFileToFile ){
		String delims = "[*]+";
		String[] tokens = line.split(delims);
		String filepath = tokens[1];
		String construct = tokens[3];
		String position = tokens[4];
		
		String actualFile = tempFileToFile.get(filepath);
		if (actualFile != null){
			filepath = actualFile;
		}

		//save all potential directories
		String delims2 = "[\\/\\\\]";
		String[] tokens2 = filepath.split(delims2);
		String beginning = tokens2[0];
		String newfilepath = tokens2[0];
		for (int i = 1; i< tokens2.length-1; ++i){
			beginning = beginning + "\\" + tokens2[i];
			newfilepath = newfilepath + "\\" + tokens2[i];
			addDirectory(beginning);
		}
		newfilepath = newfilepath + "\\" + tokens2[tokens2.length-1];
		
		//save file name
		addFile(newfilepath);
		
		//save construct position
		String delims3 = ",";
		String[] tokens3 = position.split(delims3);
		addFileInfo(newfilepath,construct,Integer.parseInt(tokens3[0]),Integer.parseInt(tokens3[1]),Integer.parseInt(tokens3[2]),Integer.parseInt(tokens3[3]));	
	}

	/**
	 * Adds a new directory to the ArrayList that stores directories
	 * that are searched for constructs.
	 * 
	 * @param dir	directory to be added
	 */
	private void addDirectory(String dir){
		if (!directories.contains(dir)){
		directories.add(dir);
		}
	}
	
	/**
	 * Adds a new file to the ArrayList that stores files
	 * that are searched for constructs.
	 * @param file	file to be added
	 */
	private void addFile(String file){
		if (!files.contains(file)){
			files.add(file);
		}
	}
	
	/**
	 * Adds a new construct information to the ArrayList that stores the constructs.
	 * 
	 * @param path			path to the file that contains the construct
	 * @param construct		construct type
	 * @param firstline		position of the first line of the construct
	 * @param firstoffset	first character position offset
	 * @param lastline		position of the first line of the construct
	 * @param lastoffset	last character position offset
	 * @see 				construct
	 */
	private void addFileInfo(String path, String construct, int firstline, int firstoffset, int lastline, int lastoffset){
		fileinfo.add(new Construct(path, construct, firstline,firstoffset,lastline,lastoffset));
	}
	
	/**
	 * Gets the directories stored.
	 * @return		ArrayList of stored directories
	 */
	public ArrayList<String> getAllDirectories(){
		return directories;
	}
	
	/**
	 * Gets the files stored.
	 * @return		ArrayList of stored files
	 */
	public ArrayList<String> getAllFiles(){
		return files;
	}
	
	/**
	 * Gets the constructs stored.
	 * @return		ArrayList of stored constructs
	 * @see			construct
	 */
	public ArrayList<Construct> getAllFilesInfo(){
		return fileinfo;
	}	

}