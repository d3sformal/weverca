<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
*****************************************  Zobrazení textů  ****************************************

 * Obsahuje funkce pro výběr z databáze a vložení textů do stránky

MYSQL tabulka pro uložení textů:
 * ID, datum, ID stránky - cíl textu, pozice ve stránce (1 nahoře), text, příznak zobrazení (1 = zobrazeno)

CREATE TABLE IF NOT EXISTS `texts` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `datum` datetime NOT NULL,
  `pid` int(10) unsigned NOT NULL,
  `poz` int(10) unsigned NOT NULL,
  `text` text COLLATE utf8_czech_ci NOT NULL,
  `zob` tinyint(4) NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=utf8 COLLATE=utf8_czech_ci AUTO_INCREMENT=1 ;

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

//Zobrazení textů článku
function ShowTexts($alert = true)
{
  global $_page;
  $sql = sql_select('texts','id,poz,text',
    "where pid = '$_page[id]' and zob = '1' order by poz, datum desc"
  );
  
  if (!mysql_num_rows($sql))
  {
    if($alert)
    {
      echo "\n<table class=\"items\" align=\"center\" ><tr><td class=\"stat\">";
      Alert("Nic tu není");
      echo "</td></tr></table>";
    }
    return false;
  }
  else
  {
    echo "<div class=\"text\">";
    while ($data = mysql_fetch_array($sql))
    {
      ShowAdminTextTools($data['poz']);
      echo "\n<div class=\"float_cl\">&nbsp;</div>".format_text($data['text'],1);
    }
    echo "</div>";
    return true;
  }
}

//Vyvolání zpracování formuláře
function TextPost()
{
  if (LCK && !IsLoged()) return false;
  if ($_GET['a'] != 'text') return false;
  switch ($_GET['f'])
  {
    case 'add':     AddText();     break;
    case 'edit':    EditText();    break;
    case 'delete':  DeleteText();  break;
    case 'trunc':   TruncTexts();  break;
    default: return false;
  }
  return true;
}
//Zobrazení formuláře
function ShowTextForm()
{
  if (LCK && !IsLoged()) return false;
  if ($_GET['a'] != 'text') return false;
  switch ($_GET['f'])
  {
    case 'add':     AddTextForm();     break;
    case 'edit':    EditTextForm();    break;
    case 'delete':  DeleteTextForm();  break;
    case 'trunc':   TruncTextsForm();  break;
    default: return false;
  }
  return true;
}

//Zobrazí panel nástrojů administrace
function ShowAdminTextsTools()
{
  if (LCK && !IsLoged()) return false;  
  ToolButton("p=$_GET[p]&amp;a=text&amp;f=add", 'Přidat text', 'pen_add');
  ToolButton("p=$_GET[p]&amp;a=text&amp;f=trunc", 'Smazat všechny texty', 'pen_delete');
}
//Zobrazí panel nástrojů administrace nad každým textem
function ShowAdminTextTools($poz)
{
  if (LCK && !IsLoged()) return false;  
  echo "<div class=\"toolbox\">";
  ToolButton("p=$_GET[p]&amp;a=text&amp;f=add&amp;i=$poz", 'Přidat text', 'pen_add');
  ToolButton("p=$_GET[p]&amp;a=text&amp;f=edit&amp;i=$poz", 'Upravit text', 'pen');
  ToolButton("p=$_GET[p]&amp;a=text&amp;f=delete&amp;i=$poz", 'Smazat text', 'pen_delete');
  echo "</div><div class=\"float_cl\">&nbsp;</div>";
}


//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

//Vytvoří náhledový text pro uložení do databáze - bude zobrazován při výpisu stránek
function MakePrewiew($text)
{
  $text = preg_replace('/^(.{1,200})(.|\n)*$/', '\\1', $text);
  return $text;
}

