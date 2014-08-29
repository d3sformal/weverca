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

/**
 * Stores information about multiple constructs. These are stored as an ArrayList of FileInfos.
 * 
 * @author Natalia Tyrpakova
 * @see    FileInfo
 *
 */
public class ConstructsInfo {
	private ArrayList<FileInfo> fileinfo = new ArrayList<FileInfo>();
	
	/**
	 * Parses one line from Weverca_Runner into file name and construct information.
	 * 
	 * @param line	line from Weverca_Runner
	 * @param path	path to actual file in workspace that is searched for constructs (does not have to be file passed to Weverca)
	 * @see   ConstructsParser
	 */
	void ProcessLine(String line, String path){
		String delims = "[*]+";
		String[] tokens = line.split(delims);
		String filepath = path;
		String construct = tokens[3];
		String position = tokens[4];
		
		String newFilePath = filepath.replace("/", "\\");
		
		//save construct position
		String delims3 = ",";
		String[] tokens3 = position.split(delims3);
		addConstruct(newFilePath,construct,Integer.parseInt(tokens3[0]),Integer.parseInt(tokens3[1]),Integer.parseInt(tokens3[2]),Integer.parseInt(tokens3[3]));
	
	}
	
	/**
	 * Adds a new construct to the corresponding FileInfo.
	 * 
	 * @param path			path to the file that contains the construct
	 * @param construct		construct type
	 * @param firstline		position of the first line of the construct
	 * @param firstoffset	first character position offset
	 * @param lastline		position of the first line of the construct
	 * @param lastoffset	last character position offset
	 */
	private void addConstruct(String path, String construct, int firstline, int firstoffset, int lastline, int lastoffset){
		for (FileInfo fi: fileinfo){
			if (fi.path.equals(path)) {
				fi.addConstruct(construct, firstoffset, lastoffset);
				return;
			}
		}
		FileInfo fi = new FileInfo(path);
		fi.addConstruct(construct, firstoffset, lastoffset);
		fileinfo.add(fi);
	}
	
	/**
	 * Gets all the stored FileInfos.
	 * 
	 * @return ArrayList of FileInfo
	 * @see		FileInfo
	 */
	public ArrayList<FileInfo> GetAllFilesInfo(){
		return fileinfo;
	}
	
	
	

}