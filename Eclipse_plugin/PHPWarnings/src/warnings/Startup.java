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
import java.util.ArrayList;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

import org.eclipse.jface.text.DocumentEvent;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.text.IDocumentListener;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IPartListener;
import org.eclipse.ui.IStartup;
import org.eclipse.ui.IWorkbench;
import org.eclipse.ui.IWorkbenchPage;
import org.eclipse.ui.IWorkbenchPart;
import org.eclipse.ui.IWorkbenchWindow;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.texteditor.IDocumentProvider;
import org.eclipse.ui.texteditor.ITextEditor;


/**
 * This class is activated after the Workbench initializes. It schedules an EditorController
 * to run every second and check whether there is any change in an editor. If so, it adds or updates 
 * the construct warnings in this editor.
 * 
 * @author Natalia Tyrpakova
 * @see		EditorController
 */
public class Startup implements IStartup {
	private ArrayList<IDocument> documents = new ArrayList<IDocument>();
	private EditorController controller = new EditorController();

	
	
	/**
	 * {@inheritDoc}	
	 * In this implementation, this method adds a listener to IEditorPart opening 
	 * and schedules the EditorController to repeatedly add warnings.
	 * 
	 * @see		EditorController
	 */
	@Override
	public void earlyStartup() {	
		
		final IWorkbench workbench = PlatformUI.getWorkbench();
		 workbench.getDisplay().asyncExec(new Runnable() {
		   public void run() {
		     IWorkbenchWindow window = workbench.getActiveWorkbenchWindow();
		     if (window != null) {
		      
				 IWorkbenchPage[] pages = window.getPages();
				 for (int i = 0; i < pages.length; i++) {
					 IWorkbenchPage p = pages[i];
					 p.addPartListener(new IPartListener(){

						@Override
						public void partActivated(IWorkbenchPart part) {
							if (part instanceof IEditorPart) {
								passToController((IEditorPart)part);
							}
						}
						@Override
						public void partClosed(IWorkbenchPart part) {
							if (part instanceof IEditorPart){
								IEditorPart editor = (IEditorPart)part;
								if (editor instanceof ITextEditor){
						 			IDocumentProvider idp = ((ITextEditor) editor).getDocumentProvider();
								    IDocument document = idp.getDocument(editor.getEditorInput());
									if (documents.contains(document)) documents.remove(document);
								    if (controller.documentsToEditors.containsKey(document)) controller.documentsToEditors.remove(document);
								    controller.removeAnnotations(editor);
								}
							}
						}
						@Override
						public void partOpened(IWorkbenchPart part) {
							if (part instanceof IEditorPart) passToController((IEditorPart)part);			
						}
						@Override
						public void partBroughtToTop(IWorkbenchPart part) {}
						@Override
						public void partDeactivated(IWorkbenchPart part) {}
					 });
				 }
				
		     }
		     IEditorPart activeEditor = window.getActivePage().getActiveEditor();
		     if (activeEditor != null) passToController(activeEditor);
		     
		     ScheduledExecutorService executor = Executors.newScheduledThreadPool(1);
			 executor.scheduleAtFixedRate(controller, 0, 1, TimeUnit.SECONDS);
			 
		   
		   }
		   
		 });
		 
		
		  	 
	}
	
	/**
	 * Adds listener to an editor's document to listen for changes and passes
	 * the document to the EditorController to create the warnings immediately.
	 * @param part	IEditorPart to process
	 * 
	 * @see		EditorController
	 * @see		IEditorPart
	 */
	private void passToController(IEditorPart part){
			IEditorPart editor = (IEditorPart)part;
			
 			if (!(editor instanceof ITextEditor)) return; 
 			IDocumentProvider idp = ((ITextEditor) editor).getDocumentProvider();
		    IDocument document = idp.getDocument(editor.getEditorInput());
		    if (!documents.contains(document)) {
		    	documents.add(document);
		    }
		    if (!controller.documentsToEditors.containsKey(document)) {
		    	controller.documentsToEditors.put(document, editor);
		    }
		    document.addDocumentListener(new IDocumentListener() {

			      @Override
			      public void documentChanged(DocumentEvent event) 
			      {	
			    	  controller.changedDocument.set(event.getDocument());
			    	  controller.editorChanged.set(true);
			      }		        
			      @Override
			      public void documentAboutToBeChanged(DocumentEvent event) {						
			      }
			    });
		    controller.changedDocument.set(document);
	    	controller.editorChanged.set(true);	
	    	controller.run();
	}

}