<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
********************************************  Definice  ********************************************

 * Script obsahuje definice podpůrných a formátovacích funkcí
 * Při vložení do stránky provede rozklad adresy a iniciaci systémových proměnných

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

  $_inf['log'] = '';
  $_inf['error'] = false;
  $_inf['form'] = false;
  $_inf['header'] = false;
  $_inf['admin'] = false;
  $_inf['ajax'] = false;
  $_inf['bottom'] = false;
  $_inf['refresh'] = '';
  $_inf['refrresh_mode'] = '0';
  $_inf['visitor'] = 0;
  
  $_inf['i'] = (isset($_GET['i']) and is_numeric($_GET['i'])) ? $_GET['i'] : 0;
  $_inf['s'] = (isset($_GET['s']) and is_numeric($_GET['s']) and $_GET['s'] > 0) ? $_GET['s'] : 1;
  
  define('ADD_FORM', 1);
  define('DELETE_FORM', 2);
  define('TRUNC_FORM', 3);
  define('MOVE_FORM', 4);
  define('EDIT_FORM', 5);
  define('STATE_FORM', 6);
  define('PROPERTIES_FORM', 7);
  
  $_inf['f'] = 0;
  switch ($_GET['f'])
  {
    case 'add':         $_inf['f'] = 1; break;
    case 'delete':      $_inf['f'] = 2; break;
    case 'trunc':       $_inf['f'] = 3; break;
    case 'move':        $_inf['f'] = 4; break;
    case 'edit':        $_inf['f'] = 5; break;
    case 'state':       $_inf['f'] = 6; break;
    case 'properties':  $_inf['f'] = 7; break;
  }
  
  define('GALERY_ADRESS', 1);
  define('MENU_ADRESS', 2);
  define('TEXT_ADRESS', 3);
  define('COMMENTS_ADRESS', 4);
  define('USER_ADRESS', 5);
  define('RUBRIC_ADRESS', 6);
  
  $_inf['a'] = 0;
  switch ($_GET['a'])
  {
    case 'galery':      $_inf['a'] = 1; break;
    case 'menu':        $_inf['a'] = 2; break;
    case 'text':        $_inf['a'] = 3; break;
    case 'comments':    $_inf['a'] = 4; break;
    case 'user':        $_inf['a'] = 5; break;
    case 'rubric':      $_inf['a'] = 6; break;
  }
  

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
function GetLink($link = true, $gp = true, $ga = true, $gf = true, $gi = true, $gs = true, $gb = true)
{
  global $_inf;
  if ($link) $adr = "$_inf[page]?";
  if ($gp and $_GET['p']) $adr .= "p=$_GET[p]";
  if ($ga and $_GET['a']) $adr .= "&amp;a=$_GET[a]";
  if ($gf and $_GET['f']) $adr .= "&amp;f=$_GET[f]";
  if ($gi and $_GET['i']) $adr .= "&amp;i=$_GET[i]";
  if ($gs and $_GET['s']) $adr .= "&amp;s=$_GET[s]";
  if ($gb and $_GET['b']) $adr .= "&amp;b=$_GET[b]";
  return $adr;
}

//True, pokud je uživatel přihlášen
function IsLoged()
{
  if(!isset($_SESSION['id']) or !$_SESSION['id']) return false;
  if(!isset($_SESSION['name'])) return false;
  if(!isset($_SESSION['state'])) return false;
  return true;
}
  
