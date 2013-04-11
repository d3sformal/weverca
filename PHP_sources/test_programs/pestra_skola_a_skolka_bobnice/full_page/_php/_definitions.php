<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
********************************************  Definice  ********************************************

 * Script obsahuje definice podpůrných a formátovacích funkcí
 * Při vložení do stránky provede rozklad adresy a iniciaci systémových proměnných

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

  $_inf['log'] = '';
  $_inf['error'] = false;
  $_inf['form'] = false;
  $_inf['ajax'] = false;
  $_inf['bottom'] = false;
  $_inf['refresh'] = '';
  $_inf['refrresh_mode'] = '0';
  $_inf['visitor'] = 0;
  
  $_inf['i'] = (isset($_GET['i']) and is_numeric($_GET['i'])) ? $_GET['i'] : 0;
  $_inf['s'] = (isset($_GET['s']) and is_numeric($_GET['s']) and $_GET['s'] > 0) ? $_GET['s'] : 1;
  

//Navrací ID stránky podle zadané adresy
function GetPageID()
{
  $id = UVOD_ID;
  if (isset($_GET['p']) && $_GET['p'] )
  {
    $i = split("_", $_GET['p']); 
    $id = $i[0];
    if (!is_numeric($id)) $id = 0;
  }
  return $id;
}

//Navrátí sformátovanou adresu, nebo její část dle zadání
function GetLink($link = true, $gp = true, $ga = true, $gf = true, $gi = true, $gwnd = true)
{
  global $_inf;
  if ($link) $adr = "$_inf[page]?";
  if ($gp and $_GET['p']) $adr .= "p=$_GET[p]";
  if ($ga and $_GET['a']) $adr .= "&amp;a=$_GET[a]";
  if ($gf and $_GET['f']) $adr .= "&amp;f=$_GET[f]";
  if ($gi and $_GET['i']) $adr .= "&amp;i=$_GET[i]";
  if ($gwnd and isset($_GET['wnd'])) $adr .= "&amp;wnd=$_GET[wnd]";
  return $adr;
}

//Navrací výčet, nebo konkrátní barvu podle indexu
function GetColor($id = -1)
{
  $colors = Array("", "green", "blue", "red", "yellow", "orange", "gray");
  
  if ($id == -1) return $colors;
  elseif ($id == 0 || $id > 5) return $colors[6];
  else return $colors[$id];
}

//True, pokud je uživatel přihlášen
function IsLoged(){
  if(!isset($_SESSION['id']) or !$_SESSION['id']) return false;
  if(!isset($_SESSION['name'])) return false;
  if(!isset($_SESSION['state'])) return false;
  return true;
}
  
//Funkce vytvoří ze vstupního řetězce adesu a navrátí ji
function MakeAdres($txt)
{
  $a = array(
  '/À/','/Á/','/Â/','/Ã/','/Ä/','/Å/','/Æ/','/Ç/','/Č/','/Đ/','/Ď/','/È/','/É/','/Ê/','/Ë/','/Ě/','/Ì/','/Í/','/Î/','/Ï/','/Ñ/','/Ò/','/Ó/','/Ô/',
  '/Õ/','/Ö/','/Ø/','/Ù/','/Ú/','/Û/','/Ü/','/Ů/','/Ý/','/Š/','/Ř/','/Ť/','/Ň/','/Ž/','/Þ/','/ß/','/à/','/á/','/â/','/ã/','/ä/','/å/','/æ/','/ç/',
  '/č/','/ð/','/ď/','/è/','/é/','/ê/','/ë/','/ě/','/ì/','/í/','/î/','/ï/','/ñ/','/ò/','/ó/','/ô/','/õ/','/ö/','/ø/','/ù/','/ú/','/û/','/ü/','/ů/',
  '/ý/','/ý/','/š/','/ř/','/ť/','/ň/','/ž/','/þ/','/ÿ/','/Ŕ/','/ŕ/','/&/','/@/','/\$/','/\§/','/\#/','/\*/','/\+/','/\|/','/!/','/÷/','/=/',
  '/\./','/,/',"/'/",'/\?/','/\:/','/</','/>/','/\[/','/\]/','/\{/','/\}/','/\(/','/\)/','/;/','/%/','/\\\/','/\//','/"/','/\s/'
  );
  $b = array(
   'a' , 'a' , 'a' , 'a' , 'a' , 'a' , 'a' , 'c' , 'c' , 'd' , 'd' , 'e' , 'e' , 'e' , 'e' , 'e' , 'i' , 'i' , 'i' , 'i' , 'n' , 'o' , 'o' , 'o',
   'o' , 'o' , 'o' , 'u' , 'u' , 'u' , 'u' , 'u' , 'y' , 's' , 'r' , 't' , 'n' , 'z' , 'b' , 's' , 'a' , 'a' , 'a' , 'a' , 'a' , 'a' , 'a' , 'c',
   'c' , 'd' , 'd' , 'e' , 'e' , 'e' , 'e' , 'e' , 'i' , 'i' , 'i' , 'i' , 'n' , 'o' , 'o' , 'o' , 'o' , 'o' , 'o' , 'u' , 'u' , 'u' , 'u' , 'u',
   'y' , 'y' , 's' , 'r' , 't' , 'n' , 'z' , 'b' , 'y' , 'R' , 'r' , 'a' , 'a' , 's' , 's' , 'x' , 'x' , 'x' , 'i' , '_' , '-' , '-' ,
   '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_' , '_'
   );

  $txt = preg_replace($a,$b,$txt);
  $txt = mb_strtolower($txt,'utf-8');
  $txt = urlencode ($txt);
  $txt = preg_replace("/%/","_",$txt);
  $txt = preg_replace("/_+/","_",$txt);
  if (mb_strlen($txt,"utf8") > 40) $txt = preg_replace("/^(.{40}).+$/",'\\1',$txt);
  return $txt;
}

