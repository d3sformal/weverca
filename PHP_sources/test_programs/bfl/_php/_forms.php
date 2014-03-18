<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
***************************  Funkce pro zobrazení a zpracování formulářů  **************************

 * Scrit obsahuje funkce pro načtení řádky formuláře, test jejich hodnot a výpis chybových hlášení
 * V další části jsou funkční celky pro vykreslení samotného formuláře 

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

/********** Funkce pro kontrolu formulářů *********************************************************/

//Zaznamená chybové hlášení při zpracovávání formulářů
function ThrowFormError($id, $errormessage)
{
  global $_inf; global $_error;
  $_inf['error'] = true;
  $_error[$id] = $errormessage;
}

//Navráti upravený obsah položky podle ID
function GetCollum($id)
{
  if (!isset($_POST['col'][$id])) return "";

  $col = preg_replace("/^([ \n\r\t\f])+|([ \n\r\t\f])+$/s", "", $_POST['col'][$id]);
  return addslashes( htmlspecialchars($col) );
}
//Navráti upravený obsah položky podle ID
function GetTextCollum($id)
{
  if (!isset($_POST['col'][$id])) return "";
  return addslashes($_POST['col'][$id]);
}

//Test přítomnosti mezer
function SpaceTest($id, $text)
{
  global $_inf;
  if (preg_match("/[ \n\r\t\f]/s",$text))
    ThrowFormError($id, "Toto pole nesmí obsahovat mezery!");

  return $_inf['error'];
}

//Čeština pro dialogy
function Czech1($num)
{
  return $num > 4 ? "ů" : ($num > 1 ? "y" : "");
}

//Test délky vstupu
function LenghtTest($id, $text, $min, $max, $duly = true)
{
  global $_inf;
  $lenght = mb_strlen($text,"utf8");       
 
  if(!$lenght and $min)
  {
    if ($duly) ThrowFormError($id, "Toto pole je povinné.");
  }
  elseif($lenght < $min)
    ThrowFormError($id, "Toto pole musí obsahovat nejméně $min znak". Czech1($min) .", zadal(a) jste $lenght znak". Czech1($lenght) ."!");
  elseif($lenght > $max)
    ThrowFormError($id, "Toto pole musí obsahovat nejvíce $max znak". Czech1($max) .", zadal(a) jste $lenght znak". Czech1($lenght) ."!");

  return $_inf['error'];
}

//Test číselnosti
function NumberTest($id, $number, $min, $max)
{
  global $_inf;
  if (!is_numeric($number))
    ThrowFormError($id, "Do tohoto pole musí být zadáno číslo");
    
  elseif(($min != 0 or $max != 0) and ($number < $min or $number > $max))
    ThrowFormError($id, "Do tohoto pole smí být zadáno číslo od $min do $max!");
  
  return $_inf['error'];
}

function MailTest($id, $mail, $duly)
{
  global $_inf;
  $lenght = mb_strlen($text,"utf8");
  if (!$lenght)
  {
    if ($duly) ThrowFormError($id, "Toto pole je povinné.");
  }
  elseif(!ereg(".+@.+\..+",$col[$x]) or ereg(" ",$col[$x]))
     ThrowFormError($id, "Zadejte platnou E-Mailovou adresu.");

  return $_inf['error'];
}

