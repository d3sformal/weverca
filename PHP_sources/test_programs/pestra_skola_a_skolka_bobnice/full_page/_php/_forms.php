<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
***************************  Funkce pro zobrazení a zpracování formulářů  **************************

 * Scrit obsahuje funkce pro načtení řádky formuláře, test jejich hodnot a výpis chybových hlášení
 * V další části jsou funkční celky pro vykreslení samotného formuláře 

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

/********** Funkce pro kontrolu formulářů *********************************************************/

//Zaznamená chybové hlášení při zpracovávání formulářů
function ThrowFormError($id, $errormessage)
{
  global $_inf; global $_error;
  $_inf['error'] = true;
  $_error[$id] = $errormessage;
}

//Navráti upravený obsah položky podle ID
function GetCollum($id)
{
  if (!isset($_POST['col'][$id])) return "";

  $col = preg_replace("/^([ \n\r\t\f])+|([ \n\r\t\f])+$/s", "", $_POST['col'][$id]);
  return addslashes( htmlspecialchars($col) );
}

//Test přítomnosti mezer
function SpaceTest($id, $text)
{
  global $_inf;
  if (preg_match("/[ \n\r\t\f]/s",$text))
    ThrowFormError($id, "Toto pole nesmí obsahovat mezery!");

  return $_inf['error'];
}

//Čeština pro dialogy
function Czech1($num)
{
  return $num > 4 ? "ů" : ($num > 1 ? "y" : "");
}

//Test délky vstupu
function LenghtTest($id, $text, $min, $max, $duly = true)
{
  global $_inf;
  $lenght = mb_strlen($text,"utf8");       
 
  if(!$lenght and $min)
  {
    if ($duly) ThrowFormError($id, "Toto pole je povinné.");
  }
  elseif($lenght < $min)
    ThrowFormError($id, "Toto pole musí obsahovat nejméně $min znak". Czech1($min) .", zadal(a) jste $lenght znak". Czech1($lenght) ."!");
  elseif($lenght > $max)
    ThrowFormError($id, "Toto pole musí obsahovat nejvíce $max znak". Czech1($max) .", zadal(a) jste $lenght znak". Czech1($lenght) ."!");

  return $_inf['error'];
}

//Test číselnosti
function NumberTest($id, $number, $min, $max)
{
  global $_inf;
  if (!is_numeric($number))
    ThrowFormError($id, "Do tohoto pole musí být zadáno číslo");
    
  elseif(($min != 0 or $max != 0) and ($number < $min or $number > $max))
    ThrowFormError($id, "Do tohoto pole smí být zadáno číslo od $min do $max!");
  
  return $_inf['error'];
}

function MailTest($id, $mail, $duly)
{
  global $_inf;
  $lenght = mb_strlen($text,"utf8");
  if (!$lenght)
  {
    if ($duly) ThrowFormError($id, "Toto pole je povinné.");
  }
  elseif(!ereg(".+@.+\..+",$col[$x]) or ereg(" ",$col[$x]))
     ThrowFormError($id, "Zadejte platnou E-Mailovou adresu.");

  return $_inf['error'];
}

/********** Časté formuláře ***********************************************************************/
function PropertiesForm($ndp)
{
  global $_page;
  if (IsRefreshed("Stránka byla upravena")) return;
  FormHead("Vlastnosti $ndp");
  TextBox("Jméno", 1, $_page['name'], 3, 30);
  TextBox("Titulek", 2, $_page['title'], 0, 100);
  SelectIcon("Ikona", 3, $_page['icon']);
  FormBottom('', "'txt', 'txt'", "3, 0", "30, 100");
}
function PropertiesPost()
{
  global $_page;
  if ( LenghtTest(1, $name = GetCollum(1) , 3, 30)) return;
  if ( LenghtTest(2, $title = GetCollum(2) , 0, 100)) return;
  if (($icon = GetCollum(3)) == 'none') $icon = "";
  $adr = MakeAdres($name);
  $dotaz = '';
  
  if ($name != $_page['name'])
  {
    $dotaz = "name = '$name', adr = '$adr'";
  }
  if ($title != $_page['title'])
  {
    if ($dotaz) $dotaz .= ", ";
    $dotaz .= "title = '$title'";
  }
  if ($icon != $_page['icon'])
  {
    if ($dotaz) $dotaz .= ", ";
    $dotaz .= "icon = '$icon'";
  }
  
  if (!$dotaz) { ThrowFormError(0, "Neprovedl jste žádnou změnu"); return; }
  
  sql_update('pages',"name = '$name', adr = '$adr', title = '$title', icon = '$icon'", "id = '$_page[id]'");
  MakeRefresh("index.php?p=$_page[id]_$adr");
}

