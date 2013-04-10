<?
echo "start";
    
foreach ($a as $key => $value) {
    echo "in foreach";
    if (true) {
      echo "break;";
      break;
    }
    echo "after break";
}

echo "end";

?>