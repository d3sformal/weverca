[expect php]
[file]
<?php
include('Phalanger.inc');
if ("fr_FR" != setlocale(LC_CTYPE, "fr_FR")) {
  die("skip setlocale() failed\n");
}
setlocale(LC_ALL, 'fr_FR');
$table = array("AB" => "Alberta",
"BC" => "Colombie-Britannique",
"MB" => "Manitoba",
"NB" => "Nouveau-Brunswick",
"NL" => "Terre-Neuve-et-Labrador",
"NS" => "Nouvelle-�cosse",
"ON" => "Ontario",
"PE" => "�le-du-Prince-�douard",
"QC" => "Qu�bec",
"SK" => "Saskatchewan",
"NT" => "Territoires du Nord-Ouest",
"NU" => "Nunavut",
"YT" => "Territoire du Yukon");
asort($table, SORT_LOCALE_STRING);
__var_dump($table);
?>
