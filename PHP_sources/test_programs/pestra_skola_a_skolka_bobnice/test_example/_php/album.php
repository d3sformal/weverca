<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");


/***************************************************************************************************
**************************************  Zobrazení alb galerie  *************************************

 * Album je v databázi uloženo v tabulce PAGES - hodnota script je nastavena na 'album'
 * Hodnota PID odkazuje na mateřskou rubriku, hodnota POS je nulová - album není zobrazeno v menu rubriky
 * Hodnota img obsahuje ID náhledového obrázku, alba jsou řazena podle data přidání od nejmladšího  

MYSQL tabulka pro uložení obrázků:
 * ID, datum, ID alba, adresa obrázku (část před koncovkou), rozměry, příznak zobrazení (1 = zobrazeno) 

CREATE TABLE IF NOT EXISTS `galery` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `datum` datetime NOT NULL,
  `pid` int(10) unsigned NOT NULL,
  `adr` varchar(50) CHARACTER SET utf8 COLLATE utf8_czech_ci NOT NULL,
  `width` smallint(5) unsigned NOT NULL,
  `height` smallint(5) unsigned NOT NULL,
  `zob` tinyint(3) unsigned NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=latin2 COLLATE=latin2_czech_cs AUTO_INCREMENT=1 ;

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

require_once "_php/_text.php";

//Dotaz na výběr obrázků
$image_sql = false;


//Vyvolá funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  if (LCK && !IsLoged()) return false;
  if (TextPost()) return;
  switch ($_GET['f'])
  {
    case 'add':         AddImage();     break;
    case 'delete':      DeleteImage();  break;
    case 'properties':  PropertiesAlbum();  break;
    case 'preview':     PreviewAlbum(); break;
    case 'move':        MoveAlbum();    break;
    case 'trunc':       TruncAlbum();   break;
    case 'delete_all':  DeleteAlbum();  break;
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
    case 'add':         AddImageForm();     break;
    case 'delete':      DeleteImageForm();  break;
    case 'properties':      PropertiesAlbumForm();  break;
    case 'preview':     PreviewAlbumForm(); break;
    case 'move':        MoveAlbumForm();    break;
    case 'trunc':       TruncAlbumForm();   break;
    case 'delete_all':  DeleteAlbumForm();  break;
    default: return false;
  }
  return true;
}

function Body()
{
  global $_page;
  
  //Administrační tlačítka
  if (IsLoged() || !LCK)
  {
    echo "<div class=\"toolbox\">";
    ToolButton("p=$_GET[p]", 'Zobrazit album', 'page', false);
    ToolButton("p=$_GET[p]&amp;a=album&amp;f=properties", 'Vlastnosti alba', 'buble');
    ToolButton("p=$_GET[p]&amp;a=album&amp;f=move", 'Přesunout album do jiné rubriky', 'folder');
    ToolButton("p=$_GET[p]&amp;a=album&amp;f=add", 'Přidat obrázek', 'plus');
    ShowAdminTextsTools();
    ToolButton("p=$_GET[p]&amp;a=album&amp;f=trunc", 'Smazat všechny obrázky', 'delete_all');
    ToolButton("p=$_GET[p]&amp;a=album&amp;f=delete_all", 'Smazat album', 'delete');
    echo "</div><div class=\"float_cl\">&nbsp;</div>";
  }
  
  if (Form()) return;
  ShowTexts(false);
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

//Vykreslí okno pro náhledy obrýzků
function PageBottom($wnd)
{
  global $_page; global $image_sql;
  
  //Recyklace SQL dotazu - pokud byly načteny obrázky již dříve, tak se nebude načítat
  if (!$image_sql || mysql_num_rows($image_sql) < $_page['ct'] )
    $image_sql = sql_select('galery','id, adr, width, height',
      "where pid = '$_page[id]' and zob = '1' order by id"
    );
  else mysql_data_seek($image_sql, 0);
  
  echo "\n<div class=\"box\" id=\"$wnd\"><table border=\"0\" class=\"wnd_table\">";
  echo "\n  <tr><td class=\"resize r_tl\" onmousedown=\"return StartAlbumResize(event, 'tl');\" title=\"Změna velikosti náhledu\" ></td><td class=\"resize r_t\" onmousedown=\"return StartAlbumResize(event, 't');\" title=\"Změna velikosti náhledu\" ></td><td class=\"resize r_tr\" onmousedown=\"return StartAlbumResize(event, 'tr');\" title=\"Změna velikosti náhledu\" ></td></tr>";
  echo "\n  <tr><td rowspan=\"2\" class=\"resize r_w\" onmousedown=\"return StartAlbumResize(event, 'l');\" title=\"Změna velikosti náhledu\" ></td><td class=\"wnd_tools\" onmousedown=\"return StartDragAndDrop('$wnd', event);\" title=\"Přesun okna náhledu\" ><a href=\"\" onclick=\"WndClose('$wnd'); return false;\" title=\"Uzavření okna náhledu\" ><img src=\"_graphics/icon/20_close.png\" alt=\"zavřít\" /></a></td><td rowspan=\"2\" class=\"resize r_w\" onmousedown=\"return StartAlbumResize(event, 'r');\" title=\"Změna velikosti náhledu\" ></td></tr>";
  echo "\n  <tr><td class=\"wnd_box\">";
  echo "\n    <div class=\"txt_box\" id=\"".$wnd."_text\"></div>";
  echo "\n    <div class=\"img_box\" id=\"".$wnd."_img\"><img onclick=\"WndClose('$wnd');\" src=\"\" alt=\"\" id=\"".$wnd."_image\" title=\"Klikutím uzavřete náhled\" /></div>";
  echo "\n    <div class=\"tools_box\" id=\"".$wnd."_toolbox\"><a href=\"\"  onclick=\"return ResizeAlbum(400, 300); \" class=\"zoom\" title=\"Změna velikosti náhledu - 400x300 pixelů\" ><img src=\"_graphics/icon/20_lupa.png\" alt=\"Zoom\" /><span>-2</span></a> <a href=\"\"  onclick=\"return ResizeAlbum(500, 400); \" class=\"zoom\" title=\"Změna velikosti náhledu - 500x400 pixelů\" ><img src=\"_graphics/icon/20_lupa.png\" alt=\"Zoom\" /><span>-1</span></a> <a href=\"\"  onclick=\"return ResizeAlbum(800, 500); \" class=\"zoom\" title=\"Změna velikosti náhledu - 800x500 pixelů\" ><img src=\"_graphics/icon/20_lupa.png\" alt=\"Zoom\" /><span>0</span></a> <a href=\"\"  onclick=\"return ResizeAlbum(1000, 700); \" class=\"zoom\" title=\"Změna velikosti náhledu - 1000x700 pixelů\" ><img src=\"_graphics/icon/20_lupa.png\" alt=\"Zoom\" /><span>+1</span></a> <a href=\"\"  onclick=\"return ResizeAlbum(1030, 1100); \" class=\"zoom\" title=\"Změna velikosti náhledu - 1030x1100 pixelů\" ><img src=\"_graphics/icon/20_lupa.png\" alt=\"Zoom\" /><span>+2</span></a> <a id=\"".$wnd."_href\" href=\"\" target=\"_blank\" onclick=\"return ShowImageWindow();\" style=\"float:right; margin: 2px 5px 0px 75px;\" title=\"Otevřít obrázek\" ><img src=\"_graphics/icon/20_file.png\" alt=\"Otevřít\" /></a> <a href=\"\" title=\"Předchozí obrázek\"  onclick=\"return ShowImage(-1); \" ><img src=\"_graphics/icon/32_larrow_red.png\" alt=\"Předchozí\" /></a><a href=\"\" title=\"Následující obrázek\"  onclick=\"return ShowImage(0); \" ><img src=\"_graphics/icon/32_rarrow_red.png\" alt=\"Další\" /></a></div>";
  echo "\n  </td></tr>";
  echo "\n  <tr><td class=\"resize r_bl\" onmousedown=\"return StartAlbumResize(event, 'bl');\" title=\"Změna velikosti náhledu\" ></td><td class=\"resize r_b\" onmousedown=\"return StartAlbumResize(event, 'b');\" title=\"Změna velikosti náhledu\" ></td><td class=\"resize r_br\" onmousedown=\"return StartAlbumResize(event, 'br');\" title=\"Změna velikosti náhledu\" ></td></tr>";
  echo "\n</table></div>";
  echo "\n<script type=\"text/javascript\">\n  var i = new Array({src:'',width:0,height:0}";
  
  //Výpis všech obrázků pro obsluhu javascriptem
  while ($data = mysql_fetch_array($image_sql))
    echo ",{src:'galery/$data[id]_$data[adr].jpg',width:$data[width],height:$data[height]}";

  echo ")\n  AlbumInit('$_page[name]', '$wnd', i, 400, 300, 30, 104);\n</script>";

}

//Zobrazení obsahu rubriky
function ShowGalery()
{
  global $_page; global $_inf; global $image_sql;
  
  $poz = IMAGE_LIMIT * ($_inf['s'] - 1);
  $image_sql = sql_select('galery','id, datum, pid, adr, width, height',
    "where pid = '$_page[id]' and zob = '1' order by id"
    , "$poz,". IMAGE_LIMIT
  );
  
  echo "\n<table class=\"items\" align=\"center\"><tr><td colspan=\"4\" class=\"stat\" >";
  if (ShowSwitcher(mysql_num_rows($image_sql), $_page['ct'], IMAGE_LIMIT) )
  {
    $_inf['bottom'] = true;
    echo "</td></tr>";
    
    $load = mysql_fetch_array($image_sql);
    while ($load)
    {
      //Načtení dat pro řádek tabulky
      for ($x = 1; $x <= TABLE_ITEMS; $x++)
        if ( $tab[$x] = $load ) $load = mysql_fetch_array($image_sql);

      //Administrační tlačítka
      if (!LCK || IsLoged())
      {
        echo "\n  <tr>";
        for ($x = 1; $x <= TABLE_ITEMS; $x++)
        {
          if ($data = $tab[$x])
          {             
            echo "<th><div class=\"toolbox\">";
            ToolButton("p=$_GET[p]&amp;a=album&amp;f=preview&amp;i=$data[id]", 'Nastavit jako náhled alba', 'lupa');
            ToolButton("p=$_GET[p]&amp;a=album&amp;f=delete&amp;i=$data[id]", 'Smazat obrázek', 'delete');
            echo "</div></th>"; 
          }
          else echo "<td class=\"blank\">&nbsp;</td>";
        }
        echo "</tr>";
      }
      
      echo "\n  <tr>";
      for ($x = 1; $x <= TABLE_ITEMS; $x++)
      {
        if ($data = $tab[$x])
        { 
          $adr = "$data[id]_$data[adr]"; $poz++;
          echo "<td class=\"imgBox\"><a href=\"image.php?p=$_GET[p]&amp;i=$poz\" target=\"_blank\" onclick=\"return ShowImage($poz); \" ><img src=\"galery/thumbs/$adr.jpg\" alt=\"$adr\" /></a></td>";
        }
        else echo "<td class=\"blank\">&nbsp;</td>";
      }
      echo "</tr>";
    }
  }
  else echo "</td></tr>";
  
  echo "</table>";
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

//Vloží obrázek do adresářové struktury a připraví SQL dotaz
//Vstup: ID formulářového pole, jméno, $nadpis, ID obrázku v databázi, pozice v databázi, test existence
//Výstup: část SQL INSERT pro vložení aktuálního obrázku ve tvaru:
//        (id, datum, pid, name, adr, ndp, poz)
function InsertImage($fid, $id, $test = true)
{
  global $_page;
  $fileName = "file$fid";
  
  //Vytvoření adresářové struktury 
  if(!is_dir("galery"))mkdir("galery");
  if(!is_dir("galery/thumbs"))mkdir("galery/thumbs");
        
  //Test existence obrázku - nevyplnění pole formuláře
  if(!is_file($_FILES[$fileName]['tmp_name']))
  {
    if ($test) ThrowFormError($fid, "Musíte zadat cestu k obrázku ve vašem počítači");
    return "";
  }
  
  //Získání přípony a jména, pokud nebylo zadáno
  preg_match("/^(.{0,50}).*\.(\S{3})$/", $_FILES[$fileName]['name'], $im);
  $name = $im[1];
  $prip = mb_strtolower($im[2], 'utf-8');
  $adres = MakeAdres($name);
  $imgAdres = $id ."_$adres.jpg";
    
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
  
  //Smazání existujících obrázků
  if (is_file("galery/$imgAdres")) unlink("galery/$imgAdres");
  if (is_file("galery/thumbs/$imgAdres")) unlink("galery/thumbs/$imgAdres");
  
  
  $vel = getimagesize($_FILES[$fileName]['tmp_name']);
  
  //Vytvoření obrázku pro náhledy
  $wid = $vel[0]; $hei = $vel[1];
  if($wid > THUMB_WIDTH) {$hei = (int)(($hei / $wid) * THUMB_WIDTH) ; $wid = THUMB_WIDTH ;}
  if($hei > THUMB_HEIGHT){$wid = (int)(($wid / $hei) * THUMB_HEIGHT); $hei = THUMB_HEIGHT;}
  $im = @imagecreatetruecolor($wid, $hei) or die("Cannot Initialize new GD image stream - new image");
  imagecopyresampled($im, $res, 0,0,0,0, $wid,$hei, $vel[0], $vel[1]);
  imagejpeg($im,"galery/thumbs/$imgAdres");
  imagedestroy($im);
  
  //Vytvoření nového obrázku v galerii
  $wid = $vel[0]; $hei = $vel[1];
  if ($wid > IMG_WIDTH) {$hei = (int)(($hei / $wid) * IMG_WIDTH) ; $wid = IMG_WIDTH ;}
  if ($hei > IMG_HEIGHT){$wid = (int)(($wid / $hei) * IMG_HEIGHT); $hei = IMG_HEIGHT;}
  $im = @imagecreatetruecolor($wid, $hei) or die("Cannot Initialize new GD image stream - new image");
  imagecopyresampled($im, $res, 0,0,0,0, $wid, $hei, $vel[0], $vel[1]);
  imagejpeg($im, "galery/$imgAdres");
  imagedestroy($res); imagedestroy($im);
  
  //Navrací součást přidávacího dotazu pro mysql databázi
  return "('$id', '". date("y-m-d H:i:s") ."', '$_page[id]', '$adres', '$wid', '$hei')";
}

//Funkce vymaže veškeré obrázky z konkrétní galrie podle ID článku
function DeleteAllImages($galeryId)
{
  $sql = sql_select('galery',"id, adr", "where pid = '$galeryId' and zob = 1");
  while ($data = mysql_fetch_array($sql))
  {
    $file = "$data[id]_$data[adr].jpg";
    if (is_file("galery/$file")) unlink("galery/$file");
    if (is_file("galery/thumbs/$file")) unlink("galery/thumbs/$file");
  }
  sql_delete('galery', "pid = '$galeryId' and zob = 1");
}

//// Zpracovatelské funkce

function AddImage()
{
  global $_page;
  
  $data = mysql_fetch_array( sql_select('galery', 'max(id) as id', "", 1));
  $id = $data ? $data['id'] + 1 : 1;
  $prev = $_page['img'] ? '' : ", img = '$id'";
      
  $dotaz = ""; $images = 0;
  for($x = 1; $x <= ADD_IMAGES_CT; $x++)
  {
    $d = InsertImage($x, $id, $false);
    if ($d)
    {
      $dotaz .= ($dotaz ? ',' : '') . $d;
      $id++; $images++;
    }
  }
      
  if ($dotaz)
  {
    //Úprava databáze
    sql_insert('galery','id, datum, pid, adr, width, height', $dotaz);
    sql_update('pages', "ct = ct + '$images'$prev", "id = $_page[id] and deleted = 0", 1);
    MakeRefresh();
  }
  else
    ThrowFormError(0, "Do databáze nebyl vložen žádný obrázek");
}
function DeleteImage()
{
  global $_page;
  
  $id = GetCollum(1);
  if (GetCollum(2))
  {
    $img_data = mysql_fetch_array( sql_select('galery',"id, adr", "where pid = '$_page[id]' and zob = 1 and id = '$id'", 1));
    if (!$img_data){ ThrowFormError(0, "Nelze nalézt vybraný obrázek"); return; }
    
    if (is_file("galery/$img_data[id]_$img_data[adr].jpg")) unlink("galery/$img_data[id]_$img_data[adr].jpg");
    if (is_file("galery/thumbs/$img_data[id]_$img_data[adr].jpg")) unlink("galery/thumbs/$img_data[id]_$img_data[adr].jpg");
    sql_delete('galery', "id = '$img_data[id]'", 1);
    
    $prev = '';
    //Obrázek byl současně náhledem alba
    if ($img_data['id'] == $_page['img'])
    {
      if ($_page['ct'] > 1)
      {
        $data = mysql_fetch_array( sql_select('galery','id, datum, pid, adr',
          "where pid = '$_page[id]' and id != '$id' and zob = '1' order by id", 1
        ));
        if ($data) $prev = ", img = '$data[id]'";
        else $prev = ', img = 0';
      }
      else $prev = ', img = 0';
    }
    
    sql_update('pages', "ct = ct - 1$prev", "id = $_page[id] and deleted = 0", 1);
    MakeRefresh();
  }
}
function PropertiesAlbum()
{
  global $_page;
  if ( LenghtTest(1, $name = GetCollum(1) , 3, 40)) return;
  if ( LenghtTest(2, $title = GetCollum(2) , 0, 100)) return;
  $adr = MakeAdres($name);
  sql_update('pages',"name = '$name', adr = '$adr', title = '$title'", "id = '$_page[id]'");
  MakeRefresh("index.php?p=$_page[id]_$adr");
}
//Nastavení náhledu
function PreviewAlbum()
{
  global $_page;
  $id = GetCollum(1);
  
  $img_data = mysql_fetch_array( sql_select('galery',"id, adr", "where pid = '$_page[id]' and zob = 1 and id = '$id'", 1));
  if (!$img_data){ ThrowFormError(0, "Nelze nalézt vybraný obrázek"); return; }
 
  sql_update('pages', "img = '$id'", "id = $_page[id] and deleted = 0", 1);
  MakeRefresh();
}
//Přesun alba do jíné galerie
function MoveAlbum()
{
  global $_page;
  if ( NumberTest(1, $id  = GetCollum(1) , 0, 0)) return;

  $data = mysql_fetch_array( sql_select('pages', 'id, color, pid',
    "where deleted = '0' and pid = '$id' and script = 'galery'"
  ));
  if (!$data){ ThrowFormError(0, "Cílová rubrika neexistuje"); return; }
  if ($_page['pid'] == $data['pid']) { ThrowFormError(1, "Vrámci stejné galerie není možno přesouvat."); return; }
  $color = $data['color'];

  sql_update('pages',"pid = '$data[pid]', color = '$color'", "id = '$_page[id]'");
  sql_update('pages', 'ct = ct - 1', "pid = $_page[pid] and script = 'galery' and deleted = 0", 1);
  sql_update('pages', 'ct = ct + 1', "id = $data[id] and deleted = 0", 1);
  
  MakeRefresh();
}
function TruncAlbum()
{
  global $_page;
  
  if (GetCollum(1))
  {
    DeleteAllImages($_page['id']);
    MakeRefresh();
    sql_update('pages', "ct = 0", "id = $_page[id] and deleted = 0", 1);
  }
}
function DeleteAlbum()
{
  global $_page;
  
  if (GetCollum(1))
  {
    DeleteAllImages($_page['id']);
    
    sql_delete('texts',"pid = '$_page[id]'");
    sql_update('pages',"deleted = '1'", "id = '$_page[id]'",1);
    
    $data = mysql_fetch_array( sql_select('pages', 'id, adr',
      "where deleted = '0' and script = 'galery' and pid = '$_page[pid]'"
    ));
    sql_update('pages', "ct = ct - 1", "pid = $_page[pid] and script = 'galery' and deleted = 0", 1);
    
    MakeRefresh("index.php?p=". ($data['id'] ? "$data[id]_$data[adr]" : $_page['pid']),2);
  }
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////

function AddImageForm()
{
  global $_inf;
  
  if (IsRefreshed("Obrázky byly přidány")) return;
  FormHead("Přidat obrázky");
  for($x = 1; $x <= ADD_IMAGES_CT; $x++) FileBox("Cesta k obrázku", $x);
  FormBottom('');
}
function DeleteImageForm()
{
  global $_inf; global $_page;
  if (IsRefreshed("Obrázek byl smazán")) return;

  $data = mysql_fetch_array( sql_select('galery',"id, datum, pid, adr",
    "where pid = '$_page[id]' and zob = 1 and id = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádný obrázek"); return; }

  FormHead("Smazat Obrázek");
  HiddenBox2(1, $data['id']);
  YNRadio("Opravdu chcete odstranit tento obrázek?", 2, 0);
  FormBottom('');
  echo "<img src=\"galery/thumbs/$data[id]_$data[adr].jpg\" alt=\"$adr\" /><br />&nbsp;";
}
function PropertiesAlbumForm()
{
  global $_page;
  if (IsRefreshed("Album bylo upraveno")) return;
  FormHead("Vlastnosti alba");
  TextBox("Jméno", 1, $_page['name'], 3, 40);
  TextBox("Titulek", 2, $_page['title'], 0, 100);
  FormBottom('', "'txt', 'txt'", "3, 0", "30, 100");
}
function PreviewAlbumForm()
{
  global $_inf; global $_page;
  if (IsRefreshed("Náhled alba byl změněn")) return;

  $data = mysql_fetch_array( sql_select('galery',"id, datum, pid, adr",
    "where pid = '$_page[id]' and zob = 1 and id = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádný obrázek"); return; }

  FormHead("Nastavit jako náhled alba");
  HiddenBox2(1, $data['id']);
  FormBottom('Odesláním zajistíte, že tento obrázek bude viditelný pod jménem alba v galerii.');
  echo "<img src=\"galery/thumbs/$data[id]_$data[adr].jpg\" alt=\"$adr\" /><br />&nbsp;";
} 
function MoveAlbumForm()
{
  global $_page;
  if (IsRefreshed("Album bylo přesunuto")) return;
  FormHead("Přesunout album do jiné galerie");

  $sql = sql_select('pages', 'name, id',
      "where deleted = '0' and script = 'rubric' and img != '0' order by name"
  );
  
  for ($x = 0; $data = mysql_fetch_array($sql) ; $x++)
  {
    $values[$x] = $data['id']; $texts[$x] = $data['name'];
  }

  SelectBox("Přesunout do galerie", 1, $_page['pid'], $values, $texts);
  FormBottom('');
}
function TruncAlbumForm()
{
  if (IsRefreshed("Album bylo vyprázdněno")) return;
  FormHead("Smazat všechny obrázky");
  YNRadio("Opravdu chcete odstranit veškeré obrázky z tohoto alba?", 1, 0);
  FormBottom('');
}
function DeleteAlbumForm()
{
  if (IsRefreshed("Album bylo smazáno")) return;
  FormHead("Smazat toto album");
  YNRadio("Opravdu chcete odstranit toto album?", 1, 0);
  FormBottom('');
}


?>