/********** Časté formuláře ***********************************************************************/
function PropertiesForm($ndp)
{
  global $_page;
  if (IsRefreshed("Stránka byla upravena")) return;
  FormHead("Vlastnosti $ndp");
  TextBox("Titulek", 1, $_page['name'], 3, 30);
  TextBox("Nadpis", 2, $_page['title'], 0, 100);
  RadioButtons("Obrázek stránky", 3, 0, Array(1, 0) , Array("Smazat obrázek", "Podle zadání") );
  FileBox("Cesta k obrázku", 4);
  SmallTextArea("Anotace", 5, $_page['text']);
  FormBottom("Pokud při volbě 'Podle zadání' necháte cestu k obrázku nevyplněnou, tak nedojde ke změně obrázku.<br>Titulek slouží jako jméno článku v menu a pokud není vyplněno pole Nadpis, tak i jako nadpis článku.<br>Nahrávejte pouze obrázky typu .JPG, .PNG, nebo .GIF, velikost obrázku nesmí přesáhnout 2048x2048px a 4MB dat. Větší obrázky zmenšete v programu pro úpravu fotografií.", "'txt', 'txt', 'nul', 'nul', 'txt'", "3, 0, 0, 0, 0", "30, 100, 0, 0, 10000");
}
function PropertiesPost()
{
  global $_page; global $_inf;
  if ( LenghtTest(1, $name = GetCollum(1) , 3, 30)) return;
  if ( LenghtTest(2, $title = GetCollum(2) , 0, 100)) return;
  if ( LenghtTest(5, $text = GetTextCollum(5) , 0, 10000)) return;
  $adr = MakeAdres($name);
  $dotaz = '';
  $text_changed = false;
  
  if ($name != $_page['name'])
  {
    $dotaz = "name = '$name', adr = '$adr'";
  }
  
  if ($title != $_page['title'])
  {
    if ($dotaz) $dotaz .= ", ";
    $dotaz .= "title = '$title'";
  }
  
  if (GetCollum(3))
  {
    if ($_page['img'] == 1)
    {
      $f = "page_image/$_page[id].jpg";
      if (is_file($f)) unlink($f);    
      if ($dotaz) $dotaz .= ", ";
      $dotaz .= "img = '0'";
    }
  }
  //Načtení náhledového obrázku
  else
  {
    $fileName = "file4";
    $f = "page_image/$_page[id].jpg";
    $fid = 4;
    
    //Vytvoření adresářové struktury 
    if(!is_dir("page_image"))mkdir("page_image");
          
    //Test existence obrázku - nevyplnění pole formuláře
    if(is_file($_FILES[$fileName]['tmp_name']))
    {
      //Získání přípony a jména, pokud nebylo zadáno
      preg_match("/^(.{0,50}).*\.(\S{3})$/", $_FILES[$fileName]['name'], $im);
      $prip = mb_strtolower($im[2], 'utf-8');
        
      $vel = getimagesize($_FILES[$fileName]['tmp_name']);
      $wid = $vel[0]; $hei = $vel[1];
      $imageSize = $vel[0] * ($vel[1] / 1000) * 4 + (memory_get_usage() / 1000);
            
      if ($imageSize > 30000)
      {
        ThrowFormError($fid, "Obrázek je příliš veliký, musíte jej zmenšit na velikost nejvíce 2048 x 2048 pixelů");
        return "";
      }
        
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
      if (is_file($f)) unlink($f);

      if ($wid > PAGE_IMG_WIDTH) {$hei = (int)(($hei / $wid) * PAGE_IMG_WIDTH) ; $wid = PAGE_IMG_WIDTH ;}
      if ($hei > PAGE_IMG_HEIGHT){$wid = (int)(($wid / $hei) * PAGE_IMG_HEIGHT); $hei = PAGE_IMG_HEIGHT;}
      $im = @imagecreatetruecolor($wid, $hei) or die("Cannot Initialize new GD image stream - new image");
      imagecopyresampled($im, $res, 0,0,0,0, $wid,$hei, $vel[0], $vel[1]);
      imagejpeg($im,$f);
      imagedestroy($im);
      
      if ($dotaz) $dotaz .= ", ";
      $dotaz .= "img = '1'";
    }
  }
  
  //Načtení textu anotace
  if ($text != $_page['text'])
  {
    $text_changed = true;
    $datum = date("y-m-d H:i:s");
    if ($_page['tid'] == 0)
    {
      sql_insert('texts',"text,pid,poz,datum,zob","('$text','$_page[id]','0','$datum','2')");
      $data = mysql_fetch_array(
        sql_select('texts',"id", "where pid = '$_page[id]' and zob = '2' and datum = '$datum'")
      );
      
      if ($data)
      {
        if ($dotaz) $dotaz .= ", ";
        $dotaz .= "tid = '$data[id]'";
      }
    }
    else
    {
      sql_update('texts',"text = '$text', datum = '$datum'", "id = '$_page[tid]'");
    }
  }
  if (!$dotaz) 
  { 
    if (!$text_changed) { ThrowFormError(0, "Neprovedl jste žádnou změnu"); return; }
  }
  else sql_update('pages',$dotaz, "id = '$_page[id]'");
  
  if ($_GET['b'] == 0) MakeRefresh("index.php?p=$_page[id]_$adr");
  else MakeRefresh("index.php?p=$_GET[b]&amp;s=$_inf[s]#item$_page[id]");
}

/********** Funkce pro vykreslení formulářů *******************************************************/

//Hlavička standartního formuláře
//Vstup: aresa zpracování, ID formuláře, nadpis formuláře
function FormHead($name, $adr = "", $show_button = true)
{
  global $_error; global $_inf;
  //Hlavička
  if (!$adr) $adr = GetLink();  
  $eBox = $_inf['error'] ? "" : " style=\"display:none\"";
  $eMsg = $_error[0] ? "<br>$_error[0]" : "";
  
?>
        <div class="form">
          
          <form action="<?=$adr?>" method="post" id="form" name="form" enctype="multipart/form-data" onsubmit="return FormCheck();" >
            <input type="hidden" value="1" name="fid" />          
<?        if($show_button)
          {?>
            <input type="submit" value="Odeslat formulář" class="submit_button" />
<?        }?>
            <h3><?=$name?></h3>
            <div class="form_error" id="form_error"<?=$eBox?>>Při zpracovávání folmuláře došlo k chybám<?=$eMsg?></div>
<?
}

