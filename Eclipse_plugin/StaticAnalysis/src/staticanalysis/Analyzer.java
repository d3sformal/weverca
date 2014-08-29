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


package staticanalysis;

import exceptions.AnalyzerNotFoundException;
import general.Messages;

import java.io.IOException;
import java.util.ArrayList;

import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.core.runtime.IStatus;
import org.eclipse.core.runtime.Status;
import org.eclipse.core.runtime.jobs.Job;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.widgets.Display;
import org.eclipse.ui.IViewPart;
import org.eclipse.ui.PartInitException;
import org.eclipse.ui.PlatformUI;

import staticanalysisvariables.VariablesDisplayer;
import staticanalysiswarnings.AnalysisWarningsDisplayer;
import representation.WarningsParser;
import representation.VariablesParser;
import staticanalysiswarnings.WarningsHighlighter;
import views.Overview;
import wevercarunner.StaticAnalysisParser;

/**
 * This class serves as a connection to the StaticAnalysisWarnings and
 * StaticAnalysisVariables plug-ins.
 * 
 * @author Natalia Tyrpakova
 * 
 */
public class Analyzer {
	private StaticAnalysis_Runner runner;
	private Job job;
	private Overview view;
	private boolean stopped = false;

	/**
	 * Gets the list of file paths, analyzes them in the background and sends
	 * the result to the StaticAnalysisWarnings and StaticAnalysisVariables
	 * plug-ins.
	 * 
	 * @param filesToAnalyze
	 *            files to be analyzed
	 */
	public void analyzeAndShowResult(final ArrayList<String> filesToAnalyze) {
		final long startTime = System.currentTimeMillis();
		// open the overview view
		try {
			IViewPart resultView = PlatformUI.getWorkbench()
					.getActiveWorkbenchWindow().getActivePage()
					.findView("StaticAnalysis.overview");
			if (resultView == null)
				resultView = PlatformUI.getWorkbench()
						.getActiveWorkbenchWindow().getActivePage()
						.showView("StaticAnalysis.overview");
			view = ((Overview) resultView);
			view.analysisRunning();
			PlatformUI.getWorkbench().getActiveWorkbenchWindow()
					.getActivePage().activate(resultView);
			view.stopAnalysisButton
					.addSelectionListener(new SelectionAdapter() {
						@Override
						public void widgetSelected(SelectionEvent e) {
							stopAnalysis(startTime);
						}
					});
		} catch (PartInitException e) {
			e.printStackTrace();
		}

		if (filesToAnalyze != null) {

			runner = new StaticAnalysis_Runner();

			final WarningsParser warningsParser = new WarningsParser();
			final VariablesParser variablesParser = new VariablesParser();

			job = new Job("calling analyzer") {
				protected IStatus run(IProgressMonitor monitor) {
					try {
						StaticAnalysisParser parsers[] = { warningsParser,
								variablesParser };
						runner.computeAnalysisResult(filesToAnalyze, parsers);
					} catch (IOException e1) {
						e1.printStackTrace();
					} catch (AnalyzerNotFoundException e) {
						return new Status(Status.ERROR, "Metrics",
								Messages.INCORRECT_ANALYZER_PATH_WARNING);
					}
					Display.getDefault().asyncExec(new Runnable() {
						public void run() {
							AnalysisWarningsDisplayer
									.displayWarnings(warningsParser
											.getWarnings());
							VariablesDisplayer.displayVariables(
									variablesParser.getFiles(),
									variablesParser.unreachablePoints);
							long stopTime = System.currentTimeMillis();
							long elapsedTime = stopTime - startTime;
							if (!stopped)
								view.done(elapsedTime, runner.warningsNum,
										runner.warningsFirstPhaseNum,
										runner.warningsSecondPhaseNum,
										runner.wevercaTime,
										runner.firstPhaseTime,
										runner.secondPhaseTime,
										runner.numberOfPPoints);
						}
					});
					return Status.OK_STATUS;
				}
			};
			job.schedule();
		}
	}

	/**
	 * Stops the static analysis
	 */
	private void stopAnalysis(long startTime) {
		stopped = true;
		runner.stopRunner();
		long stopTime = System.currentTimeMillis();
		long elapsedTime = stopTime - startTime;
		view.stopped(elapsedTime);
	}

}