<?

function f()
{
  echo "start";
      
  foreach ($a as $key => $value) {
      echo "in foreach";
      if (true) {
        echo "return;";
        return;
      }
      echo "after return";
  }  
  echo "end";
}

?>