//Přidání textu
function AddText()
{
  global $_inf; global $_page;
  $poz = GetCollum(1);
  if (!is_numeric($poz)) $poz = 0;
  if ( LenghtTest(2, $text = GetCollum(2) , 1, 10000)) return;
  
  $data = mysql_fetch_array( sql_select('texts','max(poz) as poz',"where pid = '$_page[id]' and zob = '1'"));
  $p = $data ? $data['poz'] + 1 : 1;
  
  if ($poz > 0 and $poz < $p)
    sql_update('texts',"poz = poz + 1","pid = '$_page[id]' and poz >= '$poz'");
  else $poz = $p;
  
  sql_insert('texts',"text,pid,poz,datum","('$text','$_page[id]','$poz','".date("y-m-d H:i:s")."')");
  
  if ($poz == 1)
  {
    $p =  MakePrewiew($text);
    sql_update('pages', "text = '$p'", "id = '$_page[id]'");
  }
  
  MakeRefresh();
}
//Editace existujícího
function EditText()
{
  global $_inf; global $_page;
  $id = GetCollum(1);
  $text = GetCollum(2);
  if ( LenghtTest(2, $text, 0, 10000)) return;
  
  if (mb_strlen($text,"utf8") > 0)  //Editace textu
  {
    $data = mysql_fetch_array( sql_select('texts', 'id, poz', "where pid = '$_page[id]' and id = '$id' and zob = '1'", 1) );
    if (!$data){ ThrowFormError(0, "Nelze nalézt vybraný text"); return; }
    
    sql_update('texts',"text = '$text'","pid = '$_page[id]' and id = '$id'");
    
    if ($data['poz'] == 1)
    {
      $p =  MakePrewiew($text);
      sql_update('pages', "text = '$p'", "id = '$_page[id]'");
    }
  
    MakeRefresh();
  }
  else  //Pole bylo prazdné, text bude smazán
  {
    $_GET['f'] = "delete";
    DeleteText(true);
  }
}
//Smazání existujícího
function DeleteText($d = false)
{
  global $_inf; global $_page;
  $id = GetCollum(1);
  if (GetCollum(2) || $d)
  {
    $data = mysql_fetch_array( sql_select('texts', 'id, poz', "where pid = '$_page[id]' and id = '$id' and zob = '1'", 1) );
    if (!$data){ ThrowFormError(0, "Nelze nalézt vybraný text"); return; }
    sql_delete('texts',"pid = '$_page[id]' and id = '$id'",1);
    sql_update('texts',"poz = poz - 1","pid = '$_page[id]' and poz > '$data[poz]'");
    
    if ($data['poz'] == 1)
    {
      $data = mysql_fetch_array( sql_select('texts', 'id, text',
        "where pid = '$_page[id]' and poz = '1' and zob = '1'", 1) 
      );
      $p =  $data ? MakePrewiew($data['text']) : "";
      sql_update('pages', "text = '$p'", "id = '$_page[id]'");
    }
    
    MakeRefresh();
  }
}
//Smazání všech
function TruncTexts()
{
  global $_inf; global $_page;
  if (GetCollum(1))
  {
    sql_delete('texts',"pid = '$_page[id]'");        
    MakeRefresh();
  }
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////
function AddTextForm()
{
  global $_inf;
  
  if (IsRefreshed("Text byl přidán")) return;
  FormHead("Přidat text");
  HiddenBox2(1,$_inf['i']);
  //TextBox("Jméno", 1, $_page['name'], 3, 40);
  TextArea("Text stránky", 2, '', 1, 10000);
  FormBottom('',"'nul', 'txt'","0,1","0,10000");
}
function EditTextForm()
{
  global $_inf; global $_page;
  
  if (IsRefreshed("Text byl změněn")) return;
  
  $data = mysql_fetch_array( sql_select('texts',"id,poz,text",
    "where pid = '$_page[id]' and zob = 1 and poz = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádný text"); return; }

  FormHead("Upravit text");
  HiddenBox2(1,$data['id']);
  TextArea("Text stránky", 2, $data['text'], 0, 10000);
  FormBottom("Neháte-li pole prádné, dojde k odstranění textu.","'nul', 'txt'","0,0","0,10000");
}
function DeleteTextForm()
{
  global $_inf; global $_page;
  
  if (IsRefreshed("Text odstraněn")) return;
  
  $data = mysql_fetch_array( sql_select('texts',"id,poz,text",
    "where pid = '$_page[id]' and zob = 1 and poz = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádný text"); return; }

  FormHead("Smazat text");
  HiddenBox2(1,$data['id']);
  YNRadio("Opravdu chcete odstranit tento text?", 2, 0);
  FormBottom('');
}
function TruncTextsForm()
{
  if (IsRefreshed("Článek byla vyprázdněn")) return;
  FormHead("Smazat všechny texty");
  YNRadio("Opravdu chcete odstranit veškeré texty z tohoto článku?", 1, 0);
  FormBottom('');
}
?>