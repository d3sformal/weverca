<?php
/*
Values:
 $alias = {9, array}
 
 $alias[1] = {undefined, 1} alias $alias2
 $alias[2] = {undefined, 3}
 $alias[3] = {}
 
 $alias2 = {1}
 
 $alias3 = {8}
 
 $arr = {array}
 
 $arr[?] = {undefined, array}
 $arr[1] = {array}
 $arr[2] = {8}
 $arr[3] = {undefined, array, 9} Even if it is may-alias of $alias, it does not have indexes $arr[3][1], $arr[3][2], $arr[3][3]
 
 $arr[?][2] = {undefined, 6}
 $arr[1][?] = {undefined, 7}
 $arr[1][2] = {undefined, 3, 6, 7}
 $arr[1][3] = {undefined, 4, 7}

 $arr[2][1] = {1} Before the statement $arr[2] = &$alias3; Empty after this statement.
 $arr[2][2] = {undefined, 6} Before the statement $arr[2] = &$alias3; Empty after this statement.
 $arr[2][3] = {undefined, 5} Before the statement $arr[2] = &$alias3; Empty after this statement.
 
 $arr2 similar to arr
 

 Aliases:
*/
$alias = array();
$alias2 = 0;
$alias3 = 1;
if ($_POST) {
	$arr[$_POST] = &$alias;
	$t = $arr[1];
	$t[2] = 2;
	$arr[1][2] = 3;
	$arr[1][3] = 4;
	$arr[2][3] = 5;
} else {
	$arr[$_POST][2] = 6;
	$arr[1][$_POST] = 7;
}
$arr[2][1] = &$alias2; // {$arr[2][1], $alias[1], $alias2}
$arr[2] = &$alias3; // {$arr[2], $alias3}, {$alias[1], $alias2}
$arr2 = $arr;
$arr2[2] = 8; // updates also $arr[2] and $alias3
$arr2[3] = 9; // updates also $arr[3] and $alias
$arr[$_POST] = $arr2;
$aaalias = array();
$aaalias2 = 0;
$aaalias3 = 1;
if ($_POST) {
	$aaarr[$_POST] = &$aaalias;
	$aat = $aaarr[1];
	$aat[2] = 2;
	$aaarr[1][2] = 3;
	$aaarr[1][3] = 4;
	$aaarr[2][3] = 5;
} else {
	$aaarr[$_POST][2] = 6;
	$aaarr[1][$_POST] = 7;
}
$aaarr[2][1] = &$aaalias2; // {$aaarr[2][1], $aaalias[1], $aaalias2}
$aaarr[2] = &$aaalias3; // {$aaarr[2], $aaalias3}, {$aaalias[1], $aaalias2}
$aaarr2 = $aaarr;
$aaarr2[2] = 8; // updates also $aaarr[2] and $aaalias3
$aaarr2[3] = 9; // updates also $aaarr[3] and $aaalias
$aaarr[$_POST] = $aaarr2;
?>