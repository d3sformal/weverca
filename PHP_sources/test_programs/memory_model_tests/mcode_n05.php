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
if ($_POST) {
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
	$aaaalias = array();
	$aaaalias2 = 0;
	$aaaalias3 = 1;
	if ($_POST) {
		$aaaarr[$_POST] = &$aaaalias;
		$aaat = $aaaarr[1];
		$aaat[2] = 2;
		$aaaarr[1][2] = 3;
		$aaaarr[1][3] = 4;
		$aaaarr[2][3] = 5;
	} else {
		$aaaarr[$_POST][2] = 6;
		$aaaarr[1][$_POST] = 7;
	}
	$aaaarr[2][1] = &$aaaalias2; // {$aaaarr[2][1], $aaaalias[1], $aaaalias2}
	$aaaarr[2] = &$aaaalias3; // {$aaaarr[2], $aaaalias3}, {$aaaalias[1], $aaaalias2}
	$aaaarr2 = $aaaarr;
	$aaaarr2[2] = 8; // updates also $aaaarr[2] and $aaaalias3
	$aaaarr2[3] = 9; // updates also $aaaarr[3] and $aaaalias
	$aaaarr[$_POST] = $aaaarr2;
	$aaaaaalias = array();
	$aaaaaalias2 = 0;
	$aaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaarr[$_POST] = &$aaaaaalias;
		$aaaaat = $aaaaaarr[1];
		$aaaaat[2] = 2;
		$aaaaaarr[1][2] = 3;
		$aaaaaarr[1][3] = 4;
		$aaaaaarr[2][3] = 5;
	} else {
		$aaaaaarr[$_POST][2] = 6;
		$aaaaaarr[1][$_POST] = 7;
	}
	$aaaaaarr[2][1] = &$aaaaaalias2; // {$aaaaaarr[2][1], $aaaaaalias[1], $aaaaaalias2}
	$aaaaaarr[2] = &$aaaaaalias3; // {$aaaaaarr[2], $aaaaaalias3}, {$aaaaaalias[1], $aaaaaalias2}
	$aaaaaarr2 = $aaaaaarr;
	$aaaaaarr2[2] = 8; // updates also $aaaaaarr[2] and $aaaaaalias3
	$aaaaaarr2[3] = 9; // updates also $aaaaaarr[3] and $aaaaaalias
	$aaaaaarr[$_POST] = $aaaaaarr2;
	$aaaaalias = array();
	$aaaaalias2 = 0;
	$aaaaalias3 = 1;
	if ($_POST) {
		$aaaaarr[$_POST] = &$aaaaalias;
		$aaaat = $aaaaarr[1];
		$aaaat[2] = 2;
		$aaaaarr[1][2] = 3;
		$aaaaarr[1][3] = 4;
		$aaaaarr[2][3] = 5;
	} else {
		$aaaaarr[$_POST][2] = 6;
		$aaaaarr[1][$_POST] = 7;
	}
	$aaaaarr[2][1] = &$aaaaalias2; // {$aaaaarr[2][1], $aaaaalias[1], $aaaaalias2}
	$aaaaarr[2] = &$aaaaalias3; // {$aaaaarr[2], $aaaaalias3}, {$aaaaalias[1], $aaaaalias2}
	$aaaaarr2 = $aaaaarr;
	$aaaaarr2[2] = 8; // updates also $aaaaarr[2] and $aaaaalias3
	$aaaaarr2[3] = 9; // updates also $aaaaarr[3] and $aaaaalias
	$aaaaarr[$_POST] = $aaaaarr2;
	$aaaaaaalias = array();
	$aaaaaaalias2 = 0;
	$aaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaarr[$_POST] = &$aaaaaaalias;
		$aaaaaat = $aaaaaaarr[1];
		$aaaaaat[2] = 2;
		$aaaaaaarr[1][2] = 3;
		$aaaaaaarr[1][3] = 4;
		$aaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaarr[$_POST][2] = 6;
		$aaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaarr[2][1] = &$aaaaaaalias2; // {$aaaaaaarr[2][1], $aaaaaaalias[1], $aaaaaaalias2}
	$aaaaaaarr[2] = &$aaaaaaalias3; // {$aaaaaaarr[2], $aaaaaaalias3}, {$aaaaaaalias[1], $aaaaaaalias2}
	$aaaaaaarr2 = $aaaaaaarr;
	$aaaaaaarr2[2] = 8; // updates also $aaaaaaarr[2] and $aaaaaaalias3
	$aaaaaaarr2[3] = 9; // updates also $aaaaaaarr[3] and $aaaaaaalias
	$aaaaaaarr[$_POST] = $aaaaaaarr2;
	$aaaaaaaalias = array();
	$aaaaaaaalias2 = 0;
	$aaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaarr[$_POST] = &$aaaaaaaalias;
		$aaaaaaat = $aaaaaaaarr[1];
		$aaaaaaat[2] = 2;
		$aaaaaaaarr[1][2] = 3;
		$aaaaaaaarr[1][3] = 4;
		$aaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaarr[2][1] = &$aaaaaaaalias2; // {$aaaaaaaarr[2][1], $aaaaaaaalias[1], $aaaaaaaalias2}
	$aaaaaaaarr[2] = &$aaaaaaaalias3; // {$aaaaaaaarr[2], $aaaaaaaalias3}, {$aaaaaaaalias[1], $aaaaaaaalias2}
	$aaaaaaaarr2 = $aaaaaaaarr;
	$aaaaaaaarr2[2] = 8; // updates also $aaaaaaaarr[2] and $aaaaaaaalias3
	$aaaaaaaarr2[3] = 9; // updates also $aaaaaaaarr[3] and $aaaaaaaalias
	$aaaaaaaarr[$_POST] = $aaaaaaaarr2;
	$aaaaaaaaaalias = array();
	$aaaaaaaaaalias2 = 0;
	$aaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaarr[$_POST] = &$aaaaaaaaaalias;
		$aaaaaaaaat = $aaaaaaaaaarr[1];
		$aaaaaaaaat[2] = 2;
		$aaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaarr[2][1] = &$aaaaaaaaaalias2; // {$aaaaaaaaaarr[2][1], $aaaaaaaaaalias[1], $aaaaaaaaaalias2}
	$aaaaaaaaaarr[2] = &$aaaaaaaaaalias3; // {$aaaaaaaaaarr[2], $aaaaaaaaaalias3}, {$aaaaaaaaaalias[1], $aaaaaaaaaalias2}
	$aaaaaaaaaarr2 = $aaaaaaaaaarr;
	$aaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaarr[2] and $aaaaaaaaaalias3
	$aaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaarr[3] and $aaaaaaaaaalias
	$aaaaaaaaaarr[$_POST] = $aaaaaaaaaarr2;
	$aaaaaalias = array();
	$aaaaaalias2 = 0;
	$aaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaarr[$_POST] = &$aaaaaalias;
		$aaaaat = $aaaaaarr[1];
		$aaaaat[2] = 2;
		$aaaaaarr[1][2] = 3;
		$aaaaaarr[1][3] = 4;
		$aaaaaarr[2][3] = 5;
	} else {
		$aaaaaarr[$_POST][2] = 6;
		$aaaaaarr[1][$_POST] = 7;
	}
	$aaaaaarr[2][1] = &$aaaaaalias2; // {$aaaaaarr[2][1], $aaaaaalias[1], $aaaaaalias2}
	$aaaaaarr[2] = &$aaaaaalias3; // {$aaaaaarr[2], $aaaaaalias3}, {$aaaaaalias[1], $aaaaaalias2}
	$aaaaaarr2 = $aaaaaarr;
	$aaaaaarr2[2] = 8; // updates also $aaaaaarr[2] and $aaaaaalias3
	$aaaaaarr2[3] = 9; // updates also $aaaaaarr[3] and $aaaaaalias
	$aaaaaarr[$_POST] = $aaaaaarr2;
	$aaaaaaaalias = array();
	$aaaaaaaalias2 = 0;
	$aaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaarr[$_POST] = &$aaaaaaaalias;
		$aaaaaaat = $aaaaaaaarr[1];
		$aaaaaaat[2] = 2;
		$aaaaaaaarr[1][2] = 3;
		$aaaaaaaarr[1][3] = 4;
		$aaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaarr[2][1] = &$aaaaaaaalias2; // {$aaaaaaaarr[2][1], $aaaaaaaalias[1], $aaaaaaaalias2}
	$aaaaaaaarr[2] = &$aaaaaaaalias3; // {$aaaaaaaarr[2], $aaaaaaaalias3}, {$aaaaaaaalias[1], $aaaaaaaalias2}
	$aaaaaaaarr2 = $aaaaaaaarr;
	$aaaaaaaarr2[2] = 8; // updates also $aaaaaaaarr[2] and $aaaaaaaalias3
	$aaaaaaaarr2[3] = 9; // updates also $aaaaaaaarr[3] and $aaaaaaaalias
	$aaaaaaaarr[$_POST] = $aaaaaaaarr2;
	$aaaaaaaaalias = array();
	$aaaaaaaaalias2 = 0;
	$aaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaarr[$_POST] = &$aaaaaaaaalias;
		$aaaaaaaat = $aaaaaaaaarr[1];
		$aaaaaaaat[2] = 2;
		$aaaaaaaaarr[1][2] = 3;
		$aaaaaaaaarr[1][3] = 4;
		$aaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaarr[2][1] = &$aaaaaaaaalias2; // {$aaaaaaaaarr[2][1], $aaaaaaaaalias[1], $aaaaaaaaalias2}
	$aaaaaaaaarr[2] = &$aaaaaaaaalias3; // {$aaaaaaaaarr[2], $aaaaaaaaalias3}, {$aaaaaaaaalias[1], $aaaaaaaaalias2}
	$aaaaaaaaarr2 = $aaaaaaaaarr;
	$aaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaarr[2] and $aaaaaaaaalias3
	$aaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaarr[3] and $aaaaaaaaalias
	$aaaaaaaaarr[$_POST] = $aaaaaaaaarr2;
	$aaaaaaaaaaalias = array();
	$aaaaaaaaaaalias2 = 0;
	$aaaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaaarr[$_POST] = &$aaaaaaaaaaalias;
		$aaaaaaaaaat = $aaaaaaaaaaarr[1];
		$aaaaaaaaaat[2] = 2;
		$aaaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaaarr[2][1] = &$aaaaaaaaaaalias2; // {$aaaaaaaaaaarr[2][1], $aaaaaaaaaaalias[1], $aaaaaaaaaaalias2}
	$aaaaaaaaaaarr[2] = &$aaaaaaaaaaalias3; // {$aaaaaaaaaaarr[2], $aaaaaaaaaaalias3}, {$aaaaaaaaaaalias[1], $aaaaaaaaaaalias2}
	$aaaaaaaaaaarr2 = $aaaaaaaaaaarr;
	$aaaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaaarr[2] and $aaaaaaaaaaalias3
	$aaaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaaarr[3] and $aaaaaaaaaaalias
	$aaaaaaaaaaarr[$_POST] = $aaaaaaaaaaarr2;
	$aaaaaaaaaalias = array();
	$aaaaaaaaaalias2 = 0;
	$aaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaarr[$_POST] = &$aaaaaaaaaalias;
		$aaaaaaaaat = $aaaaaaaaaarr[1];
		$aaaaaaaaat[2] = 2;
		$aaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaarr[2][1] = &$aaaaaaaaaalias2; // {$aaaaaaaaaarr[2][1], $aaaaaaaaaalias[1], $aaaaaaaaaalias2}
	$aaaaaaaaaarr[2] = &$aaaaaaaaaalias3; // {$aaaaaaaaaarr[2], $aaaaaaaaaalias3}, {$aaaaaaaaaalias[1], $aaaaaaaaaalias2}
	$aaaaaaaaaarr2 = $aaaaaaaaaarr;
	$aaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaarr[2] and $aaaaaaaaaalias3
	$aaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaarr[3] and $aaaaaaaaaalias
	$aaaaaaaaaarr[$_POST] = $aaaaaaaaaarr2;
	$aaaaaaaaaaaalias = array();
	$aaaaaaaaaaaalias2 = 0;
	$aaaaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaaaarr[$_POST] = &$aaaaaaaaaaaalias;
		$aaaaaaaaaaat = $aaaaaaaaaaaarr[1];
		$aaaaaaaaaaat[2] = 2;
		$aaaaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaaaarr[2][1] = &$aaaaaaaaaaaalias2; // {$aaaaaaaaaaaarr[2][1], $aaaaaaaaaaaalias[1], $aaaaaaaaaaaalias2}
	$aaaaaaaaaaaarr[2] = &$aaaaaaaaaaaalias3; // {$aaaaaaaaaaaarr[2], $aaaaaaaaaaaalias3}, {$aaaaaaaaaaaalias[1], $aaaaaaaaaaaalias2}
	$aaaaaaaaaaaarr2 = $aaaaaaaaaaaarr;
	$aaaaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaaaarr[2] and $aaaaaaaaaaaalias3
	$aaaaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaaaarr[3] and $aaaaaaaaaaaalias
	$aaaaaaaaaaaarr[$_POST] = $aaaaaaaaaaaarr2;
	$aaaaaaaaaaaaalias = array();
	$aaaaaaaaaaaaalias2 = 0;
	$aaaaaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaaaaarr[$_POST] = &$aaaaaaaaaaaaalias;
		$aaaaaaaaaaaat = $aaaaaaaaaaaaarr[1];
		$aaaaaaaaaaaat[2] = 2;
		$aaaaaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaaaaarr[2][1] = &$aaaaaaaaaaaaalias2; // {$aaaaaaaaaaaaarr[2][1], $aaaaaaaaaaaaalias[1], $aaaaaaaaaaaaalias2}
	$aaaaaaaaaaaaarr[2] = &$aaaaaaaaaaaaalias3; // {$aaaaaaaaaaaaarr[2], $aaaaaaaaaaaaalias3}, {$aaaaaaaaaaaaalias[1], $aaaaaaaaaaaaalias2}
	$aaaaaaaaaaaaarr2 = $aaaaaaaaaaaaarr;
	$aaaaaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaaaaarr[2] and $aaaaaaaaaaaaalias3
	$aaaaaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaaaaarr[3] and $aaaaaaaaaaaaalias
	$aaaaaaaaaaaaarr[$_POST] = $aaaaaaaaaaaaarr2;
	$aaaaaaaaaaaaaaalias = array();
	$aaaaaaaaaaaaaaalias2 = 0;
	$aaaaaaaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaaaaaaarr[$_POST] = &$aaaaaaaaaaaaaaalias;
		$aaaaaaaaaaaaaat = $aaaaaaaaaaaaaaarr[1];
		$aaaaaaaaaaaaaat[2] = 2;
		$aaaaaaaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaaaaaaarr[2][1] = &$aaaaaaaaaaaaaaalias2; // {$aaaaaaaaaaaaaaarr[2][1], $aaaaaaaaaaaaaaalias[1], $aaaaaaaaaaaaaaalias2}
	$aaaaaaaaaaaaaaarr[2] = &$aaaaaaaaaaaaaaalias3; // {$aaaaaaaaaaaaaaarr[2], $aaaaaaaaaaaaaaalias3}, {$aaaaaaaaaaaaaaalias[1], $aaaaaaaaaaaaaaalias2}
	$aaaaaaaaaaaaaaarr2 = $aaaaaaaaaaaaaaarr;
	$aaaaaaaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaaaaaaarr[2] and $aaaaaaaaaaaaaaalias3
	$aaaaaaaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaaaaaaarr[3] and $aaaaaaaaaaaaaaalias
	$aaaaaaaaaaaaaaarr[$_POST] = $aaaaaaaaaaaaaaarr2;
} else {
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
	$aaaalias = array();
	$aaaalias2 = 0;
	$aaaalias3 = 1;
	if ($_POST) {
		$aaaarr[$_POST] = &$aaaalias;
		$aaat = $aaaarr[1];
		$aaat[2] = 2;
		$aaaarr[1][2] = 3;
		$aaaarr[1][3] = 4;
		$aaaarr[2][3] = 5;
	} else {
		$aaaarr[$_POST][2] = 6;
		$aaaarr[1][$_POST] = 7;
	}
	$aaaarr[2][1] = &$aaaalias2; // {$aaaarr[2][1], $aaaalias[1], $aaaalias2}
	$aaaarr[2] = &$aaaalias3; // {$aaaarr[2], $aaaalias3}, {$aaaalias[1], $aaaalias2}
	$aaaarr2 = $aaaarr;
	$aaaarr2[2] = 8; // updates also $aaaarr[2] and $aaaalias3
	$aaaarr2[3] = 9; // updates also $aaaarr[3] and $aaaalias
	$aaaarr[$_POST] = $aaaarr2;
	$aaaaaalias = array();
	$aaaaaalias2 = 0;
	$aaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaarr[$_POST] = &$aaaaaalias;
		$aaaaat = $aaaaaarr[1];
		$aaaaat[2] = 2;
		$aaaaaarr[1][2] = 3;
		$aaaaaarr[1][3] = 4;
		$aaaaaarr[2][3] = 5;
	} else {
		$aaaaaarr[$_POST][2] = 6;
		$aaaaaarr[1][$_POST] = 7;
	}
	$aaaaaarr[2][1] = &$aaaaaalias2; // {$aaaaaarr[2][1], $aaaaaalias[1], $aaaaaalias2}
	$aaaaaarr[2] = &$aaaaaalias3; // {$aaaaaarr[2], $aaaaaalias3}, {$aaaaaalias[1], $aaaaaalias2}
	$aaaaaarr2 = $aaaaaarr;
	$aaaaaarr2[2] = 8; // updates also $aaaaaarr[2] and $aaaaaalias3
	$aaaaaarr2[3] = 9; // updates also $aaaaaarr[3] and $aaaaaalias
	$aaaaaarr[$_POST] = $aaaaaarr2;
	$aaaaalias = array();
	$aaaaalias2 = 0;
	$aaaaalias3 = 1;
	if ($_POST) {
		$aaaaarr[$_POST] = &$aaaaalias;
		$aaaat = $aaaaarr[1];
		$aaaat[2] = 2;
		$aaaaarr[1][2] = 3;
		$aaaaarr[1][3] = 4;
		$aaaaarr[2][3] = 5;
	} else {
		$aaaaarr[$_POST][2] = 6;
		$aaaaarr[1][$_POST] = 7;
	}
	$aaaaarr[2][1] = &$aaaaalias2; // {$aaaaarr[2][1], $aaaaalias[1], $aaaaalias2}
	$aaaaarr[2] = &$aaaaalias3; // {$aaaaarr[2], $aaaaalias3}, {$aaaaalias[1], $aaaaalias2}
	$aaaaarr2 = $aaaaarr;
	$aaaaarr2[2] = 8; // updates also $aaaaarr[2] and $aaaaalias3
	$aaaaarr2[3] = 9; // updates also $aaaaarr[3] and $aaaaalias
	$aaaaarr[$_POST] = $aaaaarr2;
	$aaaaaaalias = array();
	$aaaaaaalias2 = 0;
	$aaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaarr[$_POST] = &$aaaaaaalias;
		$aaaaaat = $aaaaaaarr[1];
		$aaaaaat[2] = 2;
		$aaaaaaarr[1][2] = 3;
		$aaaaaaarr[1][3] = 4;
		$aaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaarr[$_POST][2] = 6;
		$aaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaarr[2][1] = &$aaaaaaalias2; // {$aaaaaaarr[2][1], $aaaaaaalias[1], $aaaaaaalias2}
	$aaaaaaarr[2] = &$aaaaaaalias3; // {$aaaaaaarr[2], $aaaaaaalias3}, {$aaaaaaalias[1], $aaaaaaalias2}
	$aaaaaaarr2 = $aaaaaaarr;
	$aaaaaaarr2[2] = 8; // updates also $aaaaaaarr[2] and $aaaaaaalias3
	$aaaaaaarr2[3] = 9; // updates also $aaaaaaarr[3] and $aaaaaaalias
	$aaaaaaarr[$_POST] = $aaaaaaarr2;
	$aaaaaaaalias = array();
	$aaaaaaaalias2 = 0;
	$aaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaarr[$_POST] = &$aaaaaaaalias;
		$aaaaaaat = $aaaaaaaarr[1];
		$aaaaaaat[2] = 2;
		$aaaaaaaarr[1][2] = 3;
		$aaaaaaaarr[1][3] = 4;
		$aaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaarr[2][1] = &$aaaaaaaalias2; // {$aaaaaaaarr[2][1], $aaaaaaaalias[1], $aaaaaaaalias2}
	$aaaaaaaarr[2] = &$aaaaaaaalias3; // {$aaaaaaaarr[2], $aaaaaaaalias3}, {$aaaaaaaalias[1], $aaaaaaaalias2}
	$aaaaaaaarr2 = $aaaaaaaarr;
	$aaaaaaaarr2[2] = 8; // updates also $aaaaaaaarr[2] and $aaaaaaaalias3
	$aaaaaaaarr2[3] = 9; // updates also $aaaaaaaarr[3] and $aaaaaaaalias
	$aaaaaaaarr[$_POST] = $aaaaaaaarr2;
	$aaaaaaaaaalias = array();
	$aaaaaaaaaalias2 = 0;
	$aaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaarr[$_POST] = &$aaaaaaaaaalias;
		$aaaaaaaaat = $aaaaaaaaaarr[1];
		$aaaaaaaaat[2] = 2;
		$aaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaarr[2][1] = &$aaaaaaaaaalias2; // {$aaaaaaaaaarr[2][1], $aaaaaaaaaalias[1], $aaaaaaaaaalias2}
	$aaaaaaaaaarr[2] = &$aaaaaaaaaalias3; // {$aaaaaaaaaarr[2], $aaaaaaaaaalias3}, {$aaaaaaaaaalias[1], $aaaaaaaaaalias2}
	$aaaaaaaaaarr2 = $aaaaaaaaaarr;
	$aaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaarr[2] and $aaaaaaaaaalias3
	$aaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaarr[3] and $aaaaaaaaaalias
	$aaaaaaaaaarr[$_POST] = $aaaaaaaaaarr2;
	$aaaaaalias = array();
	$aaaaaalias2 = 0;
	$aaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaarr[$_POST] = &$aaaaaalias;
		$aaaaat = $aaaaaarr[1];
		$aaaaat[2] = 2;
		$aaaaaarr[1][2] = 3;
		$aaaaaarr[1][3] = 4;
		$aaaaaarr[2][3] = 5;
	} else {
		$aaaaaarr[$_POST][2] = 6;
		$aaaaaarr[1][$_POST] = 7;
	}
	$aaaaaarr[2][1] = &$aaaaaalias2; // {$aaaaaarr[2][1], $aaaaaalias[1], $aaaaaalias2}
	$aaaaaarr[2] = &$aaaaaalias3; // {$aaaaaarr[2], $aaaaaalias3}, {$aaaaaalias[1], $aaaaaalias2}
	$aaaaaarr2 = $aaaaaarr;
	$aaaaaarr2[2] = 8; // updates also $aaaaaarr[2] and $aaaaaalias3
	$aaaaaarr2[3] = 9; // updates also $aaaaaarr[3] and $aaaaaalias
	$aaaaaarr[$_POST] = $aaaaaarr2;
	$aaaaaaaalias = array();
	$aaaaaaaalias2 = 0;
	$aaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaarr[$_POST] = &$aaaaaaaalias;
		$aaaaaaat = $aaaaaaaarr[1];
		$aaaaaaat[2] = 2;
		$aaaaaaaarr[1][2] = 3;
		$aaaaaaaarr[1][3] = 4;
		$aaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaarr[2][1] = &$aaaaaaaalias2; // {$aaaaaaaarr[2][1], $aaaaaaaalias[1], $aaaaaaaalias2}
	$aaaaaaaarr[2] = &$aaaaaaaalias3; // {$aaaaaaaarr[2], $aaaaaaaalias3}, {$aaaaaaaalias[1], $aaaaaaaalias2}
	$aaaaaaaarr2 = $aaaaaaaarr;
	$aaaaaaaarr2[2] = 8; // updates also $aaaaaaaarr[2] and $aaaaaaaalias3
	$aaaaaaaarr2[3] = 9; // updates also $aaaaaaaarr[3] and $aaaaaaaalias
	$aaaaaaaarr[$_POST] = $aaaaaaaarr2;
	$aaaaaaaaalias = array();
	$aaaaaaaaalias2 = 0;
	$aaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaarr[$_POST] = &$aaaaaaaaalias;
		$aaaaaaaat = $aaaaaaaaarr[1];
		$aaaaaaaat[2] = 2;
		$aaaaaaaaarr[1][2] = 3;
		$aaaaaaaaarr[1][3] = 4;
		$aaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaarr[2][1] = &$aaaaaaaaalias2; // {$aaaaaaaaarr[2][1], $aaaaaaaaalias[1], $aaaaaaaaalias2}
	$aaaaaaaaarr[2] = &$aaaaaaaaalias3; // {$aaaaaaaaarr[2], $aaaaaaaaalias3}, {$aaaaaaaaalias[1], $aaaaaaaaalias2}
	$aaaaaaaaarr2 = $aaaaaaaaarr;
	$aaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaarr[2] and $aaaaaaaaalias3
	$aaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaarr[3] and $aaaaaaaaalias
	$aaaaaaaaarr[$_POST] = $aaaaaaaaarr2;
	$aaaaaaaaaaalias = array();
	$aaaaaaaaaaalias2 = 0;
	$aaaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaaarr[$_POST] = &$aaaaaaaaaaalias;
		$aaaaaaaaaat = $aaaaaaaaaaarr[1];
		$aaaaaaaaaat[2] = 2;
		$aaaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaaarr[2][1] = &$aaaaaaaaaaalias2; // {$aaaaaaaaaaarr[2][1], $aaaaaaaaaaalias[1], $aaaaaaaaaaalias2}
	$aaaaaaaaaaarr[2] = &$aaaaaaaaaaalias3; // {$aaaaaaaaaaarr[2], $aaaaaaaaaaalias3}, {$aaaaaaaaaaalias[1], $aaaaaaaaaaalias2}
	$aaaaaaaaaaarr2 = $aaaaaaaaaaarr;
	$aaaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaaarr[2] and $aaaaaaaaaaalias3
	$aaaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaaarr[3] and $aaaaaaaaaaalias
	$aaaaaaaaaaarr[$_POST] = $aaaaaaaaaaarr2;
	$aaaaaaaaaalias = array();
	$aaaaaaaaaalias2 = 0;
	$aaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaarr[$_POST] = &$aaaaaaaaaalias;
		$aaaaaaaaat = $aaaaaaaaaarr[1];
		$aaaaaaaaat[2] = 2;
		$aaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaarr[2][1] = &$aaaaaaaaaalias2; // {$aaaaaaaaaarr[2][1], $aaaaaaaaaalias[1], $aaaaaaaaaalias2}
	$aaaaaaaaaarr[2] = &$aaaaaaaaaalias3; // {$aaaaaaaaaarr[2], $aaaaaaaaaalias3}, {$aaaaaaaaaalias[1], $aaaaaaaaaalias2}
	$aaaaaaaaaarr2 = $aaaaaaaaaarr;
	$aaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaarr[2] and $aaaaaaaaaalias3
	$aaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaarr[3] and $aaaaaaaaaalias
	$aaaaaaaaaarr[$_POST] = $aaaaaaaaaarr2;
	$aaaaaaaaaaaalias = array();
	$aaaaaaaaaaaalias2 = 0;
	$aaaaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaaaarr[$_POST] = &$aaaaaaaaaaaalias;
		$aaaaaaaaaaat = $aaaaaaaaaaaarr[1];
		$aaaaaaaaaaat[2] = 2;
		$aaaaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaaaarr[2][1] = &$aaaaaaaaaaaalias2; // {$aaaaaaaaaaaarr[2][1], $aaaaaaaaaaaalias[1], $aaaaaaaaaaaalias2}
	$aaaaaaaaaaaarr[2] = &$aaaaaaaaaaaalias3; // {$aaaaaaaaaaaarr[2], $aaaaaaaaaaaalias3}, {$aaaaaaaaaaaalias[1], $aaaaaaaaaaaalias2}
	$aaaaaaaaaaaarr2 = $aaaaaaaaaaaarr;
	$aaaaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaaaarr[2] and $aaaaaaaaaaaalias3
	$aaaaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaaaarr[3] and $aaaaaaaaaaaalias
	$aaaaaaaaaaaarr[$_POST] = $aaaaaaaaaaaarr2;
	$aaaaaaaaaaaaalias = array();
	$aaaaaaaaaaaaalias2 = 0;
	$aaaaaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaaaaarr[$_POST] = &$aaaaaaaaaaaaalias;
		$aaaaaaaaaaaat = $aaaaaaaaaaaaarr[1];
		$aaaaaaaaaaaat[2] = 2;
		$aaaaaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaaaaarr[2][1] = &$aaaaaaaaaaaaalias2; // {$aaaaaaaaaaaaarr[2][1], $aaaaaaaaaaaaalias[1], $aaaaaaaaaaaaalias2}
	$aaaaaaaaaaaaarr[2] = &$aaaaaaaaaaaaalias3; // {$aaaaaaaaaaaaarr[2], $aaaaaaaaaaaaalias3}, {$aaaaaaaaaaaaalias[1], $aaaaaaaaaaaaalias2}
	$aaaaaaaaaaaaarr2 = $aaaaaaaaaaaaarr;
	$aaaaaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaaaaarr[2] and $aaaaaaaaaaaaalias3
	$aaaaaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaaaaarr[3] and $aaaaaaaaaaaaalias
	$aaaaaaaaaaaaarr[$_POST] = $aaaaaaaaaaaaarr2;
	$aaaaaaaaaaaaaaalias = array();
	$aaaaaaaaaaaaaaalias2 = 0;
	$aaaaaaaaaaaaaaalias3 = 1;
	if ($_POST) {
		$aaaaaaaaaaaaaaarr[$_POST] = &$aaaaaaaaaaaaaaalias;
		$aaaaaaaaaaaaaat = $aaaaaaaaaaaaaaarr[1];
		$aaaaaaaaaaaaaat[2] = 2;
		$aaaaaaaaaaaaaaarr[1][2] = 3;
		$aaaaaaaaaaaaaaarr[1][3] = 4;
		$aaaaaaaaaaaaaaarr[2][3] = 5;
	} else {
		$aaaaaaaaaaaaaaarr[$_POST][2] = 6;
		$aaaaaaaaaaaaaaarr[1][$_POST] = 7;
	}
	$aaaaaaaaaaaaaaarr[2][1] = &$aaaaaaaaaaaaaaalias2; // {$aaaaaaaaaaaaaaarr[2][1], $aaaaaaaaaaaaaaalias[1], $aaaaaaaaaaaaaaalias2}
	$aaaaaaaaaaaaaaarr[2] = &$aaaaaaaaaaaaaaalias3; // {$aaaaaaaaaaaaaaarr[2], $aaaaaaaaaaaaaaalias3}, {$aaaaaaaaaaaaaaalias[1], $aaaaaaaaaaaaaaalias2}
	$aaaaaaaaaaaaaaarr2 = $aaaaaaaaaaaaaaarr;
	$aaaaaaaaaaaaaaarr2[2] = 8; // updates also $aaaaaaaaaaaaaaarr[2] and $aaaaaaaaaaaaaaalias3
	$aaaaaaaaaaaaaaarr2[3] = 9; // updates also $aaaaaaaaaaaaaaarr[3] and $aaaaaaaaaaaaaaalias
	$aaaaaaaaaaaaaaarr[$_POST] = $aaaaaaaaaaaaaaarr2;
}
?>