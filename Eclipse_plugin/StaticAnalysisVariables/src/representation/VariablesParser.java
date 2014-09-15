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


package representation;

import java.util.ArrayList;
import java.util.Iterator;

import representation.highlevel.Context;
import representation.highlevel.File;
import representation.highlevel.Point;
import representation.highlevel.ProgramPoint;
import representation.variables.Alias;
import representation.variables.Variable;
import representation.variables.VariableType;
import wevercarunner.StaticAnalysisParser;
import wevercarunner.StaticAnalysisParser.InputParts;
import callstacks.Call;
import callstacks.CallStringParser;


/**
 * This class parses the static analysis result into the program points and their variables
 * 
 * @author Natalia Tyrpakova
 * @author David Hauzar
 *
 */
public class VariablesParser implements StaticAnalysisParser {
	private ArrayList<File> files = new ArrayList<File>();
	private boolean warnings = true;
	private boolean before = true;
	private Point point = null;
	private Context context = null;
	private String script = null;
	private ArrayList<Call> call = null;
	private String variableType = "";
	
	private boolean lastContextChecked = false;
	
	
	/**
	 * List of ProgramPoints that are unreachable
	 */
	public ArrayList<ProgramPoint> unreachablePoints = new ArrayList<ProgramPoint>();
	
	/**
	 * Returns a list of Files to be shown in the VariablesView.
	 * Should be called after parsing is finished.
	 * @return			list of Files containing all the ProgramPoints
	 */
	public ArrayList<File> getFiles(){
		if (!lastContextChecked) {
			// check the last context
			//checkAliasFields(context,before);
			checkFieldsAndArrays(context,before);
			
			lastContextChecked = true;
		}
		
		return files;
	}
	
	public void parseLine(String line) {
		if (line.equals("Variables:")) {
			warnings = false;
			return;
		}
		
		if (line.equals("Analysis warnings:")) {
			warnings = true;
			return;
		}
		
		if (line.length()>10 && line.substring(0,10).equals("File path:")){ 
			File newFile = new File(line.substring(11,line.length()));
			files.add(newFile);
			return;
		}
		
		//warnings are processed
		if (warnings) return;
		
		if (line.equals("")) return;
		
		if (line.length()>15 && line.substring(0,15).equals("Point position:")){
			point = new Point(line);	
			return;
		}
		
		if (line.length()>14 && line.substring(0,14).equals("OwningScript: ")){
			script = line.substring(14,line.length());
			return;
		}
		
		if (line.equals("Point information:")){
			if (context != null){ // do the final context checks
				//checkAliasFields(context,before);
				checkFieldsAndArrays(context,before);				
			}
			if (point == null) point = new Point(-1,-1);
			if (script == null) script = new String("");
			context = null;
			before = true;
			
			ProgramPoint existingPPoint = findPPoint(files,script,point);
			if (existingPPoint != null){
				context = findContext(existingPPoint,call);
				if (context != null) before = false;
				else {
					context = new Context(call,existingPPoint);
					existingPPoint.contexts.add(context);
				}
			}
			else{ //create new program point
				ProgramPoint newPoint = new ProgramPoint(script,point);
				boolean found = false;
				for (File f : files){
					if (f.filePath.equals(newPoint.scriptName)){
						newPoint.parentFile = f;
						f.add(newPoint);
						found = true;
						break;
					}
				}
				if (!found){ //create new file
					File newFile = new File(newPoint.scriptName);
					newPoint.parentFile = newFile;
					newFile.add(newPoint);
					files.add(newFile);
				}
				// create new context inside of this ProgramPoint
				context = new Context(call,newPoint);
				newPoint.contexts.add(context);
			}
			for (Call c : context.calledFrom){
				c.parent = context;
			}	
			point = null;
			script = null;	
			call = null;
			return;
		}
		
		if (line.contains("Fixpoint iterations=")){
			context.fixpointIterations = Integer.parseInt(line.substring(20));
			
		}
		
		//dead code
		if (line.equals("Dead code")){
			context.deadCode = true;
			if (!unreachablePoints.contains(context.parent))
				unreachablePoints.add(context.parent);
			return;
		}
		
		// call string
		if (line.length() > 2 && line.substring(0, 2).equals("->")){
			call = CallStringParser.parse(line);
			return;
		}
		
		//variable type
		if (line.length() > 1 && line.charAt(0) == '=') {
			variableType = line;
			return;
		}
		
		//variable
		/* possible variable patterns:
		 * contains -> (fields)
		 * first character is $
		 * character after . is $ (numbered variables)
		 * starts with "TEMP" or "CTRL"
		 * contains [ and ]
		 * */
		if (line.contains("->") || 
			(line.length() > 1 && (line.charAt(0) == '$' 
				|| (line.indexOf('.')+1 < line.length() && line.charAt(line.indexOf('.')+1) == '$'))) || 
			(line.length()>4 && (line.substring(0,4).equals("TEMP") 
				|| line.substring(0,4).equals("CTRL"))) ||
			line.contains("[") && line.contains("]")){
			
			int type = getVariableType(variableType);
			if (type != 2){// not alias
				Variable newVar = new Variable(getVariableName(line));
				ArrayList<String> values = parseValues(line);
				ArrayList<String> taint = parseTaint(line);
				for (String value : values){
					newVar.addValue(context, value);
				}	
				if (!taint.isEmpty()) newVar.addTaint(taint);
				if (type < 2 || type > 4){ // global/local variables or global/local controls
					if (before) context.before[type].addVariable(newVar);
					else context.after[type].addVariable(newVar);
					return;
				}
				if (type == 3){//array
					if (tryInsertArray(context, newVar, before)) return;
					if (before) context.before[type].addVariable(newVar);
					else context.after[type].addVariable(newVar);
					return;
				}
				if (type == 4){ //field
					if (tryInsertField(context, newVar, before)) return;
					if (before) context.before[type].addVariable(newVar);
					else context.after[type].addVariable(newVar);
					return;
				}
			}
			if (type == 2){ //alias
				Alias newAlias = new Alias(getVariableName(line));
				newAlias.must.variables = parseMust(context,newAlias.name,line,before);
				newAlias.may.variables = parseMay(context,newAlias.name,line,before);
				if (before) context.before[type].addAlias(newAlias);
				else context.after[type].addAlias(newAlias);
			}
			return;
		}	
	}
	
