<?php

/**
 * Mimic incorrect handling of constructions found in file calendar.php of 
 * myBloggie web application
 */

if (isset($_GET['month_no'])) { 
    $month = intval($_GET['month_no']);
}
else{ 
    $month=gmdate('n', time() ); 
}

if (isset($_GET['year'])) { 
    $year = intval($_GET['year']);
}
else { 
    $year = gmdate('Y', time() ); 
}

if ($month < 1 || $month > 12) die();

$monthIndx = date('F', mktime(0, 0, 0, $month, 1, $year));

// False positive: 
// Weverca (as well as all other tested tools) report false positive here.
// They are not able to detect that it is accessed only defined element of the
// $lang array.
$monthName = $lang[$monthIndx]; echo $monthName;
// Problem 1: access to the uninitialized element of the array
echo $lang[$monthIndxx];
?>
