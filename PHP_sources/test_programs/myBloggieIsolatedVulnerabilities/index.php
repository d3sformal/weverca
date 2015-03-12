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

$smt = $_GET['smt'];
if ($htmlsafe == "yes") {
    $smt = htmlspecialchars($smt);
}

// False positive due to the configuration (multiple instances in mybloggie)
echo $smt;




?>
