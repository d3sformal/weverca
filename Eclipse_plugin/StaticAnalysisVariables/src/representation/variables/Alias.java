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

/**
 * This class represents an alias. It has a name, must and may AliasTypes,
 * fields and a parent VariableType.
 * 
 * @author Natalia Tyrpakova
 * @see AliasType
 * @see VariableType
 */
public class Alias {
	/**
	 * Alias name
	 */
	public String name = "";
	/**
	 * AliasType holding all the must aliases - aliases that must be true.
	 * @see AliasType
	 */
	public AliasType must = new AliasType("Must",this);
	/**
	 * AliasType holding all the may aliases - aliases that may be true.
	 * @see AliasType
	 */
	public AliasType may = new AliasType("May",this);
	/**
	 * VariableType instance that holds this Alias
	 * @see VariableType
	 */
	public VariableType parentType = null;
	
	/**
	 * The constructor sets the name.
	 * 
	 * @param name		alias name
	 */
	public Alias(String name){
		this.name = name;
	}

}