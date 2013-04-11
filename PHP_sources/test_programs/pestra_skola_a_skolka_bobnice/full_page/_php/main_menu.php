<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
*************************************  Zobrazení hlavního menu  ************************************

 * Zaštiťuje zobrazení a administraci hlavního menu - obsahuje články a rubriky
 * Stránky v hlavním menu mají PID nulové a hodnota POS určuje jejich pozici
 * Stránky, které mají POS = 0 a PID = 0 se nezobrazují v žádném menu  
 * Barevné schéma je odvozeno od pozice v hlavním menu 

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

//Vyvolání funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  if (LCK && !IsLoged()) return false;
  global $_inf; global $_error; global $_page;

  switch ($_GET['f'])
  {
    case 'add':     AddMainMenu();    break;
    case 'move':    MoveMainMenu();   break;
  }
}

//Zobrazí formulář na stránce
$_inf['form'] = true;
function Form()
{
  if (LCK && !IsLoged()) return false;
  //Obsah stránky
  switch ($_GET['f'])
  {
    case 'add':     AddMainMenuForm();    break;
    case 'move':    MoveMainMenuForm();
    break;
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
    ToolButton("p=$_GET[p]", 'Zobrazit hlavní menu', 'page', false);
    ToolButton("p=$_GET[p]&amp;a=menu&amp;f=add", 'Přidat položku do hlavního menu', 'plus');
    echo "</div><div class=\"float_cl\">&nbsp;</div>";
  }
  
  if (Form()) return;
  {
    echo "\n<div id=\"ajax_target\" >";
    ShowMenu();
    echo "\n</div>";
  }
}

//Zpracování AJAX požadavku
$_inf['ajax'] = true;
function Ajax()
{ 
  if (IsLoged() && $_GET['f'] == 'move') MoveMainMenuForm();
  else ShowMenu();
}


//Zobrazení obsahu rubriky
function ShowMenu()
{
  global $_page; global $_inf;
  $sql = sql_select('pages','id, name, datum, adr, lck, pos, script, icon, text, lck',
    "where pid = '0' and pos > '0' and deleted = '0' order by pos, name"
    , (PAGE_LIMIT * ($_inf['s'] - 1) ) .",". PAGE_LIMIT
  );
  
  PrintMenuTable($sql, $_page['ct'], PAGE_LIMIT);
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

function AddMainMenu()
{
  global $_page;
  if (!is_numeric( $pos = GetCollum(1) )) $pos = 0;
  if ( LenghtTest(2, $name = GetCollum(2) , 3, 30)) return;
  if ( LenghtTest(3, $title = GetCollum(3) , 0, 100)) return;
  $script = GetCollum(4) == 1 ? "rubric" : "";
  $icon = GetCollum(5); if ($icon == 'none') $icon = "";
  $adres = MakeAdres($name);

  $p = $_page['ct'] + 1;  
  if ($pos > 0 and $pos < $p)
  {
    sql_update('pages',"pos = pos + 1","pid = '0' and pos >= '$pos' and deleted = '0'");
    sql_update('pages',"color = color + 1","color >= '$pos' and deleted = '0'");
  }
  else $pos = $p;

  sql_insert('pages','name, adr, datum, pid, pos, script, title, icon, color',
    "('$name', '$adres', '". date("y-m-d H:i:s") ."', '0', '$pos', '$script', '$title', '$icon', '$pos')"
  );
  sql_update('pages', 'ct = ct + 1', "id = $_page[id] and deleted = 0", 1);
  MakeRefresh();
}
function MoveMainMenu()
{
  global $_page;
  $id = GetCollum(1);
  if ( NumberTest(2, $pos  = GetCollum(2) , 0, 0)) return;
      
  $data = mysql_fetch_array( sql_select('pages',"id, name, adr, pos ",
    "where deleted = '0' and id = '$id' and pid = '0'",1
  ));
  if (!$data){ ThrowFormError(0, "Nelze nalézt vybranou stránku"); return; }

  if ($pos > 0 and $data['pos'] != $pos and $data['pos'] > 0)
  {
    if($pos > $_page['ct']) $pos = $_page['ct'];
        
    if ($pos > $data['pos'])
    {
      sql_update('pages',"pos = pos - 1","deleted = 0 and pos > '$data[pos]' and pos <= '$pos' and pid = '0'");
      sql_update('pages',"color = color - 1","color > '$data[pos]' and color <= '$pos' and deleted = '0'");
    }
    elseif ($pos < $data['pos'])
    {
      sql_update('pages',"pos = pos + 1","deleted = 0 and pos < '$data[pos]' and pos >= '$pos' and pid = '0'");
      sql_update('pages',"color = color + 1","color < '$data[pos]' and color >= '$pos' and deleted = '0'");
    }
    sql_update('pages',"pos = $pos, color = $pos","deleted = '0' and id = '$id'", 1);
    sql_update('pages',"color = $pos","deleted = '0' and pid = '$id'");
  }

  MakeRefresh("index.php?p=$_GET[p]");
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////

function AddMainMenuForm()
{
  global $_inf;
  if (IsRefreshed("Stránka přidána")) return;
  FormHead("Přidat stránku");
  HiddenBox2(1,$_inf['i']);
  TextBox("Jméno", 2, '', 3, 30);
  TextBox("Titulek", 3, '', 0, 100);
  RadioButtons("Druh stránky", 4, 1, Array(1,2), Array("Rubrika", "Článek"));
  SelectIcon("Ikona", 5);
  FormBottom('', "'nul', 'txt', 'txt'", "0,3,0", "0,30,100");
}
function MoveMainMenuForm()
{
  global $_inf;
  if (IsRefreshed("Stránka byla přesunuta")) return;
  
  if ($_inf['i'] <= 0){ Alert("Na této pozici není žádná stránka"); return; }
  $data = mysql_fetch_array( sql_select('pages',"id, name, adr, pos ",
    "where pid = '0' and deleted = '0' and pos = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádná stránka"); return; }
  
  Alert("Posunout stránku: ". $data['name']);
  echo "\n<form action=\"". GetLink() ."#form\" method=\"post\" id=\"form\" name=\"form\" enctype=\"multipart/form-data\" >";
  echo "<input name=\"col[1]\" id=\"col1\" type=\"hidden\" value=\"$data[id]\" />";
  ShowMenu();
  echo "</form>";
}
?>