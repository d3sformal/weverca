<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
**********************************  Funkce pro zobrazení stránek  **********************************

 * Script obsahuje funkce pro zobrazení prvků stránek

MYSQL tabulka pro uložení stránek
 * ID, jméno, adresa, nadpis, náhledový text, ID náhledového obrázku, počet položek, barevné schéma,
 * ikona, datum vytvoření, ID nadřazené stránky, pozice v menu, prováděcí PHP script,
 * přístupnost (1 = pouze pro přihlášené), zámek (1 = nelze editovat), příznak smazáno (1 = smazáno)
 * Vložení důležitých stránek - úvod, administrace, hlavní menu, změna přihlašovacích údajů
 * správa administrátorů, statistika
 * Zbytek si vytvoří administrátoři     

CREATE TABLE IF NOT EXISTS `pages` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(40) COLLATE utf8_czech_ci NOT NULL,
  `adr` varchar(50) COLLATE utf8_czech_ci NOT NULL,
  `title` varchar(100) COLLATE utf8_czech_ci NOT NULL,
  `text` varchar(200) COLLATE utf8_czech_ci NOT NULL,
  `img` int(10) unsigned NOT NULL DEFAULT '0',
  `ct` smallint(5) unsigned NOT NULL DEFAULT '0',
  `color` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `icon` varchar(15) COLLATE utf8_czech_ci NOT NULL,
  `datum` datetime NOT NULL,
  `pid` int(10) unsigned NOT NULL DEFAULT '0',
  `pos` int(10) unsigned NOT NULL DEFAULT '0',
  `script` varchar(10) COLLATE utf8_czech_ci NOT NULL DEFAULT '',
  `acces` tinyint(4) NOT NULL DEFAULT '0',
  `lck` tinyint(4) NOT NULL DEFAULT '0',
  `deleted` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=utf8 COLLATE=utf8_czech_ci AUTO_INCREMENT=7 ;

INSERT INTO `pages` (`id`, `name`, `title`, `text`, `img`, `ct`, `color`, `icon`, `adr`, `datum`, `pid`, `pos`, `script`, `acces`, `lck`, `deleted`) VALUES
(1, 'Úvod', 'Úvodní stránka', 'Vítejte na stránkách pestré školy.', 0, 0, 0, 'brick_house', 'uvod', '2010-08-30 11:05:50', 0, 0, '', 0, 1, 0),
(2, 'Administrace', '', '', 0, 4, 0, 'key', 'administrace', '2010-08-30 11:06:25', 0, 0, 'rubric', 1, 1, 0),
(3, 'Hlavní menu', '', '', 0, 0, 0, 'folder', 'hlavni_menu', '2010-08-30 14:00:42', 2, 2, 'main_menu', 0, 1, 0),
(4, 'Změna přihlašovacích údajů', '', '', 0, 0, 0, 'key2', 'zmena_prihlasovacich_udaju', '2010-09-08 13:57:01', 2, 1, 'profil', 1, 1, 0),
(5, 'Správa administrátorů', '', '', 0, 0, 0, 'couple', 'sprava_administratoru', '2010-09-08 13:57:24', 2, 3, 'users', 1, 1, 0),
(6, 'Statistika stránek', '', '', 0, 0, 0, 'statistic', 'statistika_stranek', '2010-09-09 15:31:38', 2, 4, 'statistic', 0, 1, 0);

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/


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
  $data = mysql_fetch_array( sql_select('pages','id, name ,title ,color ,icon ,acces ,datum ,pid ,script ,lck ,pos ,img, ct',
    "where id = '". GetPageID() ."' and deleted = '0'",
    1
  ));
  
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
    Alert($text);
    
    if ($_inf['page'] == "form.php")
      echo "<p class=\"refresh\"><a href=\"$_inf[refresh]\"  onclick=\"MakeRefresh(this.href, $_inf[refrresh_mode]); return false;\" >Za <span id=\"refresh_number\">5</span>s dojde k automatickému přesměrování</a></p>";
    else
      echo "<div class=\"refresh\"><a href=\"$_inf[refresh]\" >Za <span id=\"refresh_number\">5</span>s dojde k automatickému přesměrování</a></div>";
    
    echo "\n<script type=\"text/javascript\">RefreshCuntdown(5);</script>";
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
function ToolButton($adress, $text, $icon, $popup = true, $method = 'ShowForm')
{
  $js = $popup ?  " onclick=\"return $method('$adress'); \"" : "";
  echo "<a href=\"$_inf[page]?$adress\" title=\"$text\"$js ><img src=\"_graphics/icon/20_$icon.png\" alt=\"$text\" /></a>";
}

