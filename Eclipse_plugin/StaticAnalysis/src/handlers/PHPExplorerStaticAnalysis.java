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


package handlers;

import java.util.ArrayList;

import org.eclipse.core.commands.AbstractHandler;
import org.eclipse.core.commands.ExecutionEvent;
import org.eclipse.core.commands.ExecutionException;

import fileselectors.FilesFromExplorer;
import staticanalysis.Analyzer;

/**
 * Handler for the PHP Explorer view pop-up. When called, 
 * it gets the selected files and calls the analyzer to analyze them statically
 * 
 * @author 		Natalia Tyrpakova
 * @see 		AbstractHandler
 */
public class PHPExplorerStaticAnalysis extends AbstractHandler {

	/**
	 * {@inheritDoc}
	 * This implementation gets the selected PHP files from the PHP Explorer. 
	 * Then the files are analyzed statically and the results are displayed
	 */
	@Override
	public Object execute(ExecutionEvent event) throws ExecutionException {
		ArrayList<String> filesToAnalyze = FilesFromExplorer.getFilesToAnalyze("org.eclipse.php.ui.explorer");
		Analyzer analyzer = new Analyzer();
		analyzer.analyzeAndShowResult(filesToAnalyze);		
		return null;
	}

}