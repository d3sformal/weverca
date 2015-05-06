<?
// zabranime cashovani
header('Cache-Control: no-cache');
header('Pragma: no-cache');
header('Content-type: image/png'); 

  $re_st = -2;                                                         //Pozice zaибtku imaginбrnн ибsti
  $im_st = 1.2;                                                          //Pozice zaибtku reбlnй ибsti
  $incr = 0.005;                                                           //Mмшнtko pшнrustku - 1px = 1 / 20
  $max_iter = 100;                                                          //Maximum iteracн pro vэpoиet
  $x_max = 600;
  $y_max = 480;
  
  $re = $re_st;
  $im = $im_st;
  $iter = 0;
  $_re = $_im = 0;

$imag = @imagecreatetruecolor($x_max, $y_max) or die("Cannot Initialize new GD image stream - new image");
$bl = imagecolorallocate($imag,0,0,0);
$wh = imagecolorallocate($imag,255,255,255);

for($y = 1; $y <= $y_max; $y++){
  $re = $re_st;
  
  for($x = 1; $x <= $x_max; $x++){
    
    $_re = $re; $_im = $im; $iter = 0;
    do{
      $re2 = $_re * $_re; $im2 = $_im * $_im;
      $_im = 2 * $_re * $_im + $im;
      $_re = $re2 - $im2 + $re;
      $iter++;
    }
    while($iter < $max_iter and $re2 + $im2 < 4);

    if ($iter == $max_iter)imagesetpixel($imag,$x,$y,$bl);
    else imagesetpixel($imag,$x,$y,$wh);

    
    $re += $incr;
  }
  
  $im -= $incr;
}


imagepng($imag); 
imagedestroy($imag);
?>