<?
/***************************************************************************************************
*************************  Vyvolávací stránka pro zobrazení náhledu obrázku  ***********************

 * Script je ze stránek zavolán při kliknutí na zvětšovaný obrázek, pokud není aktivní javascript.
 * Po zavolání provede iniciaci a vložení ostatních scriptů, výběr z databáze a zobrazení obrázku

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

  //zámek scriptů
  define("AUTORIZACE","Q.&dJhůs63d5dS=d56sLc5%");
  define("IMG_WIDTH" , 900);
  define("IMG_HEIGHT", 900);
  $_inf['page'] = "image.php";
  
  session_start();
  require_once "conf.php";
  require_once "_php/_definitions.php";
  require_once "_php/_database.php";
  require_once "_php/_pages.php";
  
  $pid = GetPageID();
  
  //Výběr dat  změna velikosti do okna
  $poz = $_inf['i'] - 1;
  $sql = sql_select('galery','galery.id, galery.datum, galery.adr, galery.pid, pages.name, pages.adr as adres, pages.ct',
    "left join pages on galery.pid = pages.id where galery.pid = '$pid' and galery.zob = '1' and pages.deleted = '0' order by galery.id"
    , "$poz, 1"
  );
  $data = mysql_fetch_array($sql);
  $vel = getimagesize("galery/$data[id]_$data[adr].jpg");
  $wid = $vel[0]; $hei = $vel[1];
  if ($wid > IMG_WIDTH) {$hei = (int)(($hei / $wid) * IMG_WIDTH) ; $wid = IMG_WIDTH ;}
  if ($hei > IMG_HEIGHT){$wid = (int)(($wid / $hei) * IMG_HEIGHT); $hei = IMG_HEIGHT;}
  
  echo "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">";
  echo "\n<html><head>";
  echo "\n  <meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\">";
  echo "\n  <title>$data[name]: $poz. obrázek - Pestřá základní škola Bobnice :-))</title>";
  echo "\n  <link rel=\"stylesheet\" type=\"text/css\" href=\"_css/main.css\">";
  echo "\n</head><body><div id=\"main\"><div class=\"page_border\">&nbsp;</div>";
  
  //Zobrazení obrázku
  if ($data)
  {
    echo "\n  <table class=\"items\" align=\"center\" >";
    echo "\n    <tr><th>$data[name] | $_inf[i]. obrázek z $data[ct]</th></tr>";
    echo "\n    <tr><td>";
    $adr = $_inf['i'] > 1 ? $_inf['i'] - 1 : $data['ct'];
    echo "<a href=\"image.php?p=$_GET[p]&i=$adr\" title=\"Předchozí\" ><img src=\"_graphics/icon/32_larrow_red.png\"  alt=\"Předchozí\" /></a> ";
    
    $adr = $_inf['i'] < $data['ct'] ? $_inf['i'] + 1 : 1;
    echo "<a href=\"image.php?p=$_GET[p]&i=$adr\" title=\"Další\" ><img src=\"_graphics/icon/32_rarrow_red.png\"  alt=\"Další\" /></a> ";
    echo "</td></tr>";
    
    echo "\n    <tr><td><img src=\"galery/$data[id]_$data[adr].jpg\" alt=\"$data[name]: Obrázek $_inf[i]\" width=\"$wid\" height=\"$hei\" ></td></tr>";
    echo "</table>";
  }
  else  Alert("Obrázek neexistuje");
  
  echo "\n<div class=\"page_border\">&nbsp;</div></div>";
  echo "\n\n<div id=\"bottom\"><a href=\"index.php\">Pestrá škola a školka Bobnice</a> | Šíře stránky: 1000px | Pro bezchybný chod mějte aktivován <b>Javascript</b> a <b>CSS</b> | Testováno na:";
  echo " <img src=\"_graphics/icon/20_chrome.png\" alt=\"Google Chrome v. 6.0.472.55\" title=\"Google Chrome v. 6.0.472.55\" />";
  echo " <img src=\"_graphics/icon/20_firefox.png\" alt=\"Mozzila Firefox v. 3.6.9\" title=\"Mozzila Firefox v. 3.6.9\" />";
  echo " <img src=\"_graphics/icon/20_IE8.png\" alt=\"Internet Explorer 8\" title=\"Internet Explorer 8\" />";
  echo " <img src=\"_graphics/icon/20_opera.png\" alt=\"Opera v. 10.62\" title=\"Opera v. 10.62\" />";
  echo "<br>Veškerý obsah stránek Pestrá Škola je majetkem administrátorů a nesmí být bez jejich svolení dále kopírován.";
  echo "<br>Zdrojový kód PHP, CSS, HTML a grafiku webu vytvořil Pavel Baštecký © září 2010 | Případné chyby zobrazení hlaste na: <img src=\"_graphics/anebril.png\" alt=\"anebril &lt;zavináč&gt; seznam.cz\" />";
  echo "</div>";
  echo "</body></html>";
?>