//HTML hlavička dokumentu
function Head()
{
  global $_page;
  $color = GetColor($_page["color"]);
  
  //echo "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">";
  //echo "\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\"><head>";
  //echo "<!DOCTYPE XHTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">";
  echo "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
  echo "\n<html xmlns=\"http://www.w3.org/1999/xhtml\"><head>";
  echo "\n  <meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />";
  echo "\n  <title>$_page[name] - Pestřá základní škola Bobnice :-))</title>";
  echo "\n  <link rel=\"stylesheet\" type=\"text/css\" href=\"_css/main.css\" />";
  echo "\n  <script src=\"_javascript/main.js\" type=\"text/javascript\"></script>";
  echo "\n</head><body class=\"$color\"><div id=\"main\"><div class=\"page_border\" >&nbsp;</div>";
  echo "\n  <div id=\"logo\">"; LoginTools(); echo "</div>";
  echo "\n  <div class=\"page_border\">&nbsp;</div>";
  echo "\n  <div id=\"corp\"><a href=\"index.php\">Masarykova základní škola a mateřská škola Bobnice, Kovanská 171</a></div>";

  Menu();
  $icon = $_page['icon'] ? "<img src=\"_graphics/icon/32_$_page[icon].png\" alt=\"Ico: $_page[icon]\" />" : "";
  $title = $_page['title'] ? $_page['title'] : $_page['name'];
  echo "\n  <h1>$icon $title</h1><div class=\"page_border\">&nbsp;</div>";
}
//Zobrazení tlačítek pro přihlášení, nebo ohlášení
function LoginTools()
{
  if (IsLoged())
  {
    echo "\n  <div class=\"toolbox\">";
    echo "<div class=\"admin\" >Přihlášený uživatel: $_SESSION[name]</div>";
    ToolButton("p=". PROF_ID ."_administrace" , 'Administrace stránek', 'key', false);
    ToolButton("o=logout&amp;". GetLink(false) , 'Odhlášení', 'close');
    echo "</div>";
  }
  else
  {
    echo "\n  <div class=\"toolbox\">";
    ToolButton("o=login&amp;". GetLink(false) , 'Administrace stránek', 'key');
    echo "</div>";
  }
}
//Funkce provede dotaz na databázi a vypíše tabulky prvního a druhého menu
function Menu()
{
  global $_page;
  //Výběr položek - na jeden dotaz vybírá údaje do obou menu
  $rows = MAIN_MENU_ITEMS; $items = MENU_ITEMS + 1;
  $pid = $_page['pid'] ? $_page['pid'] : $_page['id'];
  $dotaz = IsLoged() ? '' : ' and acces = 0';
  $sql = sql_select('pages','id, name, adr, icon, pid',
    "where deleted = '0'$dotaz and pos > '0' and ( (pid = '0' and pos <= '$rows' ) or (pid = '$pid' and pos <= '$items') ) order by pid, pos"
  );
  
  $count = mysql_num_rows($sql);

  //Tabulka hlavního menu
  if ($data = mysql_fetch_array($sql))
  {
    echo "\n  <table id=\"main_menu\" class=\"menu\"><tr>";
    
    $colors = GetColor();
    for ($x = 1; $x <= MAIN_MENU_ITEMS; $x++)
    {
      if ($data && $data['pid'] == 0)
      {
        $count--;     //aktualizace počtu položek - nutno odečíst položky první tabulky pro výpočet u druhého menu
        $icon = $data['icon'] ? "<img src=\"_graphics/icon/32_$data[icon].png\" alt=\"icon\" />" : '';
        echo "<td class=\"$colors[$x]\">$icon<a href=\"index.php?p=" .$data['id']. "_$data[adr]\">$data[name]</a></td>";
        $data = mysql_fetch_array($sql);
      }
      else echo "<td>&nbsp;</td>";
    }
    
    echo "\n  </tr></table>";
    //<div class=\"chkMenu\" style=\"height: 5px;\">&nbsp;</div>";
  }
  else echo "\n  <div class=\"page_border\">&nbsp;</div>";
  
  //Tabulka druhého menu
  if ($data)
  {
    //Vytiskne zvyrazneni vybrane rurbriky
    if ($_page["color"] <= MAIN_MENU_ITEMS)
    {
      echo "\n  <table class=\"menu chkMenu\"><tr>";
      for ($x = 1; $x <= MAIN_MENU_ITEMS; $x++)
      {
        $c = "";
        if ($_page["color"] == $x) $c = GetColor($_page["color"]);
        echo "<td class=\"$c\" >&nbsp;</td>";
      }
      echo "</tr></table>";
    }
  
    $i = 1; $more = $count > MENU_ITEMS;
    echo "\n  <table id=\"nd_menu\" class=\"menu\" align=\"center\">";
    do
    {
      echo "\n    <tr>";
      for ($x = 1; $x <= MAIN_MENU_ITEMS; $x++)
      {
        if ($data)
        {
          if ($i++ == MENU_ITEMS and $more) //Tlačítko další položky, pro zobrazení stránkování
          {
            echo "<td><a href=\"index.php?p=$data[pid]&amp;a=menu\">Další</a></td>";
            $data = false;
          }
          else  //Položka menu
          {
            $icon = $data['icon'] ? "<img src=\"_graphics/icon/20_$data[icon].png\" alt=\"icon\" />" : '';
            echo "<td>$icon<a href=\"index.php?p=$data[id]_$data[adr]\">$data[name]</a></td>";
            $data = mysql_fetch_array($sql);
          }
        }
        else echo "<td>&nbsp;</td>";
      }
      echo "</tr>";
    }
    while ($data);
    echo "\n  </table>";
    

    //Vytiskne zvyrazneni vybrane rurbriky
    if ($_page["color"] <= MAIN_MENU_ITEMS)
    {
      echo "\n  <table class=\"menu chkMenu\"><tr>";
      for ($x = 1; $x <= MAIN_MENU_ITEMS; $x++)
      {
        $c = "";
        if ($_page["color"] == $x) $c = GetColor($_page["color"]);
        echo "<td class=\"$c\" >&nbsp;</td>";
      }
      echo "</tr></table>";
    }
  }
}

