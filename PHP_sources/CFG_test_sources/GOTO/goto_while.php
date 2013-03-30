<?

echo "<h1>While cycle using GOTO</h2>";

$start = microtime(true);
$i = 0;
StartOfLoop:
$i++;
if($i < 1000000) goto StartOfLoop;

echo "<h2>Goto construction from 0 to $i</h2>";
echo microtime(true) - $start.PHP_EOL;

$start = microtime(true);
$i = 0;
while($i < 1000000){
    $i++;
}

echo "<h2>While construction from 0 to $i</h2>";
echo microtime(true) - $start.PHP_EOL;

?>