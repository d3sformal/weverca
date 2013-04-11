<?
/***************************************************************************************************
****************  Vyvolávací stránka AJAX po žadavku pro výběr obrázku do formuláře  ***************

 * Vyvoláno po kliknutí na tlačítko Vložit Obrázek při zadávání textu stránky
 * Umožňuje administrátorům snadno vybrat jakýkoliv obrázek, který je uložen v galerii, a pomocí
 * formátovacích značek vložit na pozici kurzoru do formulářového prvku. 
 * Po zavolání provede iniciaci, vložení ostatních scriptů, načtení dat z databáze, výběr alba,
 * galerie a obrázku. Obrázek je do elementu formuláře vkládán javascriptem

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

  //zámek scriptů
  define("AUTORIZACE","Q.&dJhůs63d5dS=d56sLc5%");
  define("IMG_WIDTH" , 100);
  define("IMG_HEIGHT", 100);
  $_inf['page'] = "insert_image.php";
  
  session_start();
  require_once "conf.php";
  require_once "_php/_definitions.php";
  require_once "_php/_database.php";
  require_once "_php/_pages.php";
  
  $pid = GetPageID();   //ID galerie
  $aid = $_inf['i'];    //ID alba
  
  //Uzavírací tlačítko
  echo "<div class=\"toolbox\"><a href=\"\" onclick=\"WndClose('insert_image'); RefreshIframeSize(); return false;\" title=\"Zavřít výběr obrázku\" ><img src=\"_graphics/icon/20_close.png\" alt=\"Zavřít výběr obrázku\" ></a></div>";

  
  //Výběr výčtu galerií a zobrazení pomocí elementu SELECT
  $galery = false;
  $sql = sql_select('pages', 'name, id',
    "where img = '1' and deleted = '0' and script = 'rubric' order by name"
  );
  
  echo "<select style=\"width:200px; height:20px; float:none; margin:0px 2px 5px 2px;\" onchange=\"val=this.options[this.selectedIndex].value; InsertImage(0, val, 0, 0); \" id=\"galery\"><option value=\"0\">-vyberte galerii-</option>";
  while ($data = mysql_fetch_array($sql))
  {
    if ($data['id'] == $pid) {$galery = true; $ch = ' selected="1"'; }
    else $ch = '';
    echo "<option value=\"$data[id]\" $ch>$data[name]</option>";
  }
  echo "</select>";
  
  //Výběr výčtu alb a zobrazení pomocí elementu SELECT
  $album = false;
  if ($galery)
  {
    $sql = sql_select('pages', 'name, id, ct',
      "where pid = '$pid' and deleted = '0' and script = 'album' order by id desc"
    );
    
    echo "<select style=\"width:200px; height:20px; float:none; margin:0px 2px 5px 2px;\" onchange=\"val=this.options[this.selectedIndex].value; InsertImage(0, $pid, val, 1); \" id=\"galery\"><option value=\"0\">-vyberte album-</option>";

    while ($data = mysql_fetch_array($sql))
    {
      if ($data['id'] == $aid) {$album = true; $ch = ' selected="1"'; }
      else $ch = '';
      $cz = (!$data['ct'] || $data['ct'] > 4) ? "obrázků" : ( $data['ct'] > 1 ? "obrázky" : "obrázek" ) ;
      echo "<option value=\"$data[id]\" $ch>$data[name] - $data[ct] $cz</option>";
    }
    echo "</select>";
  }
  
  
  //Zobrazení tabulky obrázků
  if ($album)
  {
    $poz = IMAGE_LIMIT * ($_inf['s'] - 1);
    $sql = sql_select('galery','id, datum, pid, adr, width, height',
      "where pid = '$aid' and zob = '1' order by id"
    );
    $data = mysql_fetch_array($sql);
  
    if ($ct = mysql_num_rows($sql))
    {
      $rows = $ct < 5 ? $ct : 5;
      echo "\n<table align=\"center\">";
      while ($data)
      {
        //Načtení dat pro řádek tabulky
        echo "\n  <tr>";
        
        for ($x = 1; $x <= $rows; $x++)
        {
          if ($data)
          {   
            $adr = "$data[id]_$data[adr]";
            $vel = getimagesize("galery/thumbs/$adr.jpg");
            $wid = $vel[0]; $hei = $vel[1];
            if ($wid > IMG_WIDTH) {$hei = (int)(($hei / $wid) * IMG_WIDTH) ; $wid = IMG_WIDTH ;}
            if ($hei > IMG_HEIGHT){$wid = (int)(($wid / $hei) * IMG_HEIGHT); $hei = IMG_HEIGHT;}
            echo "<td class=\"imgBox\"><a href=\"javascript:InsertImageTag('galery/thumbs/$adr.jpg')\" title=\"Vložit obrázek\" ><img src=\"galery/thumbs/$adr.jpg\" alt=\"$adr\" width=\"$wid\" height=\"$hei\" /></a></td>";
            $data = mysql_fetch_array($sql);
          }
          else echo "<td class=\"blank\">&nbsp;</td>";
        }
        echo "</tr>";
      }
      echo "</table>";
    }
    else Alert('Nenalezen žádný orázek');
  }
  
  

?>