<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
<title>OOP in PHP</title>

<?php include("class_lib.php"); ?>

</head>

<body>

<?php 
/**
Created by: Stefan Mischook for killerphp.com

Please check out the videos and tutorial at killerphp.com for details.

**/




// Using our PHP objects in our PHP pages. 


$stefan = new person("Stefan Mischook");


echo "Stefan's full name: " . $stefan->get_name();

$james = new employee("Johnny Fingers");

echo "---> " . $james->get_name();

?>


</body>
</html>
