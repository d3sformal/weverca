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


package preferences;

import org.eclipse.jface.preference.BooleanFieldEditor;
import org.eclipse.jface.preference.FieldEditorPreferencePage;
import org.eclipse.jface.preference.FileFieldEditor;
import org.eclipse.ui.IWorkbench;
import org.eclipse.ui.IWorkbenchPreferencePage;

import activator.Activator;

/**
 * Weverca Analyzer preference page. It allows to define the path to Weverca analyzer 
 * and whether the incorrect path warnings should be displayed.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class WevercaAnalyzerPreferencePage extends FieldEditorPreferencePage implements
		IWorkbenchPreferencePage {

	@Override
	public void init(IWorkbench workbench) {
		 setPreferenceStore(Activator.getDefault().getPreferenceStore());
		 setDescription("A preference page for a path to the Weverca Analyzer.\nInsert a path to Weverca.exe");
	}

	@Override
	protected void createFieldEditors() {
		addField(new FileFieldEditor("PATH", "&Path to analyzer:",
				getFieldEditorParent()));
		addField(new BooleanFieldEditor("HIDE_ERROR","&Do not show incorrect analyzer path warning",
				getFieldEditorParent()));
	}


}