<?php

include 'constants.php';
include 'template.php';
include 'settings.php';
include 'lang_eng.php';
include 'db.php';
include 'functions.php';
include 'checks.php';

include 'view.php';
include 'delcomment.php';
include 'calendar.php';

/**
 * Mimic errors in mybloggie due to the configuration and custom sanitization.
 */

// Security
$htmlsafe                     = "yes";               // disable & enable html posting
$commenthtmlsafe              = "yes";              // Disable all HTML Tags for comment section prevent exploit
$searchstriptagsenable        = "yes";              // Strip all HTML Tags before search to prevent exploit
$searchhtmlsafe               = "yes";              // Disable all HTML Tags before search to prevent exploit


/// Dont change anything below here !!


$smt = $_GET['smt'];
if ($htmlsafe == "yes") {
    $smt = htmlspecialchars($smt);
}

// False positive due to the configuration (multiple instances in mybloggie)
echo $smt;




?>
