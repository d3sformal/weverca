<?php

$include_included_dir = "In include_dir/include_included_dir.php";
echo $include_included_dir;
echo "<br>";

// Should include test.php from the directory .. (not from the the directory .); because . is the main directory
include 'test.php';
// Should include test2.php from the . directory (the file with this name is not present in the .. directory)
include 'test2.php';


// Called from the .. directory
function func1() {
  global $in_func1;
  $in_func1= "In function func1";
  echo $in_func1;
  echo "<br>";
  
  // Should include test3.php from the . directory
  include 'test3.php';
}

// Never called
function func2() {
  global $in_func2;
  $in_func2 = "In function func2";
  echo $in_func2;
  echo "<br>";
  
  // As the function func2 is never called, the inclusion should not be performed
  include 'test4.php';
}

// Called from the .. directory
function func3() {
  global $in_func3;
  $in_func3 = "In function func3";
  echo $in_func3;
  echo "<br>";
  
  // Defined in the .. directory
  func0();
}



class Cl {
  // Called from the .. directory
  public function meth1() {
    global $in_meth1;
    $in_meth1 = "In method meth1 of class Cl";
    echo $in_meth1;
    echo "<br>";
    
    // Should include test5.php from the . directory
    include 'test5.php';
  }
  
  // Never called
  public function meth2() {
    global $in_meth2;
    $in_meth2 = "In method meth2 of class Cl";
    echo $in_meth2;
    echo "<br>";
    
    // As the method meth2 is never called, the inclusion should not be performed
    include 'test6.php';
  }
}                                                                                          

?>