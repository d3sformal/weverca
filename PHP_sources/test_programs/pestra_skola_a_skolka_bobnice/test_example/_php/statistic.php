<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
******************************  Zobrazení souhrné statistiky stránek  ******************************

 * Pracuje s MYSQL tabulkou VISITS a PAGES
 * Počítá a zobrazuje vybrané statistické údaje

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

//Zobrazení stránky
function Body()
{

/* Statistika stránek - počty rubrik, článků, obrázků.... */
  $sql = sql_select('pages', 'count(*) as pages, script', "where deleted = 0 group by script order by script");
  
  $pages = array(0,0,0,0,0,0,0);
  while($data = mysql_fetch_array($sql))
  {
    switch ($data['script'])
    {
      case 'rubric': $pages[0] = $data['pages']; break;
      case '': $pages[1] = $data['pages']; break;
      case 'galery': $pages[2] = $data['pages']; break;
      case 'album': $pages[3] = $data['pages']; break;
    }
    $pages[4] += $data['pages'];
  }
  $data = mysql_fetch_array( sql_select('galery', 'count(*) as galery', "where zob = 1") );
  $pages[5] = $data['galery'];
  $pages[6] = "neznámo";
  
  /*$galery = scandir("galery");
  foreach ($galery as $file)
  {
    if (is_file("galery/". $file))
      $pages[6] += filesize("galery/". $file);
    if (is_file("galery/thumbs/". $file))
      $pages[6] += filesize("galery/thumbs/". $file);
  }*/
  
  $pages[6] = round($pages[6] / 1000000, 3) ." MB";
  
  echo "\n<table class=\"list\" align=\"center\" width=\"690px\" ><tr><th colspan=\"7\" >Statistika webu</th></tr>";
  echo "\n <tr><th>Rubrik</th><th>Článků</th><th>Galerií</th><th>Alb v galeriích</th><th>Celkový počet stránek</th><th>Obrázků v albech</th><th>Celková velikost galerie</th></tr>";
  echo "<tr>";
  for ($x = 0; $x < 7; $x++) echo "<td>$pages[$x]</td>";
  echo "</tr>\n</table>";
  
  
/* Statistika návštěvnosti - dnes, včera, týdenní, měsíční, celková */
  $month = date('Y-m-d', strtotime('-1 month'));
  $week = date('Y-m-d', strtotime('-1 week'));
  $yesterday = date('Y-m-d', strtotime('-1 day'));
  $today = date('Y-m-d');
  
  $sql = sql_select('visits', "DATE_FORMAT(datum, '%Y-%m-%d') as day, count(*) as view, sum(visit) as visit",
    "where datum >= '$month' group by day order by datum desc"
  );
  $data = mysql_fetch_array( sql_select('visits', "DATE_FORMAT(datum, '%d.%m.%Y') as day, count(*) as view, sum(visit) as visit",
    "order by datum"
  ));
  
  $start = 0;
  $visit = array(0,0,0,0,0);
  $view = array(0,0,0,0,0);
  if ($data) {$visit[4] = $data['visit']; $view[4] = $data['view']; $start = $data['day']; }
  
  while($data = mysql_fetch_array($sql))
  {
    if ($data['day'] == $today) {$visit[0] = $data['visit']; $view[0] = $data['view']; }
    if ($data['day'] == $yesterday) {$visit[1] = $data['visit']; $view[1] = $data['view']; }
    if ($data['day'] >= $week) {$visit[2] += $data['visit']; $view[2] += $data['view'];}
    $visit[3] += $data['visit']; $view[3] += $data['view'];
  }

  
  echo "\n<table class=\"list\" align=\"center\" ><tr><th colspan=\"6\" >Návštěvnost webu</th></tr>";
  echo "\n  <tr><td width=\"150px\">&nbsp;</td><th width=\"100px\" >Dnes</th><th width=\"100px\">Včera</th><th width=\"100px\">Týden</th><th width=\"100px\">Měsíc</th><th width=\"100px\">Celkem</th></tr>";
  echo "\n  <tr><td>&nbsp;</td><td>". date("d.m.Y") ."</td><td>". date('d.m.Y', strtotime('-1 day')) ."</td><td>od ". date('d.m.Y', strtotime('-1 week')) ."</td><td>od ". date('d.m.Y', strtotime('-1 month')) ."</td><td>od $start</td></tr>";
  
  echo "\n  <tr><th>Návštěvníků</th>";
  for ($x = 0; $x < 5; $x++) echo "<td>$visit[$x]</td>";
  echo "</tr>";
  
  echo "\n  <tr><th>Zobrazení stránek</th>";
  for ($x = 0; $x < 5; $x++) echo "<td>$view[$x]</td>";
  echo "</tr>";
  
  echo "\n</table>";
  
  
/* Statistika podílu návštěvnosti u různých částí webu */
  $sql = sql_select('visits', "count(*) as view, pages.id, pages.name, pages.adr",
    "left join pages on pages.id = visits.pid where visits.pid != 0 group by visits.pid order by view desc", 10
  );
  
  echo "\n<table class=\"list\" align=\"center\" ><tr><th colspan=\"4\" >Nejnavštěvovanější stránky</th></tr>";
  echo "\n  <tr><th width=\"80px\">Pořadí</th><th width=\"280px\">Jméno stránky</th><th width=\"150px\">Počet návštěv</th><th width=\"150px\">Podíl na návštěvnosti</th></tr>";
  
  $others = $all = $view[4];
  for($x = 1; $data = mysql_fetch_array($sql); $x++)
  {
    $others -= $data['view'];
    $percentil = round( ($data['view'] * 100) /$all , 2);
    echo "\n <tr><th>$x</th><td><a href=\"index.php?p=$data[id]_$data[adr]\" >$data[name]</a></td><td>$data[view]</td><td>$percentil %</td></tr>";
  }
  if ($others)
  {
    $percentil = round( ($others * 100) /$all , 2);
    echo "\n <tr><th colspan=\"2\" >Ostatní</th><td>$others</td><td>$percentil %</td></tr>";
  }

  echo "\n</table>";
}

?>