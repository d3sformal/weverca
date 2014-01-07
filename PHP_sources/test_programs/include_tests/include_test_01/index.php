<?php

// Tests the following functionality:
// The path to the file to be included can be either absolute or relative to the directory:
// 1. Of the main (entry) script
// 2. Of a currently executed script / the script where the currently executed function or method is defined

// Does not test the functionality of indirect includes (include specified by a variable)


$include_main_dir = "";
$include_main_dir2 = "";

$test_main = "";

$test_included = "";
$test2_included = "";
$test3_included = "";
$test4_included = "";
$test5_included = "";
$test6_included = "";
$test7_included = "";

$in_func0 = "";

$in_func1 = "";
$in_func2 = "";
$in_func3 = "";

$in_meth1 = "";
$in_meth2 = "";

$index1 = "In ./index.php";
echo $index1;
echo "<br>";

// Called from func3 defined in include_dir/include_included_dir.php
function func0() {
  global $in_func0;
  $in_func0 = "in function func0";
  echo $in_func0;
  echo "<br>";
  
  // The included file should not be found
  include 'test7.php';
}

include 'include_dir/include_included_dir.php';

include 'include_main_dir.php';

// The included file should not be found
include 'test7.php';

$index2 = "In ./index.php";
echo "<br>";
echo $index2;


$obj = new Cl();
$obj->meth1();

// The included file should not be found
include 'test7.php';

include 'include_main_dir2.php';

func1();

// The included file should not be found
include 'test7.php';

func3();

// The included file should not be found
include 'test7.php';

$index3 = "In ./index.php";
echo "<br>";
echo $index3;

// The content of variables should be as follows:
//$include_main_dir == "In ./include_main_dir.php"
//$include_main_dir2 == "In ./include_main_dir2.php"

// $include_included_dir == "In include_dir/include_included_dir.php"

// $test_main == "In ./test.php";

// $test_included == ""
// $test2_included = "In included_dir/test2.php"
// $test3_included = "In included_dir/test3.php"
// $test4_included = ""
// $test5_included = "In included_dir/test5.php"
// $test6_included = ""
// $test7_included = ""

// $in_func1 = "In function func1";
// $in_func2 = "";
// $in_func3 = "In function func3";
// 
// $in_meth1 = "In method meth1 of class Cl";
// $in_meth2 = "";



// $index1 == "In ./index.php"
// $index2 == "In ./index.php"
// $index3 == "In ./index.php"


?>