<?
echo "start";
    
foreach ($a as $key => $value) {
    echo "in foreach";
    if (true) {
      echo "goto;";
      goto END;
    }
    echo "after goto";
}

END:

echo "end";

?>