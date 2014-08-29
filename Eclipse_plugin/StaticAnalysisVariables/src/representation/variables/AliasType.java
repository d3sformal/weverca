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


package representation.variables;

import java.util.ArrayList;

/**
 * This class stores the aliased variables - must or may, depending on the type. 
 * It is necessary for the TreeViewer to be stored this way.
 * 
 * @author Natalia Tyrpakova
 *
 */
public class AliasType {
	/**
	 * Alias type - either "Must" or "May"
	 */
	public String type = "";
	/**
	 * List of aliased variables
	 */
	public ArrayList<Variable> variables = new ArrayList<Variable>();
	/**
	 * Alias instance that holds this AliasType.
	 * @see Alias
	 */
	public Alias parentAlias;
	
	/**
	 * The constructor sets the type and its parent Alias
	 * 
	 * @param type		type of this AliasType - should be MUST or MAY
	 * @param parent	parent Alias
	 */
	AliasType(String type,Alias parent){
		this.type = type;
		this.parentAlias = parent;
	}
	
	

}