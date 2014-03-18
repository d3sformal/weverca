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
require_once "_php/_galery.php";
require_once "_php/_menu.php";
  
//Vyvolá funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  global $_page; global $_inf;
  
  if (LCK && !IsLoged()) return false;
  if (TextPost() || /* GaleryPost() ||*/ MenuPost()) return false;
  if ($_page['lck']) return false;
  
  switch ($_inf['f'])
  {
    //case PROPERTIES_FORM:  PropertiesPost();  break;
    //case DELETE_FORM:  DeleteRubric();  break;
  }  
} 
    
//Zobrazí formulář na stránce
$_inf['form'] = true;
function Form()
{
  global $_page; global $_inf;

  if (LCK && !IsLoged()) return false;
  if (ShowTextForm() || /*ShowGaleryForm() ||*/ ShowMenuForm()) return true;
  //Obsah stránky
  switch ($_inf['f'])
  {
    case PROPERTIES_FORM:  PropertiesForm('rubriky');  break;
    case DELETE_FORM:  DeleteRubricForm();  break;
    default: return false;
  }  
  return true;  
}
    
//zapíše informace do hlavičky stránky pro vložení CSS a JS souborů
$_inf['header'] = true;
function PageHeader()
{
  global $_inf; global $_page;
  
  if (($_inf['a'] == TEXT_ADRESS || $_inf['a'] == MENU_ADRESS) 
        && ($_inf['f'] == ADD_FORM || $_inf['f'] == EDIT_FORM) 
      || $_inf['f'] == PROPERTIES_FORM)
  {
    ShowCleditorHeader();
  }
  else if($_page['ct_images'] > 0 || $_inf['a'] == GALERY_ADRESS)
  {
    ShowGaleryHeader();
    ShowFancyboxHeader();
    
    if ($_page['ct_menu'] > 0 || $_inf['a'] != GALERY_ADRESS)
      ShowMenuHeader();
  }
  else
  {
    ShowMenuHeader();
  }
}   

$_inf['admin'] = true;
function PageAdministration()
{    
  ToolButton("p=$_GET[p]", 'Zobrazit stránku');
  ToolButton("p=$_GET[p]&amp;a=rubric&amp;f=properties", 'Upravit vlastnosti');
  ShowAdminMenuTools();
  ShowAdminTextsTools("");
  echo "<br />";
  ShowAdminGaleryTools();
  ToolButton("p=$_GET[p]&amp;a=rubric&amp;f=delete", 'Smazat rubriku', "");
}
  
//Zobrazení stránky
//Pokud nebude napsán žádný text, tak dojde k vyvolání menu
function Body()
{
  global $_page;

  //if (Form()) return;
  
  ShowTexts($_page['ct_images'] == 0 && $_page['ct_menu'] == 0);
  
  if ($_page['ct_menu'] > 0) ShowMenu();
  if ($_page['ct_images'] > 0)
  {
    ShowGalery();
    GaleryJSInit();
  }
}
  
//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

function DeleteRubric()
{
  global $_page;
  if (GetCollum(1))
  {
    $id = $_page['pid'] == 0 ? MENU_ID : $_page['pid'];
    DeleteInventory(true);
    sql_update('pages',"pos = pos - 1", "pid = 0 and pos > $_page[pos] and deleted = '0'");
    sql_update('pages',"deleted = '1'", "(id = '$_page[id]' or pid = '$_page[id]') and deleted = '0'");
    sql_update('pages', 'ct_menu = ct_menu - 1', "id = '$id' and deleted = 0", 1);
    MakeRefresh("index.php?p=$id" , 2);
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