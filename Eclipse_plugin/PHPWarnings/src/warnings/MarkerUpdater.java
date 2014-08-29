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

import org.eclipse.core.resources.IMarker;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.text.Position;
import org.eclipse.ui.texteditor.IMarkerUpdater;

/**
 * Class responsible for saving changes to markers.
 * @author Natalia Tyrpakova
 *
 */
public class MarkerUpdater implements IMarkerUpdater{

	@Override
	public String getMarkerType() {
		return "PHP_warnings_constructMarker";
	}

	@Override
	public String[] getAttribute() {
		return null;
	}

	@Override
	public boolean updateMarker(IMarker marker, IDocument document,Position position) {
		try {
            int start = position.getOffset();
              int end = position.getOffset() + position.getLength();
              marker.setAttribute(IMarker.CHAR_START, start);
              marker.setAttribute(IMarker.CHAR_END, end);
              return true;
        } catch (CoreException e) {
              return false;
        }
	}

}