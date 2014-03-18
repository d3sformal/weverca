<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
******************************  Zobrazení souhrné statistiky stránek  ******************************

 * Pracuje s MYSQL tabulkou VISITS a PAGES
 * Počítá a zobrazuje vybrané statistické údaje

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

//Zobrazení stránky
function Body()
{

/* Statistika stránek - počty rubrik, článků, obrázků.... */
  $pages = array(0,0,0,0,0,0,0);
  $data = mysql_fetch_array( sql_select('pages', 'count(*) as pages', "where deleted = 0") );
  $pages[0] = $data['pages'];
  
  $data = mysql_fetch_array( sql_select('comments', 'count(*) as comments', "") );
  $pages[1] = $data['comments'];

  $data = mysql_fetch_array( sql_select('galery', 'count(*) as galery', "where zob = 1") );
  $pages[2] = $data['galery'];
  
  $galery = scandir("galery");
  foreach ($galery as $file)
  {
    if (is_file("galery/". $file))
      $pages[3] += filesize("galery/". $file);
    if (is_file("galery/thumbs/". $file))
      $pages[3] += filesize("galery/thumbs/". $file);
  }
  
  $pages[3] = round($pages[3] / 1000000, 3) ." MB";
  
  echo "\n<table class=\"list\" align=\"center\" width=\"690px\" ><tr><th colspan=\"7\" >Statistika webu</th></tr>";
  echo "\n <tr><th>Celkový počet stránek</th><th>Počet komentářu v diskuzi</th><th>Počet obrázků v albech</th><th>Celková velikost galerie na disku</th></tr>";
  echo "<tr>";
  for ($x = 0; $x < 4; $x++) echo "<td>$pages[$x]</td>";
  echo "</tr>\n</table><br />";
  
  
/* Statistika návštěvnosti - dnes, včera, týdenní, měsíční, celková */
  $month = date('Y-m-d', strtotime('-1 month'));
  $week = date('Y-m-d', strtotime('-1 week'));
  $yesterday = date('Y-m-d', strtotime('-1 day'));
  $today = date('Y-m-d');
  
  $sql = sql_select('visits', "DATE_FORMAT(datum, '%Y-%m-%d') as day, count(*) as view, sum(visit) as visit",
    "where datum >= '$month' group by day order by datum desc"
  );
  $data = mysql_fetch_array( sql_select('visits', "count(*) as view, sum(visit) as visit",
    ""
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
  echo "\n  <tr><th>&nbsp;</th><td>". date("d.m.Y") ."</td><td>". date('d.m.Y', strtotime('-1 day')) ."</td><td>od ". date('d.m.Y', strtotime('-1 week')) ."</td><td>od ". date('d.m.Y', strtotime('-1 month')) ."</td><td>&nbsp;</td></tr>";
  
  echo "\n  <tr><th>Návštěvníků</th>";
  for ($x = 0; $x < 5; $x++) echo "<td>$visit[$x]</td>";
  echo "</tr>";
  
  echo "\n  <tr><th>Zobrazení stránek</th>";
  for ($x = 0; $x < 5; $x++) echo "<td>$view[$x]</td>";
  echo "</tr>";
  
  echo "\n</table><br />";
  
  
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
    echo "\n <tr><th>&nbsp;</th><td >Ostatní</td><td>$others</td><td>$percentil %</td></tr>";
  }

  echo "\n</table>";
}

?>