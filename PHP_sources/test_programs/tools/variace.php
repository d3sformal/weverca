<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html>
  <head>
  <meta http-equiv="content-type" content="text/html; charset=windows-1250">
  <meta name="generator" content="PSPad editor, www.pspad.com">
  <title></title>
  </head>
  <body>
    <table>
<?

  $prvku = 4;
  
  for($y=1, $fa=1 ; $y<=$prvku;$y++ , $fa = $fa * $y){
    $poz[$y-1] = $prvku - $y + 1;
    $fakt[$y] = $fa;
  }
  $fakt[0]=1;
  
  for($x=1;$x<=$fakt[$prvku];$x++){
    echo "\n    <tr><td>$x</td><td>";
    
    for($y=1;$y<=$prvku;$y++)$used[$y]=0;
    
    for($y=$prvku-1;$y>=0;$y--){
      echo $poz[$y];
      if($x % $fakt[$y] == 0){
        if($x % $fakt[$y+1] == 0)$poz[$y] = $prvku;
        $nova = $poz[$y];
        do{
          $nova++;
          if($nova>$prvku)$nova=1;
          if($nova==$poz[$y])break;
        }
        while($used[$nova]);
        $poz[$y] = $nova;
      }
      $used[$poz[$y]] = $y;
    }
    
    echo "</td></tr>";
  }
  

?>
    </table>
  </body>
</html>

