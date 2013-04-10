<?

$arr2["x"] = array("one", "two", "three");
$arr2["y"] = array("one", "two");
$arr2["z"] = array("one", "two", "three", "four");

echo "<ul>";
foreach ($arr2 as $key => $value) {
    echo "<li>Outer Key: $key; Value: $value<br />\n";
    echo "<ul>";
    foreach ($value as $key2 => $value2)
    {
      echo "<li>Key: $key2; Value: $value2</li>\n";
    }
    echo "</li></ul>";
}
echo "</ul>";

?>