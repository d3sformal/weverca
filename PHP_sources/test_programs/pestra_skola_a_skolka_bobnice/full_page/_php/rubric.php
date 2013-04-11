<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
************************************  Zobrazení rubrik stránek  ************************************

 * Rubriky jsou v databázi uloženy v tabulce PAGES - hodnota script je nastavena na 'rubric'
 * Každá rubrika může mít jednu galerii - rubrika, která již jednu má, má nastavenu hodnotu img na 1
 * Rubrika je uložena v hlavním menu, rubrika nemůže obsahovat jinou rubriku
 * Všechny podřízené stránky mají hodnotu PID nastavenu na ID mateřské rubriky a jejich barevné schéma
 * je odvozeno od její pozice v menu.  

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
  if ($_page['lck']) return false;
  switch ($_GET['f'])
  {
    case 'add':     AddArticle();    break;
    case 'galery':  RubricGalery();  break;
    case 'move':    MoveArticle();   break;
    case 'properties':  PropertiesPost();  break;
    case 'trunc':   TruncRubric();   break;
    case 'delete':  DeleteRubric();  break;
  }
}

//Zobrazí formulář na stránce
$_inf['form'] = true;
function Form()
{
  if (LCK && !IsLoged()) return false;
  if (ShowTextForm()) return true;
  //Obsah stránky
  switch ($_GET['f'])
  {
    case 'add':     AddArticleForm();    break;
    case 'galery':  RubricGaleryForm();  break;
    case 'move':    MoveArticleForm();   break;
    case 'properties':  PropertiesForm('rubriky');  break;
    case 'trunc':   TruncRubricForm();   break;
    case 'delete':  DeleteRubricForm();  break;
    default: return false;
  }
  return true;
}

//Zobrazení stránky
//Pokud nebude napsán žádný text, tak dojde k vyvolání menu
function Body()
{
  global $_page;
  
  //Administrační tlačítka
  if ((IsLoged() || !LCK) && !$_page['lck'])
  {
    echo "<div class=\"toolbox\">";
    ToolButton("p=$_GET[p]", 'Zobrazit rubriku', 'page', false);
    ToolButton("p=$_GET[p]&amp;a=rubric&amp;f=properties", 'Vlastnosti rubriky', 'buble');
    ToolButton("p=$_GET[p]&amp;a=menu", 'Uspořádat rubriku', 'folder', false);
    ToolButton("p=$_GET[p]&amp;a=rubric&amp;f=add", 'Přidat článek', 'plus');
    ToolButton("p=$_GET[p]&amp;a=rubric&amp;f=galery", 'Fotogalerie rubriky', 'photo');
    ShowAdminTextsTools();
    ToolButton("p=$_GET[p]&amp;a=rubric&amp;f=trunc", 'Smazat všechny články', 'delete_all');
    ToolButton("p=$_GET[p]&amp;a=rubric&amp;f=delete", 'Smazat rubriku', 'delete');
    echo "</div><div class=\"float_cl\">&nbsp;</div>";
  }

  if (Form()) return;
  if ($_GET['a'] == 'menu' || !ShowTexts(false))
  {
    echo "\n<div id=\"ajax_target\" >";
    ShowRubricMenu();
    echo "\n</div>";
  }
}

//Zpracování AJAX požadavku
$_inf['ajax'] = true;
function Ajax()
{
  if (IsLoged() && $_GET['f'] == 'move') MoveArticleForm();
  else ShowRubricMenu();
}