	/**
	 * Tries to find a Context in a ProgramPoint with defined call stack.
	 * Returns null if no such Context has been found.
	 * 
	 * @param pPoint	ProgramPoint to search in
	 * @param call		call stack to search for
	 * @return			corresponding Context or null
	 * @see ProgramPoint
	 * @see Context
	 */
	private static Context findContext(ProgramPoint pPoint,ArrayList<Call> call) {
		for (Context con : pPoint.contexts){
			if (call == null) call = new ArrayList<Call>();
			if (call.size() != con.calledFrom.size()) continue;
			boolean same = true;
			for (Call c : call){
				if (!con.calledFrom.contains(c)){
					same = false;
					break;
				}
			}
			if (same) return con;
		}
		return null;
	}

	/**
	 * Tries to find a ProgramPoint in a list of Files for defined script name and position. 
	 * Returns null if no such ProgramPoint has been found.
	 *
	 * @param files		list of Files
	 * @param script	ProgramPoint's file path
	 * @param point		ProgramPoint's position
	 * @return			corresponding ProgramPoint or null
	 * @see ProgramPoint
	 * @see File
	 */
	private static ProgramPoint findPPoint(ArrayList<File> files,String script, Point point) {
		for (File f : files){ 
			if (f.filePath.equals(script)){
				return f.programPoints.get(point.firstLine);
			}					
		}
		return null;
	}

	/**
	 * Gets a variable name from the analyzer line.
	 * 
	 * @param line	line from the analyzer
	 * @return	variable name
	 */
	private static String getVariableName(String line){
		return line.substring(0,line.indexOf(':'));
	}
	
	/**
	 * Gets a variable type from the analyzer line.
	 * 
	 * @param type	line from the analyzer
	 * @return	variable type
	 */
	private static int getVariableType(String type){
		switch(type){
		case "===GLOBALS===" : return 0;
		case "===LOCALS===" : return 1;			
		case "===ALIASES===" : return 2;
		case "===ARRAYS===" : return 3;
		case "===FIELDS===" : return 4;
		case "===GLOBAL CONTROLS===" : return 5;		
		case "===LOCAL CONTROLS===" : return 6;			
		}
		return -1;
	}
	
	/**
	 * Parses the variable values from the analyzer output.
	 * 
	 * @param line		line from analyzer that contains a variable
	 * @return			list of variable values
	 */
	private static ArrayList<String> parseValues(String line){
		ArrayList<String> result = new ArrayList<String>();
		String delims = "[\\(\\)]";
		line = changeInsideParentheses(line);
		String[] tokens = line.split(delims);
		for (int i = 1; i<tokens.length; i++ ){
			if (tokens[i].equals(" INFO: Values: ")){
				return result;
			}
			if (i%2 == 1) result.add(tokens[i]);			
		}
		return result;
	}
	