//Spodní část HTML dokumentu
function Bottom()
{
  global $_inf;
  //Ukončení hlavního divu + text 'pod čarou'
  echo "\n\n<div class=\"page_border\">&nbsp;</div></div>\n\n";
  echo "\n\n<div id=\"bottom\"><a href=\"index.php\">Pestrá škola a školka Bobnice</a> | Šíře stránky: 1000px | Pro bezchybný chod mějte aktivován <b>Javascript</b> a <b>CSS</b> | Testováno na:";
  echo " <img src=\"_graphics/icon/20_chrome.png\" alt=\"Google Chrome v. 6.0.472.55\" title=\"Google Chrome v. 6.0.472.55\" />";
  echo " <img src=\"_graphics/icon/20_firefox.png\" alt=\"Mozzila Firefox v. 3.6.9\" title=\"Mozzila Firefox v. 3.6.9\" />";
  echo " <img src=\"_graphics/icon/20_IE8.png\" alt=\"Internet Explorer 8\" title=\"Internet Explorer 8\" />";
  echo " <img src=\"_graphics/icon/20_opera.png\" alt=\"Opera v. 10.62\" title=\"Opera v. 10.62\" />";
  echo "<br />Veškerý obsah stránek Pestrá Škola je majetkem administrátorů a nesmí být bez jejich svolení dále kopírován.";
  echo "<br />Zdrojový kód PHP, CSS, HTML a grafiku webu vytvořil Pavel Baštecký © září 2010 | Případné chyby zobrazení hlaste na: <img src=\"_graphics/anebril.png\" alt=\"anebril &lt;zavináč&gt; seznam.cz\" />";
  Reklama();
  echo "</div>";
  
  //Případné okno pro dodatečné informace (náhled obrázků v galerii)
  if ($_inf['bottom']) PageBottom('wnd1');
  
  //Okno pro zobrazení formulářů
  echo "\n<div class=\"box\" id=\"wnd2\" style=\"width: 620px;\">";
  echo "\n  <div class=\"wnd_tools\" onmousedown=\"return StartDragAndDrop('wnd2', event);\" title=\"Přesun formuláře\" ><a href=\"\" onclick=\"WndClose('wnd2'); return false;\" title=\"Zavřít formulář\" ><img src=\"_graphics/icon/20_close.png\" alt=\"Zavřít formulář\" /></a></div>";
  echo "\n  <div class=\"wnd_box\" >";
  echo "<iframe id=\"box_wnd2\" name=\"ifr\" frameborder=\"0\" width=\"600\" height=\"300\" align=\"middle\" ></iframe>";
  echo "</div>\n<div class=\"float_cl\">&nbsp;</div></div>";
  echo "\n<script type=\"text/javascript\">\n  DragAndDropInit(); FormInit('wnd2');</script>";
  
  echo "\n\n</body></html>";
}