//Zobrazení obsahu rubriky
function ShowRubricMenu()
{
  global $_page; global $_inf;
  $sql = sql_select('pages','id, name, datum, adr, pos, script, icon, text, lck',
    "where pid = '$_page[id]' and deleted = '0' and pos > 0 order by pos, name"
    , (PAGE_LIMIT * ($_inf['s'] - 1) ) .",". PAGE_LIMIT
  );
  
  PrintMenuTable($sql, $_page['ct'], PAGE_LIMIT);
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

//Smaže veškerý obsah (galerie a texty) všech článků rubriky
function DeleteInventory()
{
  global $_page;
  $sql = sql_select('pages','id', "where deleted = '0' and pid = '$_page[id]'");
  $dotaz = "";
  //Sestavení mazacího dotazu
  while ($data = mysql_fetch_array($sql))
  {
    if ($dotaz) $dotaz .= " or ";
    $dotaz .= "pid = $data[id]";
  }
  if ($dotaz)
  {
    //Vymazání obrázků v galeriích
    $sql = sql_select('galery',"id, pid, adr", "where ($dotaz) and zob = 1");
    while ($data = mysql_fetch_array($sql))
    {
      $file = "$data[id]_$data[adr].jpg";
      if (is_file("galery/$file")) unlink("galery/$file");
      if (is_file("galery/thumbs/$file")) unlink("galery/thumbs/$file");
    }
    sql_delete('galery', "($dotaz) and zob = '1'");
    //Vymazání textů stránek
    sql_delete('texts',"$dotaz");
  }
}

//// Vyhodnocení postů

function AddArticle()
{
  global $_page;
  if (!is_numeric( $pos = GetCollum(1) )) $pos = 0;  
  if ( LenghtTest(2, $name = GetCollum(2) , 3, 30)) return;
  if ( LenghtTest(3, $title = GetCollum(3) , 0, 100)) return;
  if (($icon = GetCollum(4)) == 'none') $icon = "";
  
  //Přidání na pozici - posun zbylých, nebo na konec
  $p = $_page['ct'] + 1;
  if ($pos > 0 and $pos < $p)
    sql_update('pages',"pos = pos + 1","pid = '$_page[id]' and pos >= '$pos' and deleted = '0'");
  else $pos = $p;

  sql_insert('pages','name, adr, datum, pid, pos, color, title, icon',
    "('$name', '". MakeAdres($name) ."', '". date("y-m-d H:i:s") ."', '$_page[id]', '$pos', '$_page[color]', '$title', '$icon')"
  );
  sql_update('pages', 'ct = ct + 1', "id = $_page[id] and deleted = 0", 1);
  MakeRefresh("index.php?p=$_GET[p]&amp;a=menu");
}
//Zřízení galerie v rámci rubriky - může být jen jedna
function RubricGalery()
{
  global $_page;
  if ( LenghtTest(1, $name = GetCollum(1) , 3, 30)) return;
  if ( LenghtTest(2, $title = GetCollum(2) , 0, 100)) return;
  if (($icon = GetCollum(3)) == 'none') $icon = "";

  if ($_page['img']) { ThrowFormError(0, "Jednu galerii již máte"); return; }
  
  $pos = $_page['ct'] + 1;
  sql_insert('pages','name, adr, datum, pid, pos, script, title, icon, color',
    "('$name', '". MakeAdres($name) ."', '". date("y-m-d H:i:s") ."', '$_page[id]', '$pos', 'galery', '$title', '$icon', '$_page[color]')"
  );
  sql_update('pages', 'ct = ct + 1, img = 1', "id = $_page[id] and deleted = 0", 1);
  MakeRefresh("index.php?p=$_GET[p]&amp;a=menu");
}
//Posun položky v rámci menu rubriky
function MoveArticle()
{
  global $_page;
  $id = GetCollum(1);
  if ( NumberTest(2, $pos  = GetCollum(2) , 0, 0)) return;
  $data = mysql_fetch_array( sql_select('pages',"id, name, adr, pos ",
    "where deleted = '0' and id = '$id'",1
  ));
  if (!$data){ ThrowFormError(0, "Nelze nalézt vybraný článek"); return; }

  if ($pos > 0 and $data['pos'] != $pos and $data['pos'] > 0)
  {
    //Přerovnání rubriky
    if ($pos > $_page['ct']) $pos = $_page['ct'];
    
    if ($pos > $data['pos'])
      sql_update('pages',"pos = pos - 1","deleted = 0 and pos > '$data[pos]' and pos <= '$pos' and pid = '$_page[id]'");
    elseif ($pos < $data['pos'])
      sql_update('pages',"pos = pos + 1","deleted = 0 and pos < '$data[pos]' and pos >= '$pos' and pid = '$_page[id]'");

    sql_update('pages',"pos = $pos","deleted = '0' and id = '$id'", 1);
  }

  MakeRefresh("index.php?p=$_GET[p]&amp;a=menu");
}
//Vyprázdní rubriku
function TruncRubric()
{
  global $_page;
  if (GetCollum(1))
  {
    DeleteInventory();    
    sql_update('pages',"deleted = '1'", "pid = '$_page[id]' and deleted = '0'");
    sql_update('pages', 'ct = 0, img = 0', "id = $_page[id] and deleted = 0", 1);
    MakeRefresh();
  }
}
function DeleteRubric()
{
  global $_page;
  if (GetCollum(1))
  {
    DeleteInventory();    
    sql_update('pages',"pos = pos - 1", "pid = 0 and pos > $_page[pos] and deleted = '0'");
    sql_update('pages',"color = color - 1", "color > $_page[color] and deleted = '0'");
    sql_update('pages',"deleted = '1'", "(id = '$_page[id]' or pid = '$_page[id]') and deleted = '0'");
    sql_update('pages', 'ct = ct - 1', "id = ". MENU_ID ." and deleted = 0", 1);
    MakeRefresh("index.php?p=". MENU_ID, 2);
  }
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////

function AddArticleForm()
{
  global $_inf;
  if (IsRefreshed("Článek byl přidán")) return;
  FormHead("Přidat článek");
  HiddenBox2(1,$_inf['i']);
  TextBox("Jméno", 2, '', 3, 30);
  TextBox("Titulek", 3, '', 0, 100);
  SelectIcon("Ikona", 4);
  FormBottom('', "'nul', 'txt', 'txt'", "0,3,0", "0,30,100");
}
function RubricGaleryForm()
{
  global $_page; global $_inf;
  if (IsRefreshed("Fotogalerie rubriky byla vytvořena")) return;
  if ($_page['img'])
  {
    $data = mysql_fetch_array( sql_select('pages', 'id, adr',
      "where deleted = '0' and script = 'galery' and pid = '$_page[id]'"
    ));
    if (IsRefreshed("Galerie byla smazána")) return;
    FormHead("Smazat tuto galerii", "$_inf[page]?p=$data[id]_$data[adr]&amp;a=galery&amp;f=delete");
    YNRadio("Opravdu chcete odstranit tuto galerii a sní veškerá její alba?", 1, 0);
    FormBottom('');
  }
  else
  {
    FormHead("Vytvořit fotogalerii v této rubrice");
    TextBox("Jméno fotogalerie", 1, 'Fotogalerie', 3, 30);
    TextBox("Titulek fotogalerie", 2, '', 0, 100);
    SelectIcon("Ikona fotogalerie", 3, 'photo');
    FormBottom('', "'txt', 'txt'", "3,0", "30,100");
  }
}
function MoveArticleForm()
{
  global $_inf; global $_page;
  if (IsRefreshed("Stránka byla přesunuta")) return;

  if ($_inf['i'] <= 0){ Alert("Na této pozici není žádná stránka"); return; }
  
  $data = mysql_fetch_array( sql_select('pages',"id, name, adr, pos ",
    "where pid = '$_page[id]' and deleted = '0' and pos = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádná stránka"); return; }
  
  Alert("Posunout položku: ". $data['name']);
  echo "\n<form action=\"". GetLink() ."#form\" method=\"post\" id=\"form\" name=\"form\" enctype=\"multipart/form-data\" >";
  echo "<input name=\"col[1]\" id=\"col1\" type=\"hidden\" value=\"$data[id]\" />";
  ShowRubricMenu();
  echo "</form>";
  
  /* Původní formulář *
  FormHead("Posunout stránku");
  HiddenBox2(1, $data['id']);
  TextBox("Pozice stránky", 2, $data['pos'], 0,0);
  FormBottom('', "'nul', 'num'", "0,1", "0,999");*/
}
function TruncRubricForm()
{
  if (IsRefreshed("Rubrika byla vyprázdněna")) return;
  FormHead("Smazat všechny články");
  YNRadio("Opravdu chcete odstranit veškeré články?", 1, 0);
  FormBottom('');
}
function DeleteRubricForm()
{
  if (IsRefreshed("Rubrika odstraněna")) return;
  FormHead("Odstranit rubriku");
  YNRadio("Opravdu chcete odstranit tuto rubriku a veškeré články v ní?", 1, 0);
  FormBottom('');
}






?>