	/**
	 * Change parentheses in string inside '' to square brackets
	 * 
	 * @param s		string to change parentheses in
	 * @return		string with changed parentheses
	 */
	private static String changeInsideParentheses(String s){
		StringBuilder sb = new StringBuilder(s);
		boolean[] indexesToChange1 = new boolean[s.length()];
		boolean[] indexesToChange2 = new boolean[s.length()];
		boolean value = false;
		
		for (int i = 0; i < sb.length(); i++){
			if (sb.charAt(i) == '\''){
				value ^= true;
			}
			else if (sb.charAt(i) == '(' && value) {
				indexesToChange1[i] = true;
			}
			else if (sb.charAt(i) == ')' && value) {
				indexesToChange2[i] = true;
			}
		}
		
		for (int i = indexesToChange1.length-1; i>=0; i--){
			if (indexesToChange1[i]) sb.setCharAt(i, '[');
			else if (indexesToChange2[i]) sb.setCharAt(i, ']');
		}
		
		return sb.toString();
	}
	
	/**
	 * Parses the variable taint information from the analyzer output.
	 * 
	 * @param 	line from analyzer that contains a variable
	 * @return	list of taint information
	 */
	private static ArrayList<String> parseTaint(String line){
		ArrayList<String> result = new ArrayList<String>();
		String delims = "[\\(\\)]";
		String[] tokens = line.split(delims);
		for (int i = 0; i<tokens.length; i++ ){
			if (tokens[i].equals(" INFO: Values: ")){
				String[] taintTokens = tokens[i+1].split(",");
				for (String taint : taintTokens){
					if (!taint.isEmpty()) result.add(taint);
				}
			}			
		}
		return result;
	}
	
	/**
     * Tries to find variables that contain values corresponding to objects in that the field is defined.
     * Inserts the field to these varia
     * 
     * @param c         context to search for a variable
     * @param field     field that is being processed
     * @param before    which variables to search in
     * @return          true if parent has been found, false otherwise
     */
    private static boolean tryInsertField(Context c, Variable field, boolean before){
        int index = field.name.indexOf("->");
        if (index != -1) {
            int uid = Integer.parseInt(field.name.substring(0,index));
            Iterable<Variable> variables = c.getVariablesContainingUID(uid);
            if (! variables.iterator().hasNext()) return false;
            for (Variable v : c.getVariablesContainingUID(uid)) {
                v.fields.addVariable(field);
            }
            return true;
        }
        
        return false;
    }

    private static boolean tryInsertArray(Context c, Variable field, boolean before){
    	int index = field.name.indexOf('[');
		if (index != -1){
			int uid = Integer.parseInt(field.name.substring(0,index));
			Iterable<Variable> variables = c.getVariablesContainingUID(uid);
            if (! variables.iterator().hasNext()) return false;
            for (Variable v : c.getVariablesContainingUID(uid)) {
                v.fields.addVariable(field);
            }
			return true;
		}
		
		return false;
    }
	
	/**
	 * Tries to find a corresponding parent variable for an array field
	 * 
	 * @param c			context to search for a variable
	 * @param field		field that is being processed
	 * @param before	which variables to search in
	 * @return			true if parent has been found, false otherwise
	 */
	private static boolean tryInsertArrayOld(Context c, Variable field, boolean before){
		if (before){
			for (VariableType vt : c.before){
				if (tryInsertArrayOld(vt,field)) return true;
			}
		}
		if (!before){
			for (VariableType vt : c.after){
				if (tryInsertArrayOld(vt,field)) return true;
			}
		}
		return false;	
	}
	
	/**
	 * Recursively tries to find a corresponding parent variable for an array field 
	 * in a specific VariableType
	 * 
	 * @param type	VariableType to search in
	 * @param field	field that is being processed
	 * @return	true if parent has been found, false otherwise
	 */
	private static boolean tryInsertArrayOld(VariableType type, Variable field){
		int index = field.name.lastIndexOf('[');
		if (index != -1){
			String parent = field.name.substring(0,index);
			for (Variable v : type.variables){
				if (v.name.equals(parent)){
					v.fields.addVariable(field);
					return true;
				}
				if (tryInsertArrayOld(v.fields, field)) return true;
			}
		}
		return false;	
	}
	
