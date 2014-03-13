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

function f($message)
{
	global $functionResult;

	$functionResult="f called";
	// Emits the warning
	echo $message;
}
function g($message)
{
	global $functionResult;
	$functionResult="g called";
	// Emits the warning
	echo $message;
}
function h($message) {
	// Does not emit the warning - this function cannot be called
	echo $message;
}
if($_POST["a"]=="some value")
{
	$functionName="f";
}
else
{
	$functionName="g";
}
$functionName($_GET[1]);


//****************************************
// more objects with same name
//****************************************

if($_POST["a"]=="some value")
{
	class x
	{
		public $ObjectField0=0;
		function __construct()
		{
		  echo $_GET[1];

		}
	}
}
else
{
	class x
	{
	public $ObjectField1=1;
		function __construct()
		{
		  echo $_GET[1];


		}
	}
}

$object=new x();

//****************************************
// security: sql injection
//****************************************

$query="select * from table where value=".$_POST["value"];
mysql_query($query);

//****************************************
// array error
//****************************************

$array=4;
if($_POST["a"]=="some value")
{
$array=array();
}
$array[0]=4;

?>