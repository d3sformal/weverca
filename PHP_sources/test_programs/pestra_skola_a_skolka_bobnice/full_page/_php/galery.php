<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
****************************************  Zobrazení galerie  ***************************************

 * Galerie jsou v databázi uložena v tabulce PAGES - hodnota script je nastavena na 'galery'
 * Každá rubrika může mít jednu galerii - rubrika, která již jednu má, má nastavenu hodnotu img na 1
 * Galerie neobsahují texty ve stránce ani žádné obrázky, realizují pouze výpis alb

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/ 

//Vyvolání funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  if (LCK && !IsLoged()) return false;
  switch ($_GET['f'])
  {
    case 'add':     AddAlbum();       break;
    case 'properties':  PropertiesPost();   break;
    case 'trunc':   TruncGalery();    break;
    case 'delete':  DeleteGalery();   break;
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
    case 'add':     AddAlbumForm();       break;
    case 'properties':  PropertiesForm('galerie');   break;
    case 'trunc':   TruncGaleryForm();    break;
    case 'delete':  DeleteGaleryForm();   break;
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
    ToolButton("p=$_GET[p]", 'Zobrazit galerii', 'page', false);
    ToolButton("p=$_GET[p]&amp;a=galery&amp;f=properties", 'Vlastnosti galerie', 'buble');
    ToolButton("p=$_GET[p]&amp;a=galery&amp;f=add", 'Přidat album', 'plus');
    ToolButton("p=$_GET[p]&amp;a=galery&amp;f=trunc", 'Smazat všechny alba', 'delete_all');
    ToolButton("p=$_GET[p]&amp;a=galery&amp;f=delete", 'Smazat galerii', 'delete');
    echo "</div><div class=\"float_cl\">&nbsp;</div>";
  }
 
  if (Form()) return;
  echo "\n<div id=\"ajax_target\" >";
  ShowGalery();
  echo "\n</div>";
}

//Zpracování AJAX požadavku
$_inf['ajax'] = true;
function Ajax()
{
  ShowGalery();
}