//Zobrazení stránkovacích tlačítek; Vstup:
//Počet zobrazených položek; Celkem položek v databázi; limit na stránku; adresa
//Navrací false pro prázdný seznam, opačně true
function ShowSwitcher($showed, $items, $limit, $adres = "")
{
  global $_page, $_inf;

  if (!$showed)   //Prázdný seznam
  {
    if (!$items) alert("Nic tu není");
    else alert("Tento seznam nemá tolik částí");
    return false;
  }
  
  //Čeština
  if ($showed == 1) $cz = "á položka";
  elseif ($showed >= 2 and $showed <= 4) $cz = "é položky";
  else $cz = "ých položek";
  
  //Vykreslení číselníku
  if ($items > $limit)
  {
    if (!$adres) $adres = GetLink(false);
    
    //Maximální počet zobrazených čísel (zbytek se ořízne)
    $size = SW_SIZE * 2;
    //Číslo poslední stránky
    $max = (int)($items / $limit) + ($items % $limit ? 1 : 0); 
    //Číslo začátku vykreslování
    $s = ($_inf['s'] <= SW_SIZE or $max <= $size) ? 1 : 
              (($_inf['s'] + SW_SIZE > $max) ? $max - $size : $_inf['s'] - SW_SIZE);
    
    
    echo "<div class=\"swap\">"; 
    
    $adr = $_inf['s'] > 1 ? $_inf['s'] - 1 : $max;
    echo "<a href=\"$_inf[page]?$adres&amp;s=1\" title=\"První\" onclick=\"return Ajax('$adres&amp;s=1'); \" ><img src=\"_graphics/icon/20_larrow2_red.png\" alt=\"První\" /></a> ";
    echo "<a href=\"$_inf[page]?$adres&amp;s=$adr\" title=\"Předchozí\" onclick=\"return Ajax('$adres&amp;s=$adr'); \" ><img src=\"_graphics/icon/20_larrow_red.png\" alt=\"Předchozí\" /></a>";

    for ($x = 0; $x < $max and $x < $size + 1; $x++, $s++)
      echo " <a href=\"$_inf[page]?$adres&amp;s=$s\" ". ($_inf['s'] == $s ? " style=\"font-weight:bold\"" : "") ." onclick=\"return Ajax('$adres&amp;s=$s'); \" >$s</a> ";
    
    $adr = $_inf['s'] < $max ? $_inf['s'] + 1 : 1;
    echo "<a href=\"$_inf[page]?$adres&amp;s=$adr\" title=\"Následující\" onclick=\"return Ajax('$adres&amp;s=$adr'); \" ><img src=\"_graphics/icon/20_rarrow_red.png\" alt=\"Následující\" /></a> ";
    echo "<a href=\"$_inf[page]?$adres&amp;s=$max\" title=\"Poslední\" onclick=\"return Ajax('$adres&amp;s=$max'); \" ><img src=\"_graphics/icon/20_rarrow2_red.png\" alt=\"Poslední\" /></a>";

    echo "</div>";
  }
  echo "<div class=\"swap\">$showed zobrazen$cz z celokového počtu $items</div>";
  return true;
}


