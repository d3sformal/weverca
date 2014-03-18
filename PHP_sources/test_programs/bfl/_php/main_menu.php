<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
*************************************  Zobrazení hlavního menu  ************************************

 * Zaštiťuje zobrazení a administraci hlavního menu - obsahuje články a rubriky
 * Stránky v hlavním menu mají PID nulové a hodnota POS určuje jejich pozici
 * Stránky, které mají POS = 0 a PID = 0 se nezobrazují v žádném menu  
 * Barevné schéma je odvozeno od pozice v hlavním menu 

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

//Vyvolání funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  global $_inf;
  if (LCK && !IsLoged()) return false;

  switch ($_inf['f'])
  {
    case ADD_FORM:     AddMainMenu();    break;
    case MOVE_FORM:    MoveMainMenu();   break;
  }
}

//Zobrazí formulář na stránce
$_inf['form'] = true;
function Form()
{
  global $_inf;
  if (LCK && !IsLoged()) return false;
  
  //Obsah stránky
  switch ($_inf['f'])
  {
    case ADD_FORM:     AddMainMenuForm();    break;
    case MOVE_FORM:    MoveMainMenuForm();
    break;
    default: return false;
  }
  return true;
}

$_inf['admin'] = true;
function PageAdministration()
{    
    ToolButton("p=$_GET[p]", 'Zobrazit hlavní menu');
    ToolButton("p=$_GET[p]&amp;a=menu&amp;f=add", 'Přidat položku do hlavního menu', "");
}

//zapíše informace do hlavičky stránky pro vložení CSS a JS souborů
$_inf['header'] = true;
function PageHeader()
{
  global $_inf; global $_page;
  
  ShowMenuHeader();
  
  if ($_inf['f'] == ADD_FORM)
  {
    ShowCleditorHeader();
  }
}

//Zobrazení stránky
function Body()
{
  global $_page;
  
  if (Form()) return;
  {
    ShowMenu();
  }
}

//Zobrazení obsahu rubriky
function ShowMenu()
{
  global $_page; global $_inf;
  $sql = sql_select('pages',
    'pages.id, pages.name, pages.title, pages.datum, pages.adr, pages.lck, pages.pos, pages.img, texts.text as text',
    "left join texts on texts.id = pages.tid where pages.pid = '0' and pages.pos > '0' and pages.deleted = '0' order by pages.pos, pages.name"
    , (PAGE_LIMIT * ($_inf['s'] - 1) ) .",". PAGE_LIMIT
  );
    
  for($x = 1; $data = mysql_fetch_array($sql); $x++)
  {
    $menu_items[$x]['id'] = $data['id'];
    $menu_items[$x]['adr'] = "$data[id]_$data[adr]";
    $menu_items[$x]['date'] = $data['datum'];
    $menu_items[$x]['title'] = $data['title'] ? $data['title'] : $data['name'];
    $menu_items[$x]['text'] = $data['text'];
    $menu_items[$x]['pos'] = $data['pos'];
    $menu_items[$x]['lck'] = $data['lck'];
    $menu_items[$x]['img'] = $data['img'] ? "page_image/$data[id].jpg" : "_graphics/blank_image.png";
  }
  ShowPagesToPage($menu_items, mysql_num_rows($sql), 0);
  ShowSwitcher(mysql_num_rows($sql), $_page['ct_menu'], PAGE_LIMIT);
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

function AddMainMenu()
{
  global $_page;
  if (!is_numeric( $pos = GetCollum(1) )) $pos = 0;
  if ( LenghtTest(2, $name = GetCollum(2) , 3, 30)) return;
  if ( LenghtTest(3, $title = GetCollum(3) , 0, 100)) return;
  if ( LenghtTest(4, $text = GetTextCollum(4) , 0, 10000)) return;
  
  $tid = 0;
  $adres = MakeAdres($name);
  
  //Přidání anotace
  if ($text)
  {
    $datum = date("y-m-d H:i:s");
    if ($_page['tid'] == 0)
    {
      sql_insert('texts',"text,pid,poz,datum,zob","('$text','$_page[id]','0','$datum','2')");
      $data = mysql_fetch_array(
        sql_select('texts',"id", "where pid = '$_page[id]' and zob = '2' and datum = '$datum'")
      );
      
      if ($data) $tid = $data['id'];
    }
  }

  $p = $_page['ct_menu'] + 1;  
  if ($pos > 0 and $pos < $p)
  {
  echo "<br> pos: $pos";
    sql_update('pages',"pos = pos + 1","pid = '0' and pos >= '$pos' and deleted = '0'");
  }
  else $pos = $p;

  $date = date("y-m-d H:i:s");
  sql_insert('pages','name, adr, datum, pid, mid, level, pos, script, title, tid',
    "('$name', '$adres', '$date', '0', '0', '1', '$pos', 'rubric', '$title', '$tid')"
  );
  sql_update('pages', 'mid = id', "name = '$name' and datum = '$date' and pos = '$pos' and deleted = '0' and mid = '0' and pid = '0'", 1);
  sql_update('pages', 'ct_menu = ct_menu + 1', "id = $_page[id] and deleted = 0", 1);
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
    if($pos > $_page['ct_menu']) $pos = $_page['ct_menu'];
        
    if ($pos > $data['pos'])
    {
      sql_update('pages',"pos = pos - 1","deleted = 0 and pos > '$data[pos]' and pos <= '$pos' and pid = '0'");
    }
    elseif ($pos < $data['pos'])
    {
      sql_update('pages',"pos = pos + 1","deleted = 0 and pos < '$data[pos]' and pos >= '$pos' and pid = '0'");
    }
    sql_update('pages',"pos = $pos","deleted = '0' and id = '$id'", 1);
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
  TextBox("Titulek", 2, '', 3, 30);
  TextBox("Nadpis", 3, '', 0, 100);
  SmallTextArea("Anotace", 4, $_page['text']);
  FormBottom('Titulek slouží jako jméno článku v menu a pokud není vyplněno pole Nadpis, tak i jako nadpis článku.', "'nul', 'txt', 'txt', 'txt'", "0,3,0,0", "0,30,100,10000");
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