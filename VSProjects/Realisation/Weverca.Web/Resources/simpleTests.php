<?php
//****************************************
// non-constant and statically unknown accesses
// + XSS demonstration
//****************************************
$input = $_GET[1];
$a[1][1] = 1;
$a[1][2] = 1;
$a[$input][1] = $input;
// not XSS
echo $a[1][2];
// XSS
echo $a[1][1];

//****************************************
//widening example
//****************************************
for($widen=0;$widen<1000000;$widen+=0.001)
{
	$p=$widen;
}
// Emits the warning to demonstrate that the loop was processed
echo $_GET[1];

//****************************************
//indirect function call
//****************************************

function f($message) {
	global $functionResult;
	$functionResult="f called"; }

function g($message){
	global $functionResult;
	$functionResult="g called"; }

if($_POST["a"]=="some value")
	$functionName="f";
else
	$functionName="g";
$functionName($_GET[1]);



//****************************************
// more objects with same name
//****************************************

if($_POST["a"]=="some value"){
	class x	{
		public $ObjectField0=0;
		function __construct(){}
	}
}else{
	class x{
	public $ObjectField1=1;
		function __construct(){}
	}
}
$object=new x();

//****************************************
// security: sql injection
//****************************************

$query="select * from table where value=".$_POST["value"];
mysql_query($query);

?>