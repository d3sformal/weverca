[expect ct-error] Syntax error

[file]
<?php
$var = "Bob";
$Var = "Joe";
echo "$var, $Var";      // outputs "Bob, Joe"

$4site = 'not yet';     // invalid; starts with a number
$_4site = 'not yet';    // valid; starts with an underscore
$t�yte = 'mansikka';    // valid; '�' is (Extended) ASCII 228.

echo "$4site, $_4site, $t�yte";
?>
