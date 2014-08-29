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

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;

import org.eclipse.jface.text.IDocument;
import org.eclipse.ui.IEditorInput;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IEditorReference;
import org.eclipse.ui.IFileEditorInput;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.texteditor.IDocumentProvider;
import org.eclipse.ui.texteditor.ITextEditor;

import exceptions.AnalyzerNotFoundException;
import wevercarunner.CollectingOutputProcessor;
import wevercarunner.Runner;
import general.Messages;

/**
 * This class provides methods that get the construct occurrences and parse them, and stores the result.
 * 
 * @author Natalia Tyrpakova
 */
public class ConstructsParser {
	private Map<String,IDocument> filePathToDocument;
	private Map<String,String> tempFileToFile;
	private ArrayList<String> constructs;
	private ArrayList<String> dirtyEditors;
	
	/**
	 * The constructor initializes the fields.
	 */
	public ConstructsParser(){
		filePathToDocument = new HashMap<String,IDocument>();
		tempFileToFile = new HashMap<String,String>();
		constructs = new ArrayList<String>();
		dirtyEditors = new ArrayList<String>();
	}
	
	/**
	 * This method gets occurrences of given constructs in given resources and initializes
	 * a list of dirty editors. 
	 *  
	 * @param sources		paths to the resources to be searched for constructs
	 * @param constructs	types of constructs to be searched for
	 * @return				ConstructsInfo
	 * @throws IOException
	 * @see					ConstructsInfo
	 */
	public ConstructsInfo GetConstructOccurances(ArrayList<String> sources,ArrayList<String> constructs) throws IOException
	{		
		this.constructs = constructs; 		
		getDirtyEditors();
		ConstructsInfo c = new ConstructsInfo();
		
		//fill the temporary file for analyze
		File temp = File.createTempFile("fileforanalyzer", ".php");
		BufferedWriter bw = new BufferedWriter(new FileWriter(temp.getAbsolutePath()));
		fillFileForAnalyzer(bw, sources);
		bw.close();
		
		c = callAnalyzer(temp);	
		
		return c;
	}
	
	/**
	 * Fills a temporary file of file paths to analyze
	 * 
	 * @param bw			BufferedWriter that writes to the temporary file
	 * @param sources		paths to write to the file
	 * @throws IOException
	 */
	private void fillFileForAnalyzer(BufferedWriter bw,ArrayList<String> sources) throws IOException{
		for (String source : sources){
			File sourceFile = new File(source);
			
			if (sourceFile.isFile()){
				String fileToSearchIn = source;
				if (dirtyEditors.contains(sourceFile.getAbsolutePath())){//get temporary file for Weverca
					File substitute = createTempFile(sourceFile); 
					fileToSearchIn = substitute.getAbsolutePath();
					tempFileToFile.put(fileToSearchIn, sourceFile.getAbsolutePath());
				}
			bw.write(fileToSearchIn);		
			bw.newLine();
			}
			
			else if (sourceFile.isDirectory()){
				File[] childrenArray = sourceFile.listFiles();
				ArrayList<String> children = new ArrayList<String>();
				for (File ch : childrenArray){
					children.add(ch.getAbsolutePath());
				}
				fillFileForAnalyzer(bw,children);
			}
		}		
	}
	
	/**
	 * Gets the construct occurrences from the files contained in the provides file. 
	 * These ocurrences are then parsed into a ConstructInfo.
	 * 
	 * @param file		file of files to analyze
	 * @return			resulting ConstructInfo
	 * @throws IOException
	 */
	private ConstructsInfo callAnalyzer(File file) throws IOException{
		ConstructsInfo c = new ConstructsInfo();
		
		ArrayList<String> parameters = new ArrayList<String>(Arrays.asList("-cmide", "-constructsFromFileOfFiles"));
		parameters.addAll(constructs);
		parameters.add(file.getAbsolutePath());
		
		List<String> output = new LinkedList<>();
		try {
			Runner runner = new Runner();
			CollectingOutputProcessor out = new CollectingOutputProcessor();
			runner.runWeverca(parameters, out);
			output = out.getOutput();
		} catch (AnalyzerNotFoundException e) {
			Messages.incorrectAnalyzerPath();
		}
		
		for ( String line: output) {
			if (!line.equals("error")){
				String substr = line.substring(0, 7);
				if (!substr.equals("Process")) {
					c.processLine(line,tempFileToFile);
				}	
			}
		}
	
		Iterator it = tempFileToFile.entrySet().iterator();
		while (it.hasNext()) {
	        Map.Entry<String,String> pairs = (Map.Entry<String,String>)it.next();
	        File f = new File(pairs.getKey());
	        f.delete();
	    }
			
		return c;
	}
	
	/**
	 * Stores the paths to dirty editors
	 */
	private void getDirtyEditors(){
		IEditorReference[] editors = PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage().getEditorReferences();
		for (IEditorReference ref : editors){
			IEditorPart editor = (IEditorPart)ref.getEditor(true);
			if (editor.isDirty()){
				IEditorInput editorInput = editor.getEditorInput();
				if (editorInput instanceof IFileEditorInput && editor instanceof ITextEditor) {
					String editorPath = ((IFileEditorInput)editorInput).getFile().getLocation().toOSString();
					IDocumentProvider idp = ((ITextEditor)editor).getDocumentProvider();
	                dirtyEditors.add(editorPath);
	                filePathToDocument.put(editorPath, idp.getDocument(editorInput));
				}
			}
		}
	}
	
	
	
	/**
	 * Creates a temporary file with .php extension from dirty editor to be sent to analyzer.
	 * 
	 * @param sourceFile	File that is open in a dirty editor.
	 * @return				temporary File
	 * @see					File
	 */
	private File createTempFile(File sourceFile){
		File temp = null;
		try {		
			temp = File.createTempFile("fileforweverca", ".php");		
		} catch (IOException e) {
			e.printStackTrace();
		} 		
		String content = filePathToDocument.get(sourceFile.getAbsolutePath()).get();
		FileWriter fw;
		try {
			fw = new FileWriter(temp.getAbsoluteFile());
			BufferedWriter bw = new BufferedWriter(fw);
			bw.write(content);
			bw.close();
		} catch (IOException e) {
			e.printStackTrace();
		}
		return temp;
	}

}