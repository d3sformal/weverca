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

import java.io.IOException;
import java.net.URL;

import org.eclipse.core.runtime.FileLocator;
import org.eclipse.core.runtime.Platform;
import org.eclipse.core.runtime.preferences.AbstractPreferenceInitializer;
import org.eclipse.jface.preference.IPreferenceStore;
import org.osgi.framework.Bundle;

import activator.Activator;

/**
 * Initializer for the Weverca Analyzer preference page
 * 
 * @author Natalia Tyrpakova
 *
 */
public class PreferenceInitializer extends AbstractPreferenceInitializer {
    
	/**
	 * {@inheritDoc}
	 * This implementation sets the plug-in directory as a default path to Weverca analyzer
	 * and sets the hide-error option to false.
	 */
	@Override
    public void initializeDefaultPreferences() {
        IPreferenceStore store = Activator.getDefault().getPreferenceStore();

        try {
			store.setDefault("PATH",getPathToWeverca());
			store.setDefault("HIDE_ERROR",false);
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	/**
	 * Returns the path to the Weverca analyzer located in the same directory as the plug-ins
	 * 
	 * @return	path to the Weverca analyzer
	 * @throws IOException
	 */
	private String getPathToWeverca() throws IOException{
		Bundle bundle = Platform.getBundle("Common");
		URL pluginURL = bundle.getEntry("/");
		String fileURLstring = FileLocator.toFileURL(pluginURL).getPath().toString();
		return fileURLstring + "/Weverca/Weverca.exe";
		//return fileURLstring.replace("Common/","/Weverca/Weverca.exe");*/
		//return "Weverca/Weverca.exe";
	}
}