	/**
	 * Parses the must aliases from the line from analyzer with alias.
	 * 
	 * @param c				Context that is being processed
	 * @param aliasName		name of the alias
	 * @param line			line from analyzer
	 * @param before		boolean determining which variables to search in
	 * @return				list of Variables that belong to the alias
	 */
	private static ArrayList<Variable> parseMust(Context c, String aliasName, String line, boolean before){
		ArrayList<Variable> result = new ArrayList<Variable>();
		String values = line.substring(line.indexOf('{'),line.indexOf('|'));
		values = values.substring(values.indexOf(':')+1);
		String delims = "[ ,]";
		String[] tokens = values.split(delims);
		for (String token : tokens){
			if (token.length()>0 && !aliasName.equals(token)){
				if (before){
					for (VariableType type : c.before){
						Variable foundVar = findAlias(type, token);
						if (foundVar != null) result.add(foundVar);
					}
				}
				else{
					for (VariableType type : c.after){
						Variable foundVar = findAlias(type, token);
						if (foundVar != null) result.add(foundVar);
					}
				}	
			}
		}
		return result;
	}
	
	/**
	 * Parses the may aliases from the line from analyzer with alias.
	 * 
	 * @param c				Context that is being processed
	 * @param aliasName		name of the alias
	 * @param line			line from analyzer
	 * @param before		boolean determining which variables to search in
	 * @return				list of Variables that belong to the alias
	 */
	private static ArrayList<Variable> parseMay(Context c,String aliasName, String line, boolean before){
		ArrayList<Variable> result = new ArrayList<Variable>();
		String values = line.substring(line.indexOf('|'),line.indexOf('}'));
		if (values.indexOf(':') == -1) return result;
		values = values.substring(values.indexOf(':'));
		String delims = "[ ,]";
		String[] tokens = values.split(delims);
		for (String token : tokens){
			if (token.length()>0 && !aliasName.equals(token)){
				if (before){
					for (VariableType type : c.before){
						Variable foundVar = findAlias(type, token);
						if (foundVar != null) result.add(foundVar);
					}
				}
				else{
					for (VariableType type : c.after){
						Variable foundVar = findAlias(type, token);
						if (foundVar != null) result.add(foundVar);
					}
				}	
			}
		}
		return result;
	}
	
	/**
	 * Tries to find a variable for an alias.
	 * 
	 * @param aliasName 	name to search for
	 * @return				variable for the alias or null if it has not been found
	 */
	private static Variable findAlias(VariableType type,String aliasName){
		for (Variable var: type.variables){
			if (var.name.equals(aliasName)) return var;
			for (Variable childVar : var.fields.variables){
				Variable v = findAlias(childVar.fields,aliasName);
				if (v != null) return v;
			}
		}		
		return null;
	}
	
	/**
	 * This function checks whether the fields with no parent variable
	 * can be connected with one of the aliases. The aliases are processed after the fields,
	 * so it is necessary to check this additionally.
	 * 
	 * @param c			Context holding the fields to process
	 * @param before	determines which variables to process
	 */
	private static void checkAliasFields(Context c, boolean before){
		if (c == null) return;
		if (before){
			Iterator<Variable> it = c.before[4].variables.iterator();
			while (it.hasNext()){
				Variable field = it.next();
				//if (tryInsertField(c.before[2],field)) it.remove();
			}
			it = c.before[3].variables.iterator();
			while (it.hasNext()){
				Variable array = it.next();
				//if (tryInsertArray(c.before[2],array)) it.remove();
			}
		}
		else{
			Iterator<Variable> it = c.after[4].variables.iterator();
			while (it.hasNext()){
				Variable field = it.next();
				//if (tryInsertField(c.after[2],field)) it.remove();
			}
			it = c.after[3].variables.iterator();
			while (it.hasNext()){
				Variable array = it.next();
				//if (tryInsertArray(c.after[2],array)) it.remove();
			}
		}
	}
	
	/**
	 * This function checks whether the arrays with no parent variable
	 * can be connected with one of the fields. The fields are processed after the arrays,
	 * so it is necessary to check this additionally.
	 * 
	 * @param c			Context holding the fields to process
	 * @param before	determines which variables to process
	 */
	private static void checkFieldsAndArrays(Context c, boolean before){
		if (c == null) return;
		if (before){
			Iterator<Variable> it = c.before[3].variables.iterator();
			while (it.hasNext()){
				Variable array = it.next();
				if (tryInsertArray(c,array,before)) it.remove();
			}
		}
		else{
			Iterator<Variable> it = c.after[3].variables.iterator();
			while (it.hasNext()){
				Variable array = it.next();
				if (tryInsertArray(c,array,before)) it.remove();
			}
		}
	}

	@Override
	public boolean parsingShouldEnd() {
		return false;
	}
	
	@Override
	public InputParts partToParse() {
		return InputParts.VARIABLES;
	}

}