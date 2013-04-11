<?

$arr = array("one", "two", "three");
reset($arr);

echo "<ul>";
foreach ($arr as $value) {
    echo "<li>Value: $value</li>\n";
}
echo "</ul>";

?>