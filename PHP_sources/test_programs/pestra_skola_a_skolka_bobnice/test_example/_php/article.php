<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
****************************************  Zobrazení Článku  ****************************************

 * Články jsou v databázi uloženy v tabulce PAGES - hodnota script je prázdná
 * Články jsou umístěny v hlavním menu, nebo rubrice a neobsahují žádné další podřízené stránky

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

require_once "_php/_text.php";

//Vyvolá funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  global $_page;
  if (LCK && !IsLoged()) return false;
  if (TextPost()) return;
  
  switch ($_GET['f'])
  {
    case 'properties':  PropertiesPost();  break;
    case 'move':    if ($_page['lck']) return false; MoveArticle();    break;
    case 'delete':  if ($_page['lck']) return false; DeleteArticle();  break;
  }
}

//Zobrazí formulář na stránce
$_inf['form'] = true;
function Form()
{
  global $_page;
  if (LCK && !IsLoged()) return false;
  if (ShowTextForm()) return true;

  //Obsah stránky
  switch ($_GET['f'])
  {
    case 'properties':  PropertiesForm('alba');  break;
    case 'move':    if ($_page['lck']) return false; MoveArticleForm();    break;
    case 'delete':  if ($_page['lck']) return false; DeleteArticleForm();  break;
    default: return false;
  }
  return true;
}

//Zobrazení stránky
function Body()
{
  global $_page;
  
  //Administrační tlačítka
  if (IsLoged() || !LCK)
  {
    echo "<div class=\"toolbox\">";
    ToolButton("p=$_GET[p]", 'Zobrazit článek', 'page', false);
    ToolButton("p=$_GET[p]&amp;a=article&amp;f=properties", 'Vlastnosti článku', 'buble');
    
    if (!$_page['lck'])
      ToolButton("p=$_GET[p]&amp;a=article&amp;f=move", 'Přesunout článek do jiné rubriky', 'folder');

    ShowAdminTextsTools();
    
    if (!$_page['lck'])
      ToolButton("p=$_GET[p]&amp;a=article&amp;f=delete", 'Smazat článek', 'delete');
      
    echo "</div><div class=\"float_cl\">&nbsp;</div>";
  }
  
  if (Form()) return;
  ShowTexts();
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

//Přesun lánku mezi rubrikami
function MoveArticle()
{
  global $_page;
  if ( NumberTest(1, $pid  = GetCollum(1) , 0, 0)) return;
  if ($pid == $_page['pid']) { ThrowFormError(0, "Pro přesun v rámci jedné rubriky použijte <a href=\"$_inf[page]?p=$pid&amp;a=menu&amp;f=move&amp;i=$_page[pos]\" >tento formulář</a>"); return; }
  
  $color = 0; $p = 0;
  if ($pid) //Přesun do jiné rubriky
  {
    $data = mysql_fetch_array( sql_select('pages', 'id, color, ct', "where deleted = '0' and id = '$pid' and script = 'rubric'"));
    if (!$data){ ThrowFormError(0, "Cílová rubrika neexistuje"); return; }
    $p = $data['ct'] + 1;
    $color = $data['color'];
  }
  else      //Přesun do hlavní nabídky
  {
    $data = mysql_fetch_array( sql_select('pages', 'id, ct', "where deleted = '0' and id = '". MENU_ID ."'"));
    if (!$data){ ThrowFormError(0, "Cílová rubrika neexistuje"); return; }
    $p = $data['ct'] + 1;
    $color = $p;
  }
  //Posun barev - pokud je přesouváno z hlavní nabídky
  if (!$_page['pid'])
  {
    sql_update('pages',"color = color - 1", "color > $_page[color] and deleted = '0'");
    if ($color > $_page['color']) $color--;
  }
  
  //Aktualizace čítače položek rubrik
  $id = $_page['pid'] ? $_page['pid'] : MENU_ID;
  sql_update('pages', 'ct = ct - 1', "id = '$id' and deleted = 0", 1);
  $id = $pid ? $pid : MENU_ID;
  sql_update('pages', 'ct = ct + 1', "id = '$id' and deleted = 0", 1);
  //Posun zbylých položek
  sql_update('pages',"pos = pos - 1", "pid = '$_page[pid]' and pos > $_page[pos] and deleted = '0'");
  
  //Zařezení na konec cílové rubriky
  sql_update('pages',"pid = '$pid', pos = '$p', color = '$color'", "id = '$_page[id]'");

  MakeRefresh();
}

function DeleteArticle()
{
  global $_page;
  if (GetCollum(1))
  { 
    if (!$_page['pid'])
      sql_update('pages',"color = color - 1", "color > $_page[color] and deleted = '0'");
    
    sql_update('pages',"pos = pos - 1", "pid = '$_page[pid]' and pos > $_page[pos] and deleted = '0'");
    sql_delete('texts',"pid = '$_page[id]'");
    sql_update('pages',"deleted = '1'", "id = '$_page[id]'",1);
    
    $pid = $_page['pid'] ? $_page['pid'] : MENU_ID;
    sql_update('pages', 'ct = ct - 1', "id = '$pid' and deleted = 0", 1);

    MakeRefresh("index.php?p=". ($_page['pid'] ? $_page['pid'] : MENU_ID) ."&amp;a=menu", 2);
  }
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////

function MoveArticleForm()
{
  global $_page;
  if (IsRefreshed("Článek byl přesunut")) return;
  FormHead("Přesunout článek do jiné rubriky");
  
  
  $sql = sql_select('pages', 'name, id',
      "where pid = '0' and deleted = '0' and script = 'rubric' order by name"
  );
  
  $values[0] = 0; $texts[0] = "hlavní nabídka";
  for ($x = 1; $data = mysql_fetch_array($sql) ; $x++)
  {
    $values[$x] = $data['id']; $texts[$x] = $data['name'];
  }

  SelectBox("Přesunout do rubriky", 1, $_page['pid'], $values, $texts);
  FormBottom('');
}
function DeleteArticleForm()
{
  if (IsRefreshed("Článek odstraněn")) return;
  FormHead("Odstranit článek");
  YNRadio("Opravdu chcete odstranit tento článek?", 1, 0);
  FormBottom('');
}


?>