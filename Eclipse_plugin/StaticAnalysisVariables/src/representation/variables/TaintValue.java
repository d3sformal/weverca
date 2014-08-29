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
 * Stores one taint value of variable
 * 
 * @author Natalia Tyrpakova
 *
 */
public class TaintValue {
	/**
	 * Taint value - either SQL dirty, HTML dirty or File path dirty
	 */
	public String taint;
	/**
	 * Taint instance that holds this TaintValue
	 * @see Taint
	 */
	public Taint parent;
	/**
	 * Priority of this taint value
	 * @see Priority
	 */
	public Priority priority;
	
	/**
	 * The constructor initializes the taint value and parent
	 * @param t		taint
	 * @param p		parent
	 */
	TaintValue(String t,char priority, Taint p){
		taint = t;
		parent = p;
		switch (priority) {
		case 'H' : 
			this.priority = Priority.H;
			break;
		case 'L' : 
			this.priority = Priority.L;
			break;
		default :
			this.priority = Priority.N;
			break;	
		}
	}
	
	@Override
	public boolean equals(Object o){
		if (o == null) return false;
		if (!(o instanceof TaintValue)) return false;
		return ((TaintValue)o).taint.equals(taint);
	}
	
	/**
	 * Enum representing the priority. 
	 * The priority can be H - high, L - low or N - undefined
	 * 
	 * @author Natalia Tyrpakova
	 *
	 */
	public enum Priority {H,L,N};
}