<?
echo "<h1>BAD GOTO test</h1>";

echo "before";
a:
echo "middle";
a:
echo "after";

goto a;


?>