//Zformátuje datum a čas z MYSQL výstupu
function GetDatetime($dat,$h = true)
{
  $datum = 0;
  if ($h)
  {
    $datum = preg_replace("/^(\d{4})-(\d{2})-(\d{2}) (\d{2}:\d{2}:\d{2}).*$/", "\\3.\\2.\\1 \\4", $dat);
    return $datum == "00.00.0000 00:00:00" ? 0 : $datum;
  }
  else
  {
    $datum = preg_replace("/^(\d{4})-(\d{2})-(\d{2}).*$/", "\\3.\\2.\\1", $dat);
    return $datum == "00.00.0000 00:00:00" ? 0 : $datum;
  }
}  

//Navrátí náhodný řetězec zadané délky složený z čísel, malých a velkých znaků abecedy
function RandomString($items)
{
  $vratit="";
  for ($i=1; $i <= $items ;$i++ )
  {
    $rnd = rand(1 , 62);
    switch($rnd)
    {
      case 10: $znak = '0'; break;
      case 11: $znak = 'a'; break; case 12: $znak = 'b'; break; case 13: $znak = 'c'; break;
      case 14: $znak = 'd'; break; case 15: $znak = 'e'; break; case 16: $znak = 'f'; break;
      case 17: $znak = 'g'; break; case 18: $znak = 'h'; break; case 19: $znak = 'i'; break;
      case 20: $znak = 'j'; break; case 21: $znak = 'k'; break; case 22: $znak = 'l'; break; 
      case 23: $znak = 'm'; break; case 24: $znak = 'n'; break; case 25: $znak = 'o'; break; 
      case 26: $znak = 'p'; break; case 27: $znak = 'q'; break; case 28: $znak = 'r'; break; 
      case 29: $znak = 's'; break; case 30: $znak = 't'; break; case 31: $znak = 'u'; break; 
      case 32: $znak = 'v'; break; case 33: $znak = 'w'; break; case 34: $znak = 'x'; break; 
      case 35: $znak = 'y'; break; case 36: $znak = 'z'; break; 
      case 37: $znak = 'A'; break; case 38: $znak = 'B'; break; case 39: $znak = 'C'; break; 
      case 40: $znak = 'D'; break; case 41: $znak = 'E'; break; case 42: $znak = 'F'; break; 
      case 43: $znak = 'G'; break; case 44: $znak = 'H'; break; case 45: $znak = 'I'; break;
      case 46: $znak = 'J'; break; case 47: $znak = 'K'; break; case 48: $znak = 'L'; break; 
      case 49: $znak = 'M'; break; case 50: $znak = 'N'; break; case 51: $znak = 'O'; break; 
      case 52: $znak = 'P'; break; case 53: $znak = 'Q'; break; case 54: $znak = 'R'; break; 
      case 55: $znak = 'S'; break; case 56: $znak = 'T'; break; case 57: $znak = 'U'; break; 
      case 58: $znak = 'V'; break; case 59: $znak = 'W'; break; case 60: $znak = 'X'; break; 
      case 61: $znak = 'Y'; break; case 62: $znak = 'Z'; break; 
      default: $znak = $rnd;
    }
    $vratit .= $znak;
  }
  return $vratit;
}


//Vykreslení smajlíka čsílo $x
//$y:  1- smajlík do textu; 2- smajlík jako tlačítko
//$z: číslo pole pro vložení smajlíka
function get_smajl($x,$y,$z = 3){
  $s = array (1 =>
    "Anděl 0:-)", "Úsměv :-)", "Smutný :-(", "Mrknutí ;-)", "Vyplazený jazyk :-P", "Tmavé brýle 8-)", "Smích :-D",
    "Červenání :-[", "Údiv :-O", "Polibek :-{}", "Pláč :,-(", "Mlčím :-X", "Řev :-@", "Naštvaný :-\\",
    "Unavený", "Líbání", "Menší smích", "Čert ]:-&gt;", "Poslech hudby [:-}", "Políben","Nevolnost :-!",
    "Neutrální výraz :-|", "STOP", "Růže @}-&gt;--", "Palec nahoru", "Pivo", "Zamilovaný", "BUM @=",
  );
  if(!isset($s[$x]))return "";
  $n = "sm".($x < 10?"0":"").$x;$w = $x != 16 ? 20 : 34;
  switch($y){
    case 1: return "<img src=\"_grafika/smajl/$n.png\" width=\"".$w."px\" height=\"20px\" alt=\"$s[$x]\" title=\"[$n] $s[$x]\" class=\"smajl\" />";
    case 2: return "<a href=\"javascript:insertAtCursor('col$z',' [$n]','',0);\"><img src=\"_grafika/smajl/$n.png\" width=\"".$w."px\" height=\"20px\" alt=\"$s[$x]\" title=\"[$n] $s[$x]\" /></a>";
  }
}


