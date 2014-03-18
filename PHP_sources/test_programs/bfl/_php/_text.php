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
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
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
      echo "        <h5 class=\"text_alert\">Zatím zde nic není, obsah bude doplněn co nejdříve.</h5>\n";
    }
    return false;
  }
  else ShowTextToPage($sql, true);
}

//Vyvolání zpracování formuláře
function TextPost()
{
  global $_inf;
  
  if (LCK && !IsLoged()) return false;
  if ($_inf['a'] != TEXT_ADRESS) return false;
  
  switch ($_inf['f'])
  {
    case ADD_FORM:     AddText();     break;
    case EDIT_FORM:    EditText();    break;
    case DELETE_FORM:  DeleteText();  break;
    case TRUNC_FORM:   TruncTexts();  break;
    default: return false;
  }
  return true;
}
//Zobrazení formuláře
function ShowTextForm()
{
  global $_inf;
  
  if (LCK && !IsLoged()) return false;
  if ($_inf['a'] != TEXT_ADRESS) return false;
  
  switch ($_inf['f'])
  {
    case ADD_FORM:     AddTextForm();     break;
    case EDIT_FORM:    EditTextForm();    break;
    case DELETE_FORM:  DeleteTextForm();  break;
    case TRUNC_FORM:   TruncTextsForm();  break;
    default: return false;
  }
  return true;
}

//Zobrazí panel nástrojů administrace
function ShowAdminTextsTools($end = " | ")
{
  if (LCK && !IsLoged()) return false;  
  ToolButton("p=$_GET[p]&amp;a=text&amp;f=add", 'Přidat text');
  ToolButton("p=$_GET[p]&amp;a=text&amp;f=trunc", 'Smazat všechny texty', $end);
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

//Přidání textu
function AddText()
{
  global $_inf; global $_page;
  $poz = GetCollum(1);
  if (!is_numeric($poz)) $poz = 0;
  if ( LenghtTest(2, $text = GetTextCollum(2) , 1, 10000)) return;
  
  $data = mysql_fetch_array( sql_select('texts','max(poz) as poz',"where pid = '$_page[id]' and zob = '1'"));
  $p = $data ? $data['poz'] + 1 : 1;
  
  if ($poz > 0 and $poz < $p)
    sql_update('texts',"poz = poz + 1","pid = '$_page[id]' and poz >= '$poz'");
  else $poz = $p;
  
  sql_insert('texts',"text,pid,poz,datum","('$text','$_page[id]','$poz','".date("y-m-d H:i:s")."')");
  MakeRefresh();
}
//Editace existujícího
function EditText()
{
  global $_inf; global $_page;
  $id = GetCollum(1);
  $text = GetTextCollum(2);
  if ( LenghtTest(2, $text, 0, 10000)) return;
  
  if (mb_strlen($text,"utf8") > 0)  //Editace textu
  {
    $data = mysql_fetch_array( sql_select('texts', 'id, poz', "where pid = '$_page[id]' and id = '$id' and zob = '1'", 1) );
    if (!$data){ ThrowFormError(0, "Nelze nalézt vybraný text"); return; }
    
    sql_update('texts',"text = '$text'","pid = '$_page[id]' and id = '$id'");
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
  FormHead("Přidat text", "", true);
  HiddenBox2(1,$_inf['i']);
  //TextBox("Jméno", 1, $_page['name'], 3, 40);
  TextArea("Text stránky", 2, '');
  FormBottom('',"'nul', 'null'","0,0","0,0", false);
}
function EditTextForm()
{
  global $_inf; global $_page;
  
  if (IsRefreshed("Text byl změněn")) return;
  
  $data = mysql_fetch_array( sql_select('texts',"id,poz,text",
    "where pid = '$_page[id]' and zob = 1 and poz = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádný text"); return; }

  FormHead("Upravit text", "", true);
  HiddenBox2(1,$data['id']);
  TextArea("Text stránky", 2, stripslashes($data['text']));
  FormBottom("Neháte-li pole prázdné, dojde k odstranění textu.","'nul', 'null'","0,0","0,0", false);
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