//Zobrazení obsahu rubriky
function ShowGalery()
{
  global $_page; global $_inf;
  $sql = sql_select('pages','pages.id, pages.name, pages.datum, pages.adr, pages.text, pages.ct, galery.adr as gadr, galery.id as gid',
    "left join galery on galery.id = pages.img where pages.pid = '$_page[pid]' and pages.script = 'album' and pages.deleted = '0' order by pages.datum desc"
    , (ALBUM_LIMIT * ($_inf['s'] - 1) ) .",". ALBUM_LIMIT
  );

  //Výpis alb
  echo "\n<table class=\"items\" align=\"center\"><tr><td colspan=\"4\" class=\"stat\" >";
  if (ShowSwitcher(mysql_num_rows($sql), $_page['ct'], ALBUM_LIMIT) )
  {
    echo "</td></tr>";
    
    $load = mysql_fetch_array($sql);
    while ($load)
    {
      //Načtení dat pro řádek tabulky
      $box = 0;
      for ($x = 1; $x <= TABLE_ITEMS; $x++)
        if ( $tab[$x] = $load ) 
        {
          if ($load['text'] && $load['gid']) $box = 2;
          elseif ($box < 1 && ($load['text'] || $load['gid'])) $box = 1;
          $load = mysql_fetch_array($sql);
        }

      //Administrační tlačítka
      if (!LCK || IsLoged())
      {
        echo "\n  <tr>";
        for ($x = 1; $x <= TABLE_ITEMS; $x++)
        {
          if ($data = $tab[$x])
          { 
            if (!$data['script']) $data['script'] = "article";
            $adr = "$data[id]_$data[adr]";
            
            echo "<th><div class=\"toolbox\">";
            ToolButton("p=$adr&amp;a=album&amp;f=properties", 'Vlastnosi alba', 'buble');
            ToolButton("p=$adr&amp;a=album&amp;f=move", 'Přesunout album do jiné galerie', 'folder');
            ToolButton("p=$adr&amp;a=album&amp;f=delete_all", 'Smazat album', 'delete');
            echo "</div></th>"; 
          }
          else echo "<td class=\"blank\">&nbsp;</td>";
        }
        echo "</tr>";
      }
      
      //Nadpisy
      echo "\n  <tr>";
      for ($x = 1; $x <= TABLE_ITEMS; $x++)
      {
        if ($data = $tab[$x])
        {
          echo "<th>";
          $cz = (!$data['ct'] || $data['ct'] > 4) ? "obrázků" : ( $data['ct'] > 1 ? "obrázky" : "obrázek" ) ;
          echo "<div class=\"date\"><div style=\"float: left\">". GetDatetime($data['datum']) ."</div>$data[ct] $cz</div>";
          echo "<a href=\"index.php?p=$data[id]_$data[adr]\">$data[name]</a>";
          echo "</th>";
        }
        else echo "<td class=\"blank\">&nbsp;</td>";
      }
      echo "</tr>";
      
      //Náhledy obrázků a textů
      for (; $box > 0; $box--)
      {
        echo "\n  <tr>";
        for ($x = 1; $x <= TABLE_ITEMS; $x++)
        {
          if (($data = $tab[$x]))
          {
            if ($data['gid'])
            {
              echo "<td class=\"imgBox\"><a href=\"index.php?p=$data[id]_$data[adr]\"><img src=\"galery/thumbs/$data[gid]_$data[gadr].jpg\" alt=\"galery/thumbs/$data[gid]_$data[gadr].jpg\" /></a></td>";
              $tab[$x]['gid'] = false;
            }
            else if($data['text'])
            {
              echo "<td>". format_text($data['text'],1) ."</td>";
              $tab[$x]['text'] = false;
            }
            else echo "<td class=\"blank\">&nbsp;</td>";
          }
          else echo "<td class=\"blank\">&nbsp;</td>";
        }
        echo "</tr>";
      }
    }
  }
  else echo "</td></tr>";
  
  echo "</table>";
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

//Smaže veškerý obsah (galerie a texty) všech alb v galerii
function DeleteInventory()
{
  global $_page;
  $sql = sql_select('pages','id', "where pid = '$_page[pid]' and script = 'album' and deleted = '0'");
  $dotaz = "";
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

function AddAlbum()
{
  global $_page;
  if ( LenghtTest(1, $name = GetCollum(1) , 3, 40)) return;
  if ( LenghtTest(2, $title = GetCollum(2) , 0, 100)) return;
  sql_insert('pages','name, adr, datum, pid, pos, script, color, title',
    "('$name', '". MakeAdres($name) ."', '". date("y-m-d H:i:s") ."', '$_page[pid]', '0', 'album', '$_page[color]', '$title')"
  );
  sql_update('pages', 'ct = ct + 1', "id = $_page[id] and deleted = 0", 1);
  MakeRefresh();
}
function TruncGalery()
{
  global $_page;
  if (GetCollum(1))
  {
    DeleteInventory();    
    sql_update('pages',"deleted = '1'", "pid = '$_page[pid]' and deleted = '0' and script = 'album'");
    sql_update('pages', 'ct = 0', "id = $_page[id] and deleted = 0", 1);
    MakeRefresh();
  }
}
function DeleteGalery()
{
  global $_page;
  if (GetCollum(1))
  {
    DeleteInventory(); 
    sql_update('pages',"pos = pos - 1", "pid = '$_page[pid]' and pos > $_page[pos] and deleted = '0'");
    sql_update('pages',"deleted = '1'",
      "(pid = '$_page[pid]' and deleted = '0' and script = 'album') or id = '$_page[id]'"
    );  
    sql_update('pages', 'ct = ct - 1, img = 0', "id = $_page[pid] and deleted = 0", 1); 

    MakeRefresh("index.php?p=$_page[pid]&amp;a=menu", 2);
  }
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////

function AddAlbumForm()
{
  if (IsRefreshed("Album bylo přidáno")) return;
  FormHead("Přidat album");
  TextBox("Jméno", 1, '', 3, 40);
  TextBox("Titulek", 2, '', 0, 100);
  FormBottom('', "'txt', 'txt'", "3, 0", "30, 100");
}
function TruncGaleryForm()
{
  if (IsRefreshed("Galerie byla vyprázdněna")) return;
  FormHead("Smazat všechny alba");
  YNRadio("Opravdu chcete odstranit veškerá alba?", 1, 0);
  FormBottom('');
}
function DeleteGaleryForm()
{
  if (IsRefreshed("Galerie byla smazána")) return;
  FormHead("Smazat tuto galerii");
  YNRadio("Opravdu chcete odstranit tuto galerii a sní veškerá její alba?", 1, 0);
  FormBottom('');
}

?>