//formátování textu
function format_text($vstup){
  //výrazy pro nahrazení tučnosti, kurzívy a podtržení
  $reg[] = "/\[b\](.+?)?\[\/b\]/si"; $nah[] = "<b>\\1</b>";
  $reg[] = "/\[i\](.+?)?\[\/i\]/si"; $nah[] = "<i>\\1</i>";
  $reg[] = "/\[u\](.+?)?\[\/u\]/si"; $nah[] = "<u>\\1</u>";
  $reg[] = "/\n/"; $nah[] = "<br />";
  $reg[] = "/\r/"; $nah[] = ""; 
  $reg[] = "/\n?\[nadpis\](.+?)?\[\/nadpis\](<br \/>)?/si"; $nah[] = "</p><h3>\\1</h3><p>";
  $reg[] = "/\[nastred\](.+?)?\[\/nastred\](<br \/>)?/si"; $nah[] = "</p><p class=\"center\">\\1</p><p>";
  $reg[] = "/\[vpravo\](.+?)?\[\/vpravo\](<br \/>)?/si"; $nah[] = "</p><p class=\"pravo\">\\1</p><p>";
  $reg[] = "/\[vlevo\](.+?)?\[\/vlevo\](<br \/>)?/si"; $nah[] = "</p><p class=\"levo\">\\1</p><p>";
  $reg[] = "/\[barva=(.+?)\](.+?)?\[\/barva\]/si"; $nah[] = "<span style=\"color:\\1\">\\2</span>";
  $reg[] = "/\[velikost=(.+?)\](.+?)?\[\/velikost\]/si"; $nah[] = "<span style=\"font-size:\\1\">\\2</span>";
  $reg[] = "/\[pismo=(.+?)\](.+?)?\[\/pismo\]/si"; $nah[] = "<span style=\"font-family:'\\1'\">\\2</span>";
  $reg[] = "/\[img\](.+?)?\[\/img\]/si"; $nah[] = "<img src=\"\\1\" alt=\"\\1\" class=\"textImg\" />";
  $reg[] = "/\[seznam\](.+?)?\[\/seznam\]/si"; $nah[] = "</p><p><ul>\\1</ul></p><p>";
  $reg[] = "/\[cislovany\](.+?)?\[\/cislovany\]/si"; $nah[] = "</p><p><ol>\\1</ol></p><p>";
  $reg[] = "/(<br \/>)*\[polozka\](.+?)?\[\/polozka\](<br \/>)*/si"; $nah[] = "<li>\\2</li>";
  $reg[] = "/\[tabulka\][ \n\r\t\f]*(.+?)?\[\/tabulka\](<br \/>)?/si"; $nah[] = "</p><div><table align=\"center\" class=\"list\" >\\1</table></div><p>";
  $reg[] = "/(<br \/>)*\[radek\](.+?)?\[\/radek\](<br \/>)*/si"; $nah[] = "<tr>\\2</tr>";
  $reg[] = "/(<br \/>)*\[bunka\](.+?)?\[\/bunka\](<br \/>)*/si"; $nah[] = "<td>\\2</td>";
  $reg[] = "/\[odkaz=(index.+?)\](.+?)\[\/odkaz\]/si";
  $nah[] = "<a href=\"\\1\" rel=\"nofollow\">\\2</a>";
  $reg[] = "/\[odkaz=(http:\/\/)?((www\.)?.+?)\](.+?)\[\/odkaz\]/si";
  $nah[] = "<a href=\"http://\\2\" target=\"_blank\" rel=\"nofollow\">\\4</a>";
  $reg[] = "/(^| |>)(((http|ftp)s?:\/\/)[_;a-zA-Z0-9.?~%#&@'\\=+\/-]+)/si";
  $nah[] = " <a href=\"\\2\" target=\"_blank\" rel=\"nofollow\">\\2</a>";
  $reg[] = "/<br \/>((<br \/>)+)?/"; $nah[] = "</p>\\1<p>";
  $reg[] = "/(<p><\/p>)/si"; $nah[] = "";
 
  for($x = 1;$y = get_smajl($x,1); $x++){$reg[] = "/\[sm".($x < 10?"0":"")."$x\]/si";$nah[] = $y;}

  $vstup = stripcslashes($vstup);
  $vstup = "<p>$vstup</p>";
  $vystup = preg_replace($reg,$nah,$vstup);
  return $vystup;
}
  
?>