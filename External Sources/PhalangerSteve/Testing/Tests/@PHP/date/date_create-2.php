[expect php]
[file]
<?php 
  if (!function_exists('date_create')) die("SKIP");

date_default_timezone_set("GMT");
$d = date_create("2005-07-18 22:10:00 +0400");
echo $d->format(DateTime::RFC822), "\n";
?>