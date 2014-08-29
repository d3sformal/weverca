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

import representation.ConstructsInfo;
import general.FilePath;

/**
 * This class represents a tree node that holds folder information.
 * Its children are instances of a TreeFile and TreeFolder and its parent is
 * an instance of TreeFolder.
 * 
 * @author Natalia Tyrpakova
 * @see TreeFile
 */
public class TreeFolder {
	/**
	 * TreeFolder that holds this TreeFolder
	 * @see TreeFolder
	 */
	public TreeFolder parent;
	/**
	 * List of TreeFolder objects representing the child folders of this folder
	 */
	public ArrayList<TreeFolder> childrenFolders = new ArrayList<TreeFolder>();
	/**
	 * List of TreeFile objects representing the child files of this folder
	 */
	public ArrayList<TreeFile> childrenFiles = new ArrayList<TreeFile>();
	/**
	 * Truncated path to the file represented by this TreeFolder instance. This path is relative to the workspace.
	 */
	public String truncatedPath = "";
	
	private File folder;
	private ConstructsInfo info = new ConstructsInfo();
	
	/**
	 * This constructor creates an instance from a File
	 * 
	 * @param file			File
	 * @param searchresult	ConstructsInfo that the children constructs are from
	 * @see					File
	 * @see					ConstructsInfo
	 */
	public TreeFolder(File file, ConstructsInfo searchresult){
		folder = file;
		truncatedPath = FilePath.truncate(file.getAbsolutePath());
		info = searchresult;
		getChildren();
	}
	
	/**
	 * Gets the folder's parent.
	 * @return	parenting folder as an instance of File
	 */	
	public File getParent(){
		return folder.getParentFile();
	}
	
	/**
	 * Sets the folder's children.
	 */
	private void getChildren(){
		File[] children = folder.listFiles();
		for (int i = 0; i< children.length; ++i)
		{
			if (children[i].isDirectory()){
				if ((info.getAllDirectories()).contains(children[i].getAbsolutePath())){
					TreeFolder child = new TreeFolder(children[i],info);
					child.parent = this;
					childrenFolders.add(child);
				}
			}
			else{
				if ((info.getAllFiles()).contains(children[i].getAbsolutePath())){
					TreeFile child = new TreeFile(children[i],info);
					child.parent = this;
					childrenFiles.add(child);
				}
			}
		}
	}
	
}