//Zobrazí konec formuláře s dodatečným textem
function FormBottom($text, $scripts = "", $minims = "", $maxims = "", $show_button = false)
{
  if ($scripts) $scripts = ', '.$scripts;
  if ($minims) $minims = ', '.$minims;
  if ($maxims) $maxims = ', '.$maxims;
  
          if($show_button)
          {?>
            <input type="submit" value="Odeslat formulář" class="submit_button bottom_button" />
<?        }?>
          </form>
          
          <script type="text/javascript"> 
            scripts = new Array('nul'<?=$scripts?>); minims = new Array(0<?=$minims?>); maxims = new Array(0<?=$maxims?>)
          </script>
           
           <div class="float_cleaner"></div>
        </div>
        <div class="form_text"><p><?=$text?></p></div>
<?
}

function Label($text)
{
?>
          <p><?=$text?></p>
<?
}

//Skrytý prvek formuláře
function HiddenBox($name, $value)
{
?>
              <input name="<?=$name?>" id="<?=$name?>" type="hidden" value="<?=$value?>" />
<?
}

function HiddenBox2($id, $value)
{
?>
              <input name="col[<?=$id?>]" id="col<?=$id?>" type="hidden" value="<?=$value?>" />
<?
}

//Jednoduché textové pole
function TextBox($name, $id, $value, $min = -1, $max = -1, $duly = true)
{
  global $_error;

  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];
  $length = mb_strlen($value,"utf8");
  
  $e = $_error[$id] ? "" : " style=\"display:none\"";
  $c = $_error[$id] || ($duly && $min > 0 && $length == 0) ? " error" : "";    
  
?>
            <div class="form_item<?=$c?>" id="c<?=$id?>">
              <div class="form_error" id="err_<?=$id?>"<?=$e?>><?=$_error[$id]?></div>
              <label for="col<?=$id?>" class="form_label"><?=$name?></label>
              <input name="col[<?=$id?>]" id="col<?=$id?>" type="text" value="<?=$value?>" class="text_box" onkeyup="Typing(<?=$id?>)" onblur="Typing(<?=$id?>)" />
          <?if($min != -1 && $max != -1)
            {?>
              <div class="text_limits">
                <span id="num<?=$id?>" class="help" title="Napsané znaky"><?=$length?></span>
                | <span class="help" title="Minimální počet znaků"><?=$min?></span>
                | <span class="help" title="Maximální počet znaků"><?=$max?></span>
              </div>
<?          }?>
            </div>
<?
}

//Pole pro heslo
function PasswordBox($name, $id, $value, $min = -1, $max = -1, $duly = true)
{
  global $_error;
  $e = $_error[$id] ? "" : " style=\"display:none\"";
  $c = $_error[$id] || ($duly && $min > 0 && $length == 0) ? " error" : "";    
  
?>
            <div class="form_item<?=$c?>" id="c<?=$id?>">
              <div class="form_error" id="err_<?=$id?>"<?=$e?>><?=$_error[$id]?></div>
              <label for="col<?=$id?>" class="form_label"><?=$name?></label>
              <input name="col[<?=$id?>]" id="col<?=$id?>" type="password" value="" class="text_box" onkeyup="Typing(<?=$id?>)" onblur="Typing(<?=$id?>)" />
          <?if($min != -1 && $max != -1)
            {?>
              <div class="text_limits">
                <span id="num<?=$id?>" class="help" title="Napsané znaky">0</span>
                | <span class="help" title="Minimální počet znaků"><?=$min?></span>
                | <span class="help" title="Maximální počet znaků"><?=$max?></span>
              </div>
<?          }?>
            </div>
<?
}

//Pole pro soubor
function FileBox($name, $id)
{
  global $_error;
  $e = $_error[$id] ? "" : " style=\"display:none\"";
  $c = $_error[$id] || ($duly && $min > 0 && $length == 0) ? " error" : "";    
?>
            <div class="form_item<?=$c?>" id="c<?=$id?>">
              <div class="form_error" id="err_<?=$id?>"<?=$e?>><?=$_error[$id]?></div>
              <label for="col<?=$id?>" class="form_label"><?=$name?></label>
              <input name="file<?=$id?>" id="col<?=$id?>" type="file" class="file_box" />
            </div>
<?
}

