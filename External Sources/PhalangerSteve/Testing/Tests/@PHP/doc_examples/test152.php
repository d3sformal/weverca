[expect php]

[file]
<?php
$var = "Bob";
$Var = "Joe";
echo "$var, $Var";      // outputs "Bob, Joe"

$_4site = 'not yet';    // valid; starts with an underscore
$t�yte = 'mansikka';    // valid; '�' is (Extended) ASCII 228.

echo "$4site, $_4site, $t�yte";
?>
