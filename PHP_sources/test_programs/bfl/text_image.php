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
    case ADD_FORM:       AddImage();      break;
    case DELETE_FORM:    DeleteImage();   break;
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
    case ADD_FORM:       AddImageForm();      break;
    case DELETE_FORM:    DeleteImageForm();   break;
    default: return false;
  }
  return true;
}

$_inf['admin'] = true;
function PageAdministration()
{    
    ToolButton("p=$_GET[p]", 'Zobrazit obrázky');
    ToolButton("p=$_GET[p]&amp;a=image&amp;f=add", 'Přidat obrázky', "");
}

//zapíše informace do hlavičky stránky pro vložení CSS a JS souborů
$_inf['header'] = true;
function PageHeader()
{
  ShowMenuHeader();
}

//Zobrazení stránky
function Body()
{
  global $_page;
  
  if (Form()) return;
  {
    ShowImages();
  }
}

function ShowImages()
{
  global $_page; global $_inf;
  
  $poz = IMAGE_LIMIT * ($_inf['s'] - 1);
  $image_sql = sql_select('galery','id, datum, adr, width, height',
    "where pid = '0' and zob = '1' order by id"
    , "$poz,". IMAGE_LIMIT
  );
  
   
  for($x = 1; $data = mysql_fetch_array($image_sql); $x++)
  {
    $image_data[$x]['id'] = $data['id'];
    $image_data[$x]['adr'] = "$data[id]_$data[adr]";
    $image_data[$x]['date'] = GetDatetime($data['datum']);
  }
    
  ShowImagesListToPage($image_data, mysql_num_rows($image_sql));
  ShowSwitcher(mysql_num_rows($image_sql), $_page['ct_images'], IMAGE_LIMIT);
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
  $name = $im[1];
  $prip = mb_strtolower($im[2], 'utf-8');
  $adres = MakeAdres($name);
  $imgAdres = $id ."_$adres.jpg";
    
  //Vytvoření kontextu obrázku k překopírování
  $res = null;
  switch($prip)
  {
    //case "bmp": $res = @imagecreatefromwbmp($_FILES["$fileName"]['tmp_name']);    break;
    case "jpg": $res = @imagecreatefromjpeg($_FILES[$fileName]['tmp_name']) or die("jpg error");  break;
    case "gif": $res = @imagecreatefromgif( $_FILES[$fileName]['tmp_name']) or die("gif error");  break;
    case "png": $res = @imagecreatefrompng( $_FILES[$fileName]['tmp_name']) or die("png error");  break;
    
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
  
  echo "$imgAdres";
  
  if($hei != THUMB_HEIGHT){$wid = (int)(($wid / $hei) * THUMB_HEIGHT); $hei = THUMB_HEIGHT;}
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
  return "('$id', '". date("y-m-d H:i:s") ."', '0', '$adres', '$wid', '$hei')";
}

//// Zpracovatelské funkce

function AddImage()
{
  global $_page;  
  $data = mysql_fetch_array( sql_select('galery', 'max(id) as id', "", 1));
  $id = $data ? $data['id'] + 1 : 1;
  
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
    sql_update('pages', "ct_images = ct_images + '$images'", "id = $_page[id] and deleted = 0", 1);
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
    $img_data = mysql_fetch_array( sql_select('galery',"id, adr", "where pid = '0' and zob = 1 and id = '$id'", 1));
    if (!$img_data){ ThrowFormError(0, "Nelze nalézt vybraný obrázek"); return; }
    
    if (is_file("galery/$img_data[id]_$img_data[adr].jpg")) unlink("galery/$img_data[id]_$img_data[adr].jpg");
    if (is_file("galery/thumbs/$img_data[id]_$img_data[adr].jpg")) unlink("galery/thumbs/$img_data[id]_$img_data[adr].jpg");
    sql_delete('galery', "id = '$img_data[id]'", 1);
    sql_update('pages', "ct_images = ct_images - 1", "id = $_page[id] and deleted = 0", 1);
    MakeRefresh();
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
    "where pid = '0' and zob = 1 and id = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádný obrázek"); return; }

  FormHead("Smazat Obrázek");
  HiddenBox2(1, $data['id']);
  YNRadio("Opravdu chcete odstranit tento obrázek?", 2, 0);
  FormBottom("<img src=\"galery/thumbs/$data[id]_$data[adr].jpg\" alt=\"$adr\" />");
}

?>