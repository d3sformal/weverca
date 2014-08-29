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


package representation.tree;

import java.io.File;
import java.util.ArrayList;

import representation.Construct;
import representation.ConstructsInfo;
import general.FilePath;

/**
 * This class represents a tree node that holds file information.
 * Its children are instances of a TreeConstruct and its parent is
 * an instance of TreeFolder.
 * 
 * @author Natalia Tyrpakova
 * @see TreeConstruct
 * @see TreeFolder
 */
public class TreeFile {
	/**
	 * File this TreeFile is representing
	 * @see File
	 */
	public File tFile;
	/**
	 * TreeFolder that holds this TreeFile
	 * @see TreeFolder
	 */
	public TreeFolder parent;
	/**
	 * List of TreeConstruct objects representing the construct occurrences 
	 * found in the file represented by this TreeFile
	 */
	public ArrayList<TreeConstruct> childrenConstructs;
	/**
	 * Truncated path to the file represented by this TreeFile instance. This path is relative to the workspace.
	 */
	public String truncatedFilePath = "";
	
	private ConstructsInfo info;
	
	/**
	 * This constructor creates an instance from a File
	 * 
	 * @param file			File
	 * @param searchresult	ConstructsInfo that the children constructs are from
	 * @see					File
	 * @see					ConstructsInfo
	 */
	public TreeFile(File file, ConstructsInfo searchresult){
		tFile = file;
		truncatedFilePath = FilePath.truncate(file.getAbsolutePath());
		info = searchresult;
		childrenConstructs = new ArrayList<TreeConstruct>();
		getChildren();
	}
	
	/**
	 * Sets the file's children.
	 */
	private void getChildren(){
		ArrayList<Construct> temp = new ArrayList<Construct>();
		temp = info.getAllFilesInfo();
		for (Construct fi : temp){
			if (fi.path.equals(tFile.getAbsolutePath())){
				TreeConstruct constr = new TreeConstruct(fi.construct,fi.position[0],fi.position[2],fi.position[1],fi.position[3]);
				constr.parent = this;
				childrenConstructs.add(constr);
			}
		}
	}
	
	/**
	 * Gets the file's parent.
	 * @return	parenting folder as an instance of File
	 */	
	public File getParent(){
		return tFile.getParentFile();
	}
	
}