//Funkce vytvoří ze vstupního řetězce adesu a navrátí ji
function MakeAdres($txt)
{
  $txt = urlencode ($txt);
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

//Funkce provede dotaz na databázi a uloží data pro tabulku prvního a druhého menu
function Menu()
{
  global $_page;
  
  $menu[0][1] = 0;
  $menu[0][2] = 1;
  
  //Výběr položek - na jeden dotaz vybírá údaje do obou menu
  $rows = MAIN_MENU_ITEMS; $items = MENU_ITEMS + 1;
  $dotaz = IsLoged() ? '' : ' and acces = 0';
  $lvl = $_page['level'] > 1 ? $_page['level'] : 2;
  $sql = sql_select('pages','id, name, adr, pid, level',
    "where deleted = '0'$dotaz and pos > '0' and (level = 1 or level = '$lvl' and mid = '$_page[mid]') order by level, pos"
  );
  
  $count = mysql_num_rows($sql);
  $data = mysql_fetch_array($sql);
  
  if ($data)
  {
    for ($x = 0; $data && $data['level'] == 1; $x++)
    {
      $menu[1][$x]['adr'] = $data['id']. "_" .$data['adr'];
      $menu[1][$x]['name'] = $data['name'];
      $data = mysql_fetch_array($sql);
    }
    $menu[0][1] = $x;   
    
    for ($x = 0; $data; $x++)
    {
      $menu[2][$x]['adr'] = $data['id']. "_" .$data['adr'];
      $menu[2][$x]['name'] = $data['name'];
      $data = mysql_fetch_array($sql);
    }
    $menu[0][2] = $x;  
  } 
  
  return $menu;
}

//Funkce provede dotaz na databázi a navrátí informace o nadřazené stránce
function GetParent()
{
  global $_page;
  
  //Výběr položek
  $rows = MAIN_MENU_ITEMS; $items = MENU_ITEMS + 1;
  $dotaz = IsLoged() ? '' : ' and acces = 0';
  $lvl = $_page['level'] - 1;
  $data2 = mysql_fetch_array(sql_select('pages','id, name, adr, pid, level',
    "where deleted = '0'$dotaz and pos > '0' and id = '$_page[pid]' and level = '$lvl' order by level, pos"
  ));   
  
  $parent[0] = 0;
  $parent['adr'] = "";
  $parent['name'] = "";
  
  if ($data2 && $data2['id'] == $_page['pid'] && $data2['level'] == $_page['level'] - 1)
  {  
    $parent[0] = 1;
    $parent['adr'] = $data2['id']. "_" .$data2['adr'];
    $parent['name'] = $data2['name'];
  }  
  
  return $parent;
}

//Načte informace o otevřené stránce
function GetPageData()
{
  //Tačítka přihlášení a odhlášení
  switch($_GET['o'])
  {
    case 'login': return GetLoginData(); break;
    case 'logout': return GetLogoutData(); break;
  }
  
  //Dotaz na stránku
  $data = mysql_fetch_array( 
    sql_select(
      'pages',
      'pages.id, pages.name, pages.title, pages.acces, pages.datum, pages.pid, pages.mid, pages.level, pages.script, pages.lck, '
      .'pages.pos, pages.img, pages.ct_menu, pages.ct_images, pages.tid, texts.text as text',
      "left join texts on texts.id = pages.tid where pages.id = '". GetPageID() ."' and pages.deleted = '0'",
      1)
  );
  
  //Stránka nenalezena
  if (!$data){$data['script'] = "error"; $data['id'] = -1;}
  //Výzva k přihlášení
  elseif ($data['acces'] and LCK && !IsLoged()) { $data = GetLoginData($data['name'], $data['id'], $data['adr']);}
  //Bude zobrazen článek
  elseif($data['script'] == '') {$data['script'] = 'article';}

  return $data;
}

//Navrátí data pro přihlášení
function GetLoginData($name = "", $id = -1, $adr = 'login')
{
  return Array(
    'id' => $id,
    'title' => $name ? "$name - pro vstup do této části stránek musíte být přihlášen" : "Přihlášení",
    'acces' => 0,
    'script' => "profil",
    'adr' => $adr,
  );
}

//Data pro odhlášení
function GetLogoutData()
{
  return Array(
    'id' => 0,
    'title' => "Odhlášení",
    'acces' => 0,
    'script' => "profil",
    'adr' => 'logout'
  );
}

//Obnoví stránku, případně přesměruje na danou adresu
function MakeRefresh($adr = "", $mode = '0')
{
  global $_inf;
  if (!$adr) $adr = "index.php?p=$_GET[p]";
  $_inf['refresh'] = $adr;
  $_inf['refrresh_mode'] = $mode;
}

//Vypíše dialog přesměrování
function IsRefreshed($text)
{
  global $_inf;
  
  if ($_inf['refresh'])
  {
    $js =$_inf['page'] == "form" ? 1 : 0;
    

?>
          <?Alert($text);?>

          <div class="refresh">
            <a href="<?=$_inf['refresh']?>" >Za <span id="refresh_number"><?=REFRESH_TIME?></span>s dojde k automatickému přesměrování (přesměrování urychlíte kliknutím na tento text)</a>
          </div>
          
          <script type="text/javascript">RefreshCuntdown(<?=REFRESH_TIME?>);</script>  
<?
    return true;
  }
  return false;
}

//Vykreslení hlášky
function Alert($text)
{
  echo "<h2>$text</h2>";
}

//Vykreslení tlačítka do panelu nástrojů; Vstupy:
// Adresa odkazu; popisek tlačítka; ikona; potlačení vyvolání okna formuláře
function ToolButton($adress, $text, $separator = " | ", $method = 0)
{
  $js = $method ?  " onclick=\"return $method('$adress'); \"" : "";
  echo "<a href=\"$_inf[page]?$adress\" title=\"$text\"$js>$text</a>$separator";
}

//Zobrazení stránkovacích tlačítek; Vstup:
//Počet zobrazených položek; Celkem položek v databázi; limit na stránku; adresa
//Navrací false pro prázdný seznam, opačně true
function ShowSwitcher($showed, $items, $limit, $adres = "")
{
  global $_page, $_inf;

  if (!$showed)   //Prázdný seznam
  {
    if ($items) alert("Tento seznam nemá tolik částí");
    return false;
  }
  
  //Čeština
  if ($showed == 1) $cz = "á položka";
  elseif ($showed >= 2 and $showed <= 4) $cz = "é položky";
  else $cz = "ých položek";
?>

          <div class="swap"><?=$showed?> zobrazen<?=$cz?> z celkového počtu <?=$items?></div>
<?
  
  //Vykreslení číselníku
  if ($items > $limit)
  {
    if (!$adres) $adres = GetLink(false, true, true, true, true, false);
    
    //Maximální počet zobrazených čísel (zbytek se ořízne)
    $size = SW_SIZE * 2;
    //Číslo poslední stránky
    $max = (int)($items / $limit) + ($items % $limit ? 1 : 0); 
    //Číslo začátku vykreslování
    $s = ($_inf['s'] <= SW_SIZE or $max <= $size) ? 1 : 
              (($_inf['s'] + SW_SIZE > $max) ? $max - $size : $_inf['s'] - SW_SIZE);
    
?>
          <div class="swap">
            <a href="<?=$_inf['page']?>?<?=$adres?>&amp;s=1" title="První" >&lt;&lt;</a>
            <a href="<?=$_inf['page']?>?<?=$adres?>&amp;s=<?=$adr?>" title="Předchozí">&lt;</a>
<?
          for ($x = 0; $x < $max and $x < $size + 1; $x++, $s++)
          {
            $b = $_inf['s'] == $s ? " style=\"font-weight:bold\"" : "";
?>
            <a href="<?=$_inf['page']?>?<?=$adres?>&amp;s=<?=$s?>" title="Následující"<?=$b?>><?=$s?></a>
<?        }
    
          $adr = $_inf['s'] < $max ? $_inf['s'] + 1 : 1;
?>
            <a href="<?=$_inf['page']?>?<?=$adres?>&amp;s=<?=$adr?>" title="Následující">&gt;</a>
            <a href="<?=$_inf['page']?>?<?=$adres?>&amp;s=<?=$max?>" title="Poslední">&gt;&gt;</a>
          </div>
<?
  }
  return true;
}
?>