/*Vypis tabulky polozek menu
    - v zahlavi tabulky jsou tlacitka strankovani
    - kazda polozka je rozdelena na tri radky: tlacitka administrace, nadpis a ikona, datum a ukazka textu
*/
function PrintMenuTable($sql, $count, $limit)
{
  global $_inf; global $_page;
  
  echo "\n<table class=\"items\" align=\"center\"><tr><td colspan=\"4\" class=\"stat\" >";
  if (ShowSwitcher(mysql_num_rows($sql), $count, $limit) )
  {
    echo "</td></tr>";
    
    $move = $_GET['f'] == 'move';
    $admin = (!LCK || IsLoged()) && (!$_page['lck'] || $_page['script'] != 'rubric');
    $load = mysql_fetch_array($sql);
    while ($load)
    {
      //Načtení dat pro řádek tabulky
      for ($x = 1; $x <= TABLE_ITEMS; $x++)
        if ( $tab[$x] = $load )
        {
          if (!$load['script']) $tab[$x]['script'] = "article";
          $load = mysql_fetch_array($sql);
        }

      //Administrační tlačítka
      if ($admin)
      {
        echo "\n  <tr>";
        for ($x = 1; $x <= TABLE_ITEMS; $x++)
        {
          if ($data = $tab[$x])
          { 
            echo "<th><div class=\"toolbox\">";
            if ($move && $data['pos'] != $_inf['i'])
              echo "<input name=\"col[2]\" value=\"$data[pos]\" type=\"submit\" class=\"button\" title=\"Přesunout na toto místo\" />";
            else
            {
              $adr = "$data[id]_$data[adr]";
              ToolButton("p=$_GET[p]&amp;a=menu&amp;f=add&amp;i=$data[pos]" , 'Přidat stránku na toto místo', 'plus');
              ToolButton("p=$_GET[p]&amp;a=menu&amp;f=move&amp;i=$data[pos]&amp;s=$_inf[s]" , 'Přesunout v menu', 'move', true, 'Ajax');
              
              if (!$data['lck']){
                ToolButton("p=$adr&amp;a=$data[script]&amp;f=properties" , 'Upravit stránku', 'buble');
                if ($data['script'] == "article")
                  ToolButton("p=$adr&amp;a=article&amp;f=move" , 'Přesunout článek do jiné rubriky', 'folder');
                
                ToolButton("p=$adr&amp;a=$data[script]&amp;f=delete" , 'Smazat stránku', 'delete');
              }
            }
            echo "</div></th>"; 
          }
          else echo "<td class=\"blank\">&nbsp;</td>";
        }
        echo "</tr>";
      }
      
      //Nadpisy a ikony
      echo "\n  <tr>";
      for ($x = 1; $x <= TABLE_ITEMS; $x++)
      {
        if ($data = $tab[$x])
        {
          echo "<th>";
          if ($data['icon']) echo "<img src=\"_graphics/icon/32_$data[icon].png\" alt=\"icon\" />";
          echo "<a href=\"index.php?p=$data[id]_$data[adr]\">$data[name]</a></th>";
        }
        else echo "<td class=\"blank\">&nbsp;</td>";
      }
      echo "</tr>";
      
      //Datum a náhled textu
      echo "\n  <tr>";
      for ($x = 1; $x <= TABLE_ITEMS; $x++)
      {
        if ($data = $tab[$x])
        {
          $datum = GetDatetime($data['datum']);
          echo "<td><div class=\"date\">$datum</div>";
          if ($data['text']) echo format_text($data['text'],1);
          echo "</td>";
        }
        else echo "<td class=\"blank\">&nbsp;</td>";
      }
      echo "</tr>";
    }
  }
  else echo "</td></tr>";
  
  echo "</table>";
}


?>