//Zobrazení přepínačů
//Vstup: jméno, ID, výchozí hodnota, pole hodnot, pole popisků
function RadioButtons($name, $id, $value, $values, $texts)
{
  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];
  $z = 0;
  
?>
            <div class="form_item<?=$c?>" id="c<?=$id?>">
              <div class="form_error" id="err_<?=$id?>"<?=$e?>></div>
              <label class="form_label"><?=$name?></label>
              <div class="rb_box_area">
<?            foreach ($values as $val)
              {
                $col = $id."_$z";
?>
                <label class="rb_label" for="col<?=$col?>"><?=$texts[$z]?></label>
                <input name="col[<?=$id?>]" id="col<?=$col?>" type="radio" value="<?=$val?>" class="imp"<?=($val == $value ? ' checked="checked"' : '')?> />
              
<?              $z++;
              }?>
              </div>
            </div>
<?
}

//Předpřipravený YES/NO přepínač
function YNRadio($name, $id, $value)
{
  RadioButtons($name, $id, $value, Array(1, 0) , Array("Ano", "Ne") );
}

//Víceřádková TEXTAREA s plnou nabídkou
function TextArea($name, $id, $value)
{
  global $_page;
  global $_error;
  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];  
  $e = $_error[$id] ? "" : " style=\"display:none\"";
  $c = $_error[$id] || ($duly && $min > 0 && $length == 0) ? " error" : "";    
 
  
  ?>
            <div class="form_item<?=$c?>" id="c<?=$id?>">
              <div class="form_error" id="err_<?=$id?>"<?=$e?>><?=$_error[$id]?></div>        
              <div class="text_area">
                <textarea class="text_box" id="col<?=$id?>" name="col[<?=$id?>]" rows="15" cols="68" ><?=$value?></textarea>
              </div>
                    
              <script type="text/javascript">
                $(document).ready(function() {
                  $("#col<?=$id?>").cleditor(
                  {
                    width:        855, // width not including margins, borders or padding
                    height:       400 // height not including margins, borders or padding);
                  })
                });
              </script>
            </div>
<?
}

//Menší TEXTAREA s omezenou nabídkou
function SmallTextArea($name, $id, $value)
{
  global $_page;
  global $_error;
  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];  
  
  $e = $_error[$id] ? "" : " style=\"display:none\"";
  $c = $_error[$id] || ($duly && $min > 0 && $length == 0) ? " error" : "";    
 
  ?>
           
            <div class="form_item<?=$c?>" id="c<?=$id?>">
              <div class="form_error" id="err_<?=$id?>"<?=$e?>><?=$_error[$id]?></div>
              <label for="col<?=$id?>" class="form_label"><?=$name?></label>
              <div class="small_textarea">
                <textarea class="text_box" id="col<?=$id?>" name="col[<?=$id?>]" rows="15" cols="68" ><?=$value?></textarea>
              </div>
              <div class="float_cleaner"></div>
            </div>
                    
            <script type="text/javascript">
              $(document).ready(function() {
                $("#col<?=$id?>").cleditor(
                {
                  width:        400, // width not including margins, borders or padding
                  height:       200, // height not including margins, borders or padding);          
                  controls:     // controls to add to the toolbar
                        "bold italic underline | color highlight removeformat | undo redo | " +
                        "link unlink | cut copy paste pastetext | print source",
                })
              });
            </script>
<?
}

//Menší TEXTAREA s nabídkou pro diskusi
function CommentTextArea($name, $id, $value)
{
  global $_page;
  global $_error;
  if (isset($_POST['col'][$id])) $value = $_POST['col'][$id];  
  
  $e = $_error[$id] ? "" : " style=\"display:none\"";
  $c = $_error[$id] || ($duly && $min > 0 && $length == 0) ? " error" : "";    
 
  ?>
           
            <div class="form_item<?=$c?>" id="c<?=$id?>">
              <div class="form_error" id="err_<?=$id?>"<?=$e?>><?=$_error[$id]?></div>
              <label for="col<?=$id?>" class="form_label"><?=$name?></label>
              <div class="small_textarea">
                <textarea class="text_box" id="col<?=$id?>" name="col[<?=$id?>]" rows="15" cols="68" ><?=$value?></textarea>
              </div>
              <div class="float_cleaner"></div>
            </div>
                    
            <script type="text/javascript">
              $(document).ready(function() {
                $("#col<?=$id?>").cleditor(
                {
                  width:        400, // width not including margins, borders or padding
                  height:       200, // height not including margins, borders or padding);          
                  controls:     // controls to add to the toolbar
                        "bold italic underline removeformat | undo redo | cut copy paste pastetext | print source",
                })
              });
            </script>
<?
}

?>