/********** Funkce pro vykreslení formulářů *******************************************************/

//Hlavička standartního formuláře
//Vstup: aresa zpracování, ID formuláře, nadpis formuláře
function FormHead($name, $adr = "")
{
  global $_error; global $_inf;
  //Hlavička
  if (!$adr) $adr = GetLink();
  
  echo "\n<form action=\"$adr\" method=\"post\" id=\"form\" name=\"form$fid\" enctype=\"multipart/form-data\" onsubmit=\"return FormCheck();\" >";
  HiddenBox("time", date('t')); 
  echo "\n  <table align=\"center\" class=\"formTable\" >";
  
  $eBox = $_inf['error'] ? "" : "style=\"display:none\" ";
  $eMsg = $_error[0] ? "<br><p>$_error[0]</p>" : "";
  
  echo "\n    <tr id=\"e0\" class=\"emsg\" $eBox><th colspan=\"3\" id=\"form_error\" >Při zpracovávání folmuláře došlo k chybám$eMsg</th></tr><tr><th colspan=\"2\" class=\"formTitle\" >$name</th><td class=\"rightCol\" ><input type=\"submit\" value=\"Odeslat\" class=\"button\" /></td></tr>";
}

//Zobrazí konec formuláře s dodatečným textem
function FormBottom($text, $scripts = "", $minims = "", $maxims = "")
{
  if ($scripts) $scripts = ', '.$scripts;
  if ($minims) $minims = ', '.$minims;
  if ($maxims) $maxims = ', '.$maxims;
  
  if ($text) echo "\n    <tr><td colspan=\"3\"><p>$text</p></td></tr>";
  echo "\n  </table>";
  echo "\n  <script type=\"text/javascript\"> scripts = new Array('nul'$scripts); minims = new Array(0$minims); maxims = new Array(0$maxims)</script>";
  echo "\n</form>";
}

//Hlavička normalizované položky
//Vstup: nadpis položky, ID, maximální a minimální počet znaků
function InputHead($name, $id, $min = -1, $max = -1, $length = -1, $duly = true)
{  
  global $_error;

  //Chybové hlášení
  $e = $_error[$id] ? "" : " style=\"display:none\"";
  $c = $_error[$id] || ($duly && $min > 0 && $length == 0) ? " class=\"error\"" : "";

  echo "\n    <tr id=\"e$id\" class=\"emsg\"$e ><th colspan=\"3\" id=\"err_$id\" >$_error[$id]</th></tr>";
  echo "<tr id=\"c$id\" $c><th class=\"inputTitle\" >$name</th>";  
}
//Zapatí normalizované položky
function InputBottom($name, $id, $min = -1, $max = -1, $length = -1)
{
  echo "<td class=\"rightCol\">";
  if ($min != -1 && $max != -1)
  {
    echo "<span id=\"num$id\" class=\"help\" title=\"Napsané znaky\">". ($length >= 0 ? $length : "0") ."</span>/";
    echo "<span class=\"help\" title=\"Minimální počet znaků\">$min</span>/";
    echo "<span class=\"help\" title=\"Maximální počet znaků\">$max</span>";
  }
  else echo "&nbsp;";
  echo "</td></tr>";
}

//Skrytý prvek formuláře
function HiddenBox($name, $value)
{
  echo "<input name=\"$name\" id=\"$name\" type=\"hidden\" value=\"$value\" />";
}
function HiddenBox2($id, $value)
{
  echo "<tr><td colspan=\"3\"><input name=\"col[$id]\" id=\"col$id\" type=\"hidden\" value=\"$value\" /></td></tr>";
}

//Jednoduché textové pole
function TextBox($name, $id, $value, $min = -1, $max = -1, $duly = true)
{
  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];
  $length = mb_strlen($value,"utf8");
  
  InputHead($name, $id, $min, $max, $length, $duly);
  echo "<td class=\"inputTd\"><input name=\"col[$id]\" id=\"col$id\" type=\"text\" value=\"$value\" class=\"input\" onkeyup=\"Typing($id)\" onblur=\"Typing($id)\" /></td>";
  InputBottom($name, $id, $min, $max, $length);
}

