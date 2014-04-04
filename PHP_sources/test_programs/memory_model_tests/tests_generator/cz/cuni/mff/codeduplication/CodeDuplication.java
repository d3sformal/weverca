package cz.cuni.mff.codeduplication;

public class CodeDuplication {
	
	/**
	 * 
	 * @param args
	 * 
	 * 
	 * 
	 */
	public static void main(String[] args) {
		String code = "$alias = array();"+"\n"+
			"$alias2 = 0;"+"\n"+
			"$alias3 = 1;"+"\n"+
			"if (?) {"+"\n"+
			"	$arr[?] = &$alias;"+"\n"+
			"	$t = $arr[1];"+"\n"+
			"	$t[2] = 2;"+"\n"+
			"	$arr[1][2] = 3;"+"\n"+
			"	$arr[1][3] = 4;"+"\n"+
			"	$arr[2][3] = 5;"+"\n"+
		"} else {"+"\n"+
		"	$arr[?][2] = 6;"+"\n"+
		"	$arr[1][?] = 7;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
//		"	1;"+"\n"+
		"}"+"\n"+
		"$arr[2][1] = &$alias2; // {$arr[2][1], $alias[1], $alias2}"+"\n"+
		"$arr[2] = &$alias3; // {$arr[2], $alias3}, {$alias[1], $alias2}"+"\n"+
		"$arr2 = $arr;"+"\n"+
		"$arr2[2] = 8; // updates also $arr[2] and $alias3"+"\n"+
		"$arr2[3] = 9; // updates also $arr[3] and $alias"
		 +"\n$arr[?] = $arr2;";
		
		//code = "$x = $y;";
		int numDuplications = Integer.parseInt(args[1]);
		if (args[0].equals("code_n")) {
			System.out.print(createFinalCode(duplicateCodeHelper(code, numDuplications)));
		} else {
			System.out.print(createFinalCode(duplicateCode(code, numDuplications)));
		}
	}
	
	private static String createFinalCode(String code) {
		return "<?php" + "\n" +
				"/*" + "\n"+
				"Values:"+"\n"+
				" $alias = {9, array}"+"\n"+
				 " "+"\n"+
				 " $alias[1] = {undefined, 1} alias $alias2"+"\n"+
				 " $alias[2] = {undefined, 3}"+"\n"+
				 " $alias[3] = {}"+"\n"+
				 " "+"\n"+
				 " $alias2 = {1}"+"\n"+
				 " "+"\n"+
				 " $alias3 = {8}"+"\n"+
				 " "+"\n"+
				 " $arr = {array}"+"\n"+
				 " "+"\n"+
				 " $arr[?] = {undefined, array}"+"\n"+
				 " $arr[1] = {array}"+"\n"+
				 " $arr[2] = {8}"+"\n"+
				 " $arr[3] = {undefined, array, 9} Even if it is may-alias of $alias, it does not have indexes $arr[3][1], $arr[3][2], $arr[3][3]"+"\n"+
				 " "+"\n"+
				 " $arr[?][2] = {undefined, 6}"+"\n"+
				 " $arr[1][?] = {undefined, 7}"+"\n"+
				 " $arr[1][2] = {undefined, 3, 6, 7}"+"\n"+
				 " $arr[1][3] = {undefined, 4, 7}"+"\n"+
				 ""+"\n"+ 
				 " $arr[2][1] = {1} Before the statement $arr[2] = &$alias3; Empty after this statement."+"\n"+
				 " $arr[2][2] = {undefined, 6} Before the statement $arr[2] = &$alias3; Empty after this statement."+"\n"+
				 " $arr[2][3] = {undefined, 5} Before the statement $arr[2] = &$alias3; Empty after this statement."+"\n"+
				 " "+"\n"+
				 " $arr2 similar to arr"+"\n"+
				 " "+"\n"+
				 ""+"\n"+ 
				 " Aliases:"+"\n"+
				 "*/" + "\n"+
				code.replace("?", "$_POST[1]") + 
				"\n" + "?>";
	}
	
	private static String duplicateCode(String code, int numDuplications) 
	{
		code = duplicateCodeHelper(code, numDuplications);
		return "if (?) {" + "\n	" + code.replace("\n", "\n"+"	") + "\n" + "}" + " else {" + "\n	" + code.replace("\n", "\n"+"	") + "\n" + "}";	}
	
	private static String duplicateCodeHelper(String code, int numDuplications) {
		for (int i = 1; i < numDuplications; i++) {
			code += "\n" +
					addPrefixToVars(code, createPrefix(i));
		}
		
		return code;
	}
	
	private static String createPrefix(int length) {
		String str = "";
		for (int i = 0; i <= length; i++) {
			str += "a";
		}
		return str;
	}
	
	private static String duplicateCodeOld(String code, int numDuplications) {
		if (numDuplications == 0) return code;
		
		code = "if (?) {" + "\n" + addPrefixToVars(code, "a") + "\n" + "}" + "else {" + "\n" + addPrefixToVars(code, "b") + "\n" + "}";   
		
		return duplicateCode(code, numDuplications-1);
	}
	
	private static String addPrefixToVars(String code, String suffix) {
		return code.replace("$", "$"+suffix);
	}

}
