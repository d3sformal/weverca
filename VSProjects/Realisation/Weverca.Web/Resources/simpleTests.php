<?php 
//****************************************
//widening example
//****************************************
for($widen=0;$widen<1000000;$widen+=0.001)
{
	$p=$widen;
}

//****************************************
//indirect function call
//****************************************

function f()
{
	global $functionResult;
	$functionResult="f called";
}
function g()
{
	global $functionResult;
	$functionResult="g called";
}
if($_POST["a"]=="some value")
{
	$functionName="f";
}
else
{
	$functionName="g";
}
$functionName();

//****************************************
// not constant constants
//****************************************

if($_POST["a"]=="some value")
{
	define("constant",0);
}
else
{
	define("constant",1);
}
const newConstant=constant;

//****************************************
// more objects with same name
//****************************************

if($_POST["a"]=="some value")
{
	class x
	{
		public $field=0;
		function __construct()
		{
		}
	}
}
else
{
	class x
	{
		public $field=1;
		function __construct()
		{
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
