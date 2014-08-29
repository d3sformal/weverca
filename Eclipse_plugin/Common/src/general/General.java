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


package general;

/**
 * Contains methods used in multiple plug-ins
 * 
 * @author Natalia Tyrpakova
 *
 */
public class General {
	
	/**
	 * Determines whether a file has .php extension.
	 * 
	 * @param fileName		full path to the file
	 * @return				true if the file has .php extension, false otherwise
	 */
	public static boolean isPHPFile(String fileName){
		int i = fileName.lastIndexOf('.');
		if (i > 0) {
		    String extension = fileName.substring(i+1);
		    if (extension.equals("php")) return true;
		}
		return false;
	}
	
	/**
	 * Nicely converts float number to String
	 * 
	 * @param f		float to convert
	 * @return		float as a String
	 */
	public static String floatToString(float f){
		if (f - (int)f != 0){
			return Float.toString(f);
		}
		return Integer.toString((int)f);
		
	}
	
	/**
	 * Converts milliseconds to a nicely formatted String comprising of hours,
	 * minutes, seconds and milliseconds
	 * 
	 * @param ms	milliseconds to convert	
	 * @return		time as String
	 */
	public static String toTime(long ms){
		StringBuilder result = new StringBuilder();
		long rem = ms;
		long hours = ms/3600000;
		if (hours > 0) result.append(hours + "h ");
		rem = rem - hours*3600000;
		long minutes = rem/60000;
		if (minutes > 0) result.append(minutes + "m ");
		rem = rem - minutes*60000;
		long seconds = rem/1000;
		if (seconds > 0) result.append(seconds + "s ");
		rem = rem - seconds*1000;
		if (rem > 0) result.append(rem + "ms ");
		
		result.append("(" + ms + " ms)");
		
		return result.toString();
	}
}