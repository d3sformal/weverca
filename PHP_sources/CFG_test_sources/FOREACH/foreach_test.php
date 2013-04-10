<?

$arr = array("one", "two", "three");
reset($arr);

echo "<ul>";
foreach ($arr as $key => $value) {
    echo "<li>Key: $key; Value: $value</li>\n";
}
echo "</ul>";

?>