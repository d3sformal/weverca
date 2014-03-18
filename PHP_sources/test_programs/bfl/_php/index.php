<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
************************************  Zobrazení rubrik stránek  ************************************

 * Rubriky jsou v databázi uloženy v tabulce PAGES - hodnota script je nastavena na 'rubric'
 * Každá rubrika může mít jednu galerii - rubrika, která již jednu má, má nastavenu hodnotu img na 1
 * Rubrika je uložena v hlavním menu, rubrika nemůže obsahovat jinou rubriku
 * Všechny podřízené stránky mají hodnotu PID nastavenu na ID mateřské rubriky a jejich barevné schéma
 * je odvozeno od její pozice v menu.  

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

require_once "_php/_text.php";

//Vyvolá funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  global $_inf;
  if (LCK && !IsLoged()) return false;
  if (TextPost()) return true;
  
  if ($_inf['f'] == PROPERTIES_FORM) { PropertiesPost(); return true; }
  else return false;
}

//Zobrazí formulář na stránce
$_inf['form'] = true;
function Form()
{
  global $_inf;
  if (LCK && !IsLoged()) return false;
  if (ShowTextForm()) return true;
  
  if ($_inf['f'] == PROPERTIES_FORM) { PropertiesForm('úvodní stránky'); return true; }
  else return false;
}

$_inf['admin'] = true;
function PageAdministration()
{    
    ToolButton("p=$_GET[p]", 'Zobrazit stránku');
    ToolButton("p=$_GET[p]&amp;a=index&amp;f=properties", 'Vlastnosti stránky');
    ShowAdminTextsTools("");
}

//zapíše informace do hlavičky stránky pro vložení CSS a JS souborů
$_inf['header'] = true;
function PageHeader()
{
  global $_inf; global $_page;
  
  if ($_inf['a'] == TEXT_ADRESS && ($_inf['f'] == ADD_FORM || $_inf['f'] == EDIT_FORM) 
      || $_inf['f'] == PROPERTIES_FORM)
  {
    ShowCleditorHeader();
  }
  else
  {
    ShowMenuHeader();
  }
}

//Zobrazení stránky
//Pokud nebude napsán žádný text, tak dojde k vyvolání menu
function Body()
{
  global $_page;

  if (Form()) return;
  
  ShowTexts(false);
  
  global $_page; global $_inf;
  $sql = sql_select('pages',
    'pages.id, pages.name, pages.title, pages.datum, pages.adr, pages.lck, pages.pos, pages.img, texts.text as text',
    "left join texts on texts.id = pages.tid where pages.deleted = '0' and pages.level = 3 order by pages.datum desc"
    , (NEW_PAGES_LIMIT * ($_inf['s'] - 1) ) .",". NEW_PAGES_LIMIT
  );
  
  $count = mysql_num_rows($sql);
  
  if (mysql_num_rows($sql) > 0)
  {
    echo "\n          <h2>Nejnovější články</h2>";
    for($x = 1; $data = mysql_fetch_array($sql); $x++)
    {
      $menu_items[$x]['adr'] = "$data[id]_$data[adr]";
      $menu_items[$x]['date'] = $data['datum'];
      $menu_items[$x]['title'] = $data['title'] ? $data['title'] : $data['name'];
      $menu_items[$x]['text'] = $data['text'];
      $menu_items[$x]['pos'] = $data['pos'];
      $menu_items[$x]['lck'] = $data['lck'];
      $menu_items[$x]['img'] = $data['img'] ? "page_image/$data[id].jpg" : "_graphics/blank_image.png";
    }
    
    ShowPagesToPage($menu_items, mysql_num_rows($sql), 0, false);
    ShowSwitcher($count, $count, NEW_PAGES_LIMIT);
  }
}


//Zpracování AJAX požadavku
$_inf['ajax'] = true;
function Ajax()
{
  if ($_GET['a'] == "menu")
  {
    if (IsLoged() && $_GET['f'] == 'move') MoveArticleForm();
    else ShowMenu();
  }
  else ShowGalery();
}


//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

function DeleteRubric()
{
  global $_page;
  if (GetCollum(1))
  {
    DeleteInventory(true);    
    sql_update('pages',"pos = pos - 1", "pid = 0 and pos > $_page[pos] and deleted = '0'");
    sql_update('pages',"deleted = '1'", "(id = '$_page[id]' or pid = '$_page[id]') and deleted = '0'");
    sql_update('pages', 'ct_menu = ct_menu - 1', "id = ". MENU_ID ." and deleted = 0", 1);
    MakeRefresh("index.php?p=". MENU_ID, 2);
  }
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////


function DeleteRubricForm()
{
  if (IsRefreshed("Rubrika odstraněna")) return;
  FormHead("Odstranit rubriku");
  YNRadio("Opravdu chcete odstranit tuto rubriku a veškeré články v ní?", 1, 0);
  FormBottom('');
}






?>