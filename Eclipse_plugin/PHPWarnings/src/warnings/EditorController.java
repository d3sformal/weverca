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


package warnings;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;
import java.util.TimerTask;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicReference;

import org.eclipse.core.resources.IFile;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IFileEditorInput;

import representation.ConstructsInfo;
import representation.ConstructsParser;
import exceptions.AnalyzerNotFoundException;
import general.Messages;

/**
 * This class manages the process of warning creation. 
 * It can be scheduled for repeated execution.
 * @author Natalia Tyrpakova
 *
 */
class EditorController extends TimerTask {
	/**
	 * Maps a IDocument to the corresponding IEditorPart
	 * @see IDocument
	 * @see IEditorPart
	 */
	Map<IDocument,IEditorPart> documentsToEditors = new HashMap<IDocument,IEditorPart>();
	/**
	 * Indicates that there was a change in the active editor
	 */
	AtomicBoolean editorChanged = new AtomicBoolean();
	/**
	 * Represents the changed document
	 */
	AtomicReference<IDocument> changedDocument = new AtomicReference<IDocument>();
	
	private WarningsMaker wm = new WarningsMaker();	
	private static String[] CONSTRUCTTYPES = new String[] {"SQL","Sessions","Autoload","Magic methods",
		"Class presence","Aliasing","Inside function declaration","Use of super global variable",
		"Dynamic call","Dynamic dereference","Dynamic include","Eval","Passing by reference at call side"};
	
	/**
	 * The constructor sets the editor change indicator to false
	 */
	EditorController(){
		editorChanged.set(false);
	}

	/**
	 * {@inheritDoc}
	 * In this implementation, it is checked whether any changes were made in the editor. If so, 
	 * a temporary file with editor content is created and sent to Weverca_Runner to get the
	 * selected constructs occurrences. The result is passed to the WarningsMaker that creates
	 * warnings for these occurrences.
	 * 
	 * @see		ConstructsParser
	 * @see		WarningsMaker
	 */
	@Override
	public void run() {
		
		if (!editorChanged.get()) return; //no change happened
		editorChanged.set(false);
		
		
		File temp = null;
		try {		
			temp = File.createTempFile("fileforweverca", ".php");		
		} catch (IOException e) {
			return;
		} 
		
		String content = changedDocument.get().get();
		String filePath = temp.getAbsolutePath();
		FileWriter fw;
		try {
			fw = new FileWriter(temp.getAbsoluteFile());
			BufferedWriter bw = new BufferedWriter(fw);
			bw.write(content);
			bw.close();
		} catch (IOException e) {
			return;
		}
			
		//not working
		//fdp.saveDocument(monitor, temp, changedDocument.get(), true);
		
		try {
			callWeverca(filePath);
		} catch (AnalyzerNotFoundException e) {
			Messages.incorrectAnalyzerPath();
		}
		temp.delete();
		createWarnings();
	}
	
	/**
	 * This method calls Weverca_Runner to get occurrences of the constructs.
	 * 
	 * @param path	temporary file with editor content
	 * @throws AnalyzerNotFoundException 
	 * @see		ConstructsParser
	 */
	private void callWeverca(String path) throws AnalyzerNotFoundException{
		ArrayList<String> constructTypesToUse = new ArrayList<String>();
		for (int i =0;i<CONSTRUCTTYPES.length;++i){
			if (Activator.getDefault().getPreferenceStore().getBoolean(CONSTRUCTTYPES[i])) constructTypesToUse.add(CONSTRUCTTYPES[i]);
		}
		
		IEditorPart editor = documentsToEditors.get(changedDocument.get());
		
		IFileEditorInput fileEditorInput = (IFileEditorInput)editor.getEditorInput();
		IFile file = fileEditorInput.getFile();
		String ipath = new String(file.getRawLocation().toString());	
		
		try {
			ConstructsInfo constructInfo = ConstructsParser.GetConstructOccurances(path,constructTypesToUse,ipath);
			wm.setConstructInfo(constructInfo);
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	/**
	 * Calls WarningsMaker to add warnings to the editor that has changed.
	 * 
	 * @see		WarningsMaker
	 */
	private void createWarnings(){
		if (!wm.constructInfoIsNull()){
			IEditorPart editor = documentsToEditors.get(changedDocument.get());
        	try {
        		removeAnnotations(editor);
				wm.addWarningsToEditor(editor);
			} catch (CoreException e) {
				e.printStackTrace();
			}
		}
	}
	
	void removeAnnotations(IEditorPart editor){
		wm.removeWarningsFromEditor(editor);
	}
	
}