//Pole pro heslo
function PasswordBox($name, $id, $value, $min = -1, $max = -1, $duly = true)
{
  InputHead($name, $id, $min, $max, 0, $duly);
  echo "<td class=\"inputTd\"><input name=\"col[$id]\" id=\"col$id\" type=\"password\" class=\"input\" value=\"$value\" onkeyup=\"Typing($id)\" onblur=\"Typing($id)\" /></td>";
  InputBottom($name, $id, $min, $max, 0 );
}

//Pole pro soubor
function FileBox($name, $id)
{
  InputHead($name, $id);
  echo "<td class=\"inputTd\"><input name=\"file$id\" id=\"col$id\" type=\"file\" /></td>";
  InputBottom($name, $id);
}

//Zobrazení přepínačů
//Vstup: jméno, ID, výchozí hodnota, pole hodnot, pole popisků
function RadioButtons($name, $id, $value, $values, $texts)
{
  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];
  
  InputHead($name, $id);
  $z = 0;
  echo "<td class=\"inputTd\">";
  foreach ($values as $val)
  {
    $col = $id."_$z";
    echo "\n<span class=\"radio_text\" onclick=\"document.getElementById('col$col').checked = true;\">$texts[$z]</span><input name=\"col[$id]\" id=\"col$col\" type=\"radio\" value=\"$val\" class=\"imp\"". ($val == $value ? ' checked="checked"' : '') ." />";
    $z++;
  }
  echo "</td>";
  InputBottom($name, $id);
}

//Předpřipravený YES/NO přepínač
function YNRadio($name, $id, $value)
{
  RadioButtons($name, $id, $value, Array(1, 0) , Array("Ano", "Ne") );
}

//Zobrazení zaškrtávacích položek
function CheckboxButtons($name, $id, $value, $values, $texts)
{
  InputHead($name, $id);
  echo "<td class=\"inputTd\">";
  foreach ($values as $val)
  {
    $col = $id."_$z";
    echo "\n<span class=\"radio_text\" onclick=\"document.getElementById('col$col').checked = !document.getElementById('col$col').checked;\">$texts[$z]</span><input name=\"chbox".$x."[$id]\" id=\"col$col\" type=\"checkbox\" value=\"$val\" class=\"imp\" />";
    $z++;
  }
  echo "</td>";
  InputBottom($name, $id);
}

//Combobox
function SelectBox($name, $id, $value, $values, $texts)
{
  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];
  
  InputHead($name, $id);
  echo "<td class=\"inputTd\"><select id=\"col$id\" name=\"col[$id]\" class=\"input\" >";
  $z = 0;
  foreach ($values as $val)
  {
    echo "<option value=\"$val\"" .($value == $val ? " selected=\"selected\"" : ""). ">$texts[$z]</option>";
    $z++;
  }
  echo "</select></td>";
  InputBottom($name, $id);
}

//Ikony
function SelectIcon($name, $id, $value = 0)
{
  $icons = Array('none', 'car',  'plane',  'train',  'clock',  'sun',  'sighn', 'price', 'phone', 'photo', 'table', 'brick_house',
  'fork', 'key', 'key2', 'lupa',  'pen', 'person', 'couple', 'statistic',
  'plus', 'delete', 'close', 'file', 'folder', 'move', 'page', 'page2',  'pergamen');
  
  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];
  elseif(!$value) $value = 'none';
  
  InputHead($name, $id);
  echo "<td class=\"inputTd\">&nbsp;</td>";
  InputBottom($name, $id);
  
  echo "<tr><td colspan=\"3\" class=\"big_col\" ><table class=\"icoTable\" align=\"center\" >";
  $col = $id."_0";
  $c = 13;
  echo "\n    <tr><td colspan=\"$c\" ><span class=\"radio_text\" onclick=\"document.getElementById('col$col').checked = true;\">žádná</span>";
  echo "<input name=\"col[$id]\" id=\"col$col\" type=\"radio\" value=\"none\" class=\"imp\"". ('none' == $value ? ' checked="checked"' : '') ." /></td></tr>";
       
  $z = 1;
  $ico = $icons[$z];
  while ($ico)
  {
    echo "\n    <tr>";
    for ($x = 1; $x <= $c; $x++)
    {
      if ($ico)
      {    
        $col = $id."_$z";
        echo "<td><input name=\"col[$id]\" id=\"col$col\" type=\"radio\" value=\"$ico\" class=\"imp\"". ($ico == $value ? ' checked="checked"' : '') ." />";
        echo " <span class=\"radio_text\" onclick=\"document.getElementById('col$col').checked = true;\"><img src=\"_graphics/icon/32_$ico.png\" alt=\"$ico\" /></span></td>";
        $ico = $icons[++$z];
      }
      else echo "<td>&nbsp;</td>";
    }
    echo "</tr>";
  }
  
  echo "</table></td></tr>";
}

//Víceřádková TEXTAREA s plnou nabídkou
function TextArea($name, $id, $value, $min, $max)
{
  global $_page;
  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];

  $barvy = array(array('red','červená'),array('purple','fialová'), array('brown','hnědá'), array('navy','námořnická'), array('blue','modrá'), array('orange','oranžová'), array('gray','šedá'),array('green','zelená'),array('yellow','žlutá'));
  $pisma = array('Arial','Comic Sans MS','Courier New','Impact','Garamond','Georgia','MS Sans Serif','Times New Roman','Verdana');
  $pid = $_page['pid'] != 0 ? $_page['pid'] : $_page['id'];

  $length = mb_strlen($value,"utf8");
  
  InputHead($name, $id, $min, $max, $length);
  
  echo "<td class=\"inputTd\" >&nbsp;</td>";
  InputBottom($name, $id, $min, $max, $length);
  
  echo "\n    <tr><td colspan=\"3\" class=\"big_col\" >";
  
  
  
  
  echo "<div class=\"textTools\"><br>";
  echo "<select  style=\"width:100px; height:20px; float:none; margin:0px 2px 5px 2px;\" onchange=\"val=this.options[this.selectedIndex].value; insertAtCursor('col$id','[BARVA='+ val + ']','[/BARVA]'); this.selectedIndex = 0;\" id=\"color\"><option value=\"---\">-barva-</option>"; foreach($barvy as $_barvy)echo "<option value=\"$_barvy[0]\" style=\"color:$_barvy[0]\">$_barvy[1]</option>"; echo "</select>";
  echo "<select  style=\"width:120px; height:20px; float:none; margin:0px 2px 5px 2px;\" onchange=\"val=this.options[this.selectedIndex].value; insertAtCursor('col$id','[PISMO='+ val + ']','[/PISMO]'); this.selectedIndex = 0;\" id=\"pismo\"><option value=\"---\">-písmo-</option>"; foreach($pisma as $_pisma)echo "<option value=\"$_pisma\" style=\"font-family:'$_pisma'\">$_pisma</option>"; echo "</select>";
  echo "<select  style=\"width:100px; height:20px; float:none; margin:0px 2px 5px 2px;\" onchange=\"val=this.options[this.selectedIndex].value; insertAtCursor('col$id','[VELIKOST='+ val + ']','[/VELIKOST]'); this.selectedIndex = 0;\" id=\"velikost\"><option value=\"---\">-velikost-</option>"; for($y = 10; $y <= 40; $y += 2)echo "<option value=\"$y\" style=\"font-size:$y\">$y bodů</option>"; echo "</select>";
  echo "<br>";
  echo "<a href=\"javascript:insertAtCursor('col$id','[B]','[/B]',0);\" title=\"Tučné písmo  [B][/B]\"><img src=\"_grafika/tlacitka/20_tl_b.png\" width=\"20px\" height=\"20px\" alt=\"Tučné\"></a>";
  echo "<a href=\"javascript:insertAtCursor('col$id','[I]','[/I]',0);\" title=\"Kurzíva  [I][/I]\"><img src=\"_grafika/tlacitka/20_tl_i.png\" width=\"20px\" height=\"20px\" alt=\"Kurzíva\"></a>";
  echo "<a href=\"javascript:insertAtCursor('col$id','[U]','[/U]',0);\" title=\"Podtržené  [U][/U]\"><img src=\"_grafika/tlacitka/20_tl_u.png\" width=\"20px\" height=\"20px\" alt=\"Podtržené\"></a>";
  echo "<span style=\"margin:0px 2px;font-size:1px;\">&nbsp;</span>";
  echo "<a href=\"javascript:insertAtCursor('col$id','[VLEVO]','[/VLEVO]',1);\" title=\"Zarovnání vlevo  [VLEVO][/VLEVO]\"><img src=\"_grafika/tlacitka/20_tl_vlevo.png\" width=\"20px\" height=\"20px\" alt=\"Vlevo\"></a>";
  echo "<a href=\"javascript:insertAtCursor('col$id','[NASTRED]','[/NASTRED]',1);\" title=\"Zarovnání nastřed  [NASTRED][/NASTRED]\"><img src=\"_grafika/tlacitka/20_tl_nastred.png\" width=\"20px\" height=\"20px\" alt=\"Nastřed\"></a>";
  echo "<a href=\"javascript:insertAtCursor('col$id','[VPRAVO]','[/VPRAVO]',1);\" title=\"Zarovnání vpravo  [VPRAVO][/VPRAVO]\"><img src=\"_grafika/tlacitka/20_tl_vpravo.png\" width=\"20px\" height=\"20px\" alt=\"Vpravo\"></a>";
  echo "<span style=\"margin:0px 2px;font-size:1px;\">&nbsp;</span>";
  echo "<a href=\"javascript:InsertList('col$id');\" title=\"Seznam  [SEZNAM][/SEZNAM]\"><img src=\"_grafika/tlacitka/20_tl_seznam.png\" width=\"20px\" height=\"20px\" alt=\"Seznam\"></a>";
  echo "<a href=\"javascript:InsertNumList('col$id');\" title=\"Číslovaný seznam  [CISLOVANY][/CISLOVANY]\"><img src=\"_grafika/tlacitka/20_tl_num_seznam.png\" width=\"20px\" height=\"20px\" alt=\"Číslovaný seznam\"></a>";
  echo "<a href=\"javascript:insertAtCursor('col$id','[POLOZKA]','[/POLOZKA]',1);\" title=\"Položka seznamu  [POLOZKA][/POLOZKA]\"><img src=\"_grafika/tlacitka/20_tl_li.png\" width=\"20px\" height=\"20px\" alt=\"Položka seznamu\"></a>";
  echo "<span style=\"margin:0px 2px;font-size:1px;\">&nbsp;</span>";
  echo "<a href=\"javascript:InsertTable('col$id');\" title=\"Tabulka  [TABULKA][/TABULKA]\"><img src=\"_grafika/tlacitka/20_tl_tab.png\" width=\"20px\" height=\"20px\" alt=\"Tabulka\"></a>";
  echo "<a href=\"javascript:InsertRow('col$id');\" title=\"Řádek tabulky  [RADEK][/RADEK]\"><img src=\"_grafika/tlacitka/20_tl_tabtr.png\" width=\"20px\" height=\"20px\" alt=\"Řádek tabulky\"></a>";
  echo "<a href=\"javascript:insertAtCursor('col$id','[BUNKA]','[/BUNKA]',1);\" title=\"Buňka tablky  [BUNKA][/BUNKA]\"><img src=\"_grafika/tlacitka/20_tl_tabtd.png\" width=\"20px\" height=\"20px\" alt=\"Buňka tablky\"></a>";
  echo "<span style=\"margin:0px 2px;font-size:1px;\">&nbsp;</span>";
  echo "<a href=\"javascript:InsertImage('col$id',$pid,0,1);\" title=\"Obrázek  [IMG][/IMG]\"><img src=\"_grafika/tlacitka/20_tl_img.png\" width=\"20px\" height=\"20px\" alt=\"Obrázek\"></a>";
  echo "<a href=\"javascript:InsertLink('col$id');\" title=\"Odkaz  [ODKAZ=][/ODKAZ]\"><img src=\"_grafika/tlacitka/20_tl_url.png\" width=\"20px\" height=\"20px\" alt=\"Odkaz\"></a>";
  echo "<span style=\"margin:0px 2px;font-size:1px;\">&nbsp;</span>";
  echo "<a href=\"javascript:insertAtCursor('col$id','[NADPIS]','[/NADPIS]',1);\" title=\"Nadpis  [NADPIS][/NADPIS]\"><img src=\"_grafika/tlacitka/20_tl_ndp.png\" width=\"20px\" height=\"20px\" alt=\"Nadpis\"></a>";
  echo "\n<br>";
  for($z = 1;$y = get_smajl($z,2,$id); $z++){echo $y;}

  
  echo "</div><div id=\"insert_image\"></div>";   
  
  echo "<textarea id=\"col$id\" name=\"col[$id]\" class=\"input\" rows=\"15\" cols=\"68\"  onkeyup=\"Typing($id)\" onblur=\"Typing($id)\" >$value</textarea>";
  echo "</td></tr>";  
}




?>
