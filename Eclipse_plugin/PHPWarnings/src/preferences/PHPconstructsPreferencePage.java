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
import org.eclipse.ui.IWorkbench;
import org.eclipse.ui.IWorkbenchPreferencePage;

import warnings.Activator;

/**
 * Contribution to the preference page extension. Provides possibility to select
 * which construct types will throw the warnings.
 * 
 * @author Natalia Tyrpakova
 * @see IWorkbenchPreferencePage
 */
public class PHPconstructsPreferencePage extends FieldEditorPreferencePage implements
		IWorkbenchPreferencePage {
	private static String[] GENERALCOSTRUCTTYPES = new String[] {"SQL","Sessions","Autoload","Magic methods","Class presence","Aliasing","Inside function declaration","Use of super global variable"};
	private static String[] DYNAMICCONSTRUCTTYPES = new String[] {"Dynamic call","Dynamic dereference","Dynamic include","Eval"};
	private static String[] PROBLEMATICCONSTRUCTTYPES = new String[] {"Passing by reference at call side"};
	
	@Override
	public void init(IWorkbench workbench) {
		 setPreferenceStore(Activator.getDefault().getPreferenceStore());
		
	}

	/**
	 * {@inheritDoc}
	 * 
	 * In this implementation a field is created for each construct type.
	 */
	@Override
	protected void createFieldEditors() {
			for (int i = 0; i<GENERALCOSTRUCTTYPES.length;++i){
				addField(new BooleanFieldEditor(GENERALCOSTRUCTTYPES[i],
						GENERALCOSTRUCTTYPES[i], getFieldEditorParent()));
			}
			for (int i = 0; i<DYNAMICCONSTRUCTTYPES.length;++i){
				addField(new BooleanFieldEditor(DYNAMICCONSTRUCTTYPES[i],
						DYNAMICCONSTRUCTTYPES[i], getFieldEditorParent()));
			}
			for (int i = 0; i<PROBLEMATICCONSTRUCTTYPES.length;++i){
				addField(new BooleanFieldEditor(PROBLEMATICCONSTRUCTTYPES[i],
						PROBLEMATICCONSTRUCTTYPES[i], getFieldEditorParent()));
			}
		
	}

	
}