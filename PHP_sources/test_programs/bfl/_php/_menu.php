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
function MenuPost()
{
  global $_page; global $_inf;
  
  if (LCK && !IsLoged() || $_page['lck']) return false;
  if ($_inf['a'] != MENU_ADRESS) return false;
  
  switch ($_inf['f'])
  {
    case ADD_FORM:     AddArticle();    break;
    case MOVE_FORM:    MoveArticle();   break;
    case TRUNC_FORM:   TruncRubric();   break;
  }
}

//Zobrazí formulář na stránce
function ShowMenuForm()
{
  global $_page; global $_inf;
  
  if (LCK && !IsLoged()) return false;
  if ($_inf['a'] != MENU_ADRESS) return false;
  
  //Obsah stránky
  switch ($_inf['f'])
  {
    case ADD_FORM:     AddArticleForm();    break;
    case MOVE_FORM:    MoveArticleForm();   break;
    case TRUNC_FORM:   TruncRubricForm();   break;
    default: ShowMenu();
  }
  return true;
}

//Zobrazí panel nástrojů administrace
function ShowAdminMenuTools()
{
  global $_page;
  if (LCK && !IsLoged()) return false;  
  ToolButton("p=$_GET[p]&amp;a=menu", 'Uspořádat');
  ToolButton("p=$_GET[p]&amp;a=menu&amp;f=add", 'Přidat článek');
  ToolButton("p=$_GET[p]&amp;a=menu&amp;f=trunc", 'Smazat všechny články');
}

//Zobrazení obsahu rubriky
function ShowMenu()
{
  global $_page; global $_inf;
  $sql = sql_select('pages',
    'pages.id, pages.name, pages.title, pages.datum, pages.adr, pages.lck, pages.img, pages.pos, texts.text as text',
    "left join texts on texts.id = pages.tid where pages.pid = '$_page[id]' and pages.deleted = '0' and pages.pos > 0 order by pages.pos, pages.name"
    , (PAGE_LIMIT * ($_inf['s'] - 1) ) .",". PAGE_LIMIT
  );
  
  for($x = 1; $data = mysql_fetch_array($sql); $x++)
  {
    $menu_items[$x]['id'] = $data['id'];
    $menu_items[$x]['adr'] = "$data[id]_$data[adr]";
    $menu_items[$x]['title'] = $data['title'] ? $data['title'] : $data['name'];
    $menu_items[$x]['text'] = $data['text'];
    $menu_items[$x]['pos'] = $data['pos'];
    $menu_items[$x]['lck'] = $data['lck'];
    $menu_items[$x]['img'] = $data['img'] ? "page_image/$data[id].jpg" : "_graphics/blank_image.png";
  }
  
  ShowPagesToPage($menu_items, mysql_num_rows($sql), 0);
  
  if ($_GET['f'] == 'move') $m = "&amp;f=$_GET[f]&amp;i=$_inf[i]";
  else $m = "";
  ShowSwitcher(mysql_num_rows($sql), $_page['ct_menu'], PAGE_LIMIT, "p=$_GET[p]&amp;a=menu$m");
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

//Smaže veškerý obsah (galerie a texty) všech článků rubriky
function DeleteInventory($killAll = false)
{
  global $_page;
  $sql = sql_select('pages','id', "where deleted = '0' and (pid = '$_page[id]' or mid = '$_page[id]')");
  $dotaz = $killAll ? "pid = '$_page[id]'" : "";

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
  if ( LenghtTest(5, $text = GetTextCollum(5) , 0, 10000)) return;
 
  $tid = 0;
  //Přidání anotace
  if ($text)
  {
    $datum = date("y-m-d H:i:s");
    sql_insert('texts',"text,pid,poz,datum,zob","('$text','$_page[id]','0','$datum','2')");
    $data = mysql_fetch_array(
      sql_select('texts',"id", "where pid = '$_page[id]' and zob = '2' and datum = '$datum'")
    );
    
    if ($data) $tid = $data['id'];
  }
  
  //Přidání na pozici - posun zbylých, nebo na konec
  $p = $_page['ct_menu'] + 1;
  if ($pos > 0 and $pos < $p)
    sql_update('pages',"pos = pos + 1","pid = '$_page[id]' and pos >= '$pos' and deleted = '0'");
  else $pos = $p;

  $level = $_page['level'] + 1;
  $date = date("y-m-d H:i:s");
  $adr = MakeAdres($name);
  sql_insert('pages','name, adr, datum, pid, mid, level, pos, script, title, tid',
    "('$name', '$adr', '$date', '$_page[id]', '$_page[id]', '$level', '$pos', 'rubric', '$title', '$tid')"
  );
  sql_update('pages', 'ct_menu = ct_menu + 1', "id = $_page[id] and deleted = 0", 1);
  
  
  
  //Obrázek stránky
  $fileName = "file4";
  $fid = 4;
  if(is_file($_FILES[$fileName]['tmp_name']))
  {
    if(!is_dir("page_image"))mkdir("page_image");
    
    $vel = getimagesize($_FILES[$fileName]['tmp_name']);
    $wid = $vel[0]; $hei = $vel[1];
    $imageSize = $vel[0] * ($vel[1] / 1000) * 4 + (memory_get_usage() / 1000);
          
    if ($imageSize > 30000)
    {
      ThrowFormError($fid, "Obrázek je příliš veliký, musíte jej zmenšit na velikost nejvíce 2048 x 2048 pixelů");
      return "";
    }
    
    //Získání přípony a jména, pokud nebylo zadáno
    preg_match("/^(.{0,50}).*\.(\S{3})$/", $_FILES[$fileName]['name'], $im);
    $prip = mb_strtolower($im[2], 'utf-8');
      
    //Vytvoření kontextu obrázku k překopírování
    $res = null;
    switch($prip)
    {
      //case "bmp": $res = @imagecreatefromwbmp($_FILES["$fileName"]['tmp_name']);    break;
      case "jpg": $res = @imagecreatefromjpeg($_FILES[$fileName]['tmp_name']);  break;
      case "gif": $res = @imagecreatefromgif( $_FILES[$fileName]['tmp_name']);  break;
      case "png": $res = @imagecreatefrompng( $_FILES[$fileName]['tmp_name']);  break;
      
      default :
        ThrowFormError($fid, "Nebyl zadán soubor s obrázkem - přípona vámi nahraného obrázku: <u>$prip</u>; zadávejte pouze soubory s příponou jpg, gif, png."); 
        return "";
    }
    if ($res == null)
    {
      ThrowFormError($fid, "Interní chyba při zpracovávání obrázku - selhalo načtení obrázku."); 
      return "";
    }

    $data = mysql_fetch_array(
      sql_select('pages', "id", "where datum = '$date' and pid = '$_page[id]' and adr = '$adr' and deleted = 0",1)
    );
    if ($data)
    {
      $f = "page_image/$data[id].jpg";
      //Smazání existujících obrázků
      if (is_file($f)) unlink($f);
  
  
      $vel = getimagesize($_FILES[$fileName]['tmp_name']);
      
      //Vytvoření obrázku pro náhledy
      $wid = $vel[0]; $hei = $vel[1];
      if ($wid > PAGE_IMG_WIDTH) {$hei = (int)(($hei / $wid) * PAGE_IMG_WIDTH) ; $wid = PAGE_IMG_WIDTH ;}
      if ($hei > PAGE_IMG_HEIGHT){$wid = (int)(($wid / $hei) * PAGE_IMG_HEIGHT); $hei = PAGE_IMG_HEIGHT;}
      $im = @imagecreatetruecolor($wid, $hei) or die("Cannot Initialize new GD image stream - new image");
      imagecopyresampled($im, $res, 0,0,0,0, $wid,$hei, $vel[0], $vel[1]);
      imagejpeg($im,$f);
      imagedestroy($im);
      
      sql_update('pages',"img = 1", "id = $data[id] and deleted = 0");
    }
  }

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
    if ($pos > $_page['ct_menu']) $pos = $_page['ct_menu'];
    
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
    sql_update('pages', 'ct_menu = 0', "id = $_page[id] and deleted = 0", 1);
    MakeRefresh();
  }
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////

function AddArticleForm()
{
  global $_inf;
  if (IsRefreshed("Stránka byla přidána")) return;
  FormHead("Přidat stránku");
  HiddenBox2(1,$_inf['i']);
  TextBox("Titulek", 2, '', 3, 30);
  TextBox("Nadpis", 3, '', 0, 100);
  FileBox("Obrázek stránky", 4);
  SmallTextArea("Anotace", 5, '');
  FormBottom('Titulek slouží jako jméno článku v menu a pokud není vyplněno pole Nadpis, tak i jako nadpis článku.<br>Nahrávejte pouze obrázky typu .JPG, .PNG, nebo .GIF, velikost obrázku nesmí přesáhnout 2048x2048px a 4MB dat. Větší obrázky zmenšete v programu pro úpravu fotografií.', "'nul', 'txt', 'txt', 'nul', 'txt'", "0,3,0,0,0", "0,30,100,0,10000");
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
  echo "\n        <form action=\"". GetLink() ."#form\" method=\"post\" id=\"form\" name=\"form\" enctype=\"multipart/form-data\" >";
  echo "<input name=\"col[1]\" id=\"col1\" type=\"hidden\" value=\"$data[id]\" />";
  ShowMenu();
  echo "\n        </form>";
  
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

?>