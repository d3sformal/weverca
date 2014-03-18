<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
**********************************  Funkce pro zobrazení stránek  **********************************

 * Script obsahuje funkce pro zobrazení prvků stránek

MYSQL tabulka pro uložení stránek
 * ID, jméno, adresa, nadpis, náhledový text, ID náhledového obrázku, počet položek, barevné schéma,
 * ikona, datum vytvoření, ID nadřazené stránky, pozice v menu, prováděcí PHP script,
 * přístupnost (1 = pouze pro přihlášené), zámek (1 = nelze editovat), příznak smazáno (1 = smazáno)
 * Vložení důležitých stránek - úvod, administrace, hlavní menu, změna přihlašovacích údajů
 * správa administrátorů, statistika
 * Zbytek si vytvoří administrátoři     

CREATE TABLE IF NOT EXISTS `pages` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(40) COLLATE utf8_czech_ci NOT NULL,
  `adr` varchar(50) COLLATE utf8_czech_ci NOT NULL,
  `title` varchar(100) COLLATE utf8_czech_ci NOT NULL,
  `text` varchar(200) COLLATE utf8_czech_ci NOT NULL,
  `img` int(10) unsigned NOT NULL DEFAULT '0',
  `ct_menu` smallint(5) unsigned NOT NULL DEFAULT '0',
  `ct_images` smallint(5) unsigned NOT NULL DEFAULT '0',
  `icon` varchar(15) COLLATE utf8_czech_ci NOT NULL,
  `datum` datetime NOT NULL,
  `pid` int(10) unsigned NOT NULL DEFAULT '0',  
  `mid` int(10) unsigned NOT NULL DEFAULT '0',
  `level` tinyint(4) NOT NULL DEFAULT '0',
  `pos` int(10) unsigned NOT NULL DEFAULT '0',
  `script` varchar(10) COLLATE utf8_czech_ci NOT NULL DEFAULT '',
  `acces` tinyint(4) NOT NULL DEFAULT '0',
  `lck` tinyint(4) NOT NULL DEFAULT '0',
  `deleted` tinyint(4) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=utf8 COLLATE=utf8_czech_ci AUTO_INCREMENT=7 ;

INSERT INTO `pages` (`id`, `name`, `adr`, `title`, `ct_menu`, `ct_images`, `icon`, `datum`, `pid`, `mid`, `level`, `pos`, `script`, `acces`, `lck`, `deleted`) VALUES
(1, 'Úvod', 'uvod', 'Úvodní stránka', 0, 0, 'brick_house', '2010-08-30 11:05:50', 0, 1, 1, 1, '', 0, 1, 0),
(2, 'Administrace', 'administrace', '', 0, 0, 'key', '2010-08-30 11:06:25', 0, 2, 0, 0, 'rubric', 1, 1, 0),
(3, 'Hlavní menu', 'hlavni_menu', '', 1, 0, 'folder', '2010-08-30 14:00:42', 2, 2, 2, 2, 'main_menu', 0, 1, 0),
(4, 'Změna přihlašovacích údajů', 'zmena_prihlasovacich_udaju', '', 0, 0, 'key2', '2010-09-08 13:57:01', 2, 2, 2, 1, 'profil', 1, 1, 0),
(5, 'Správa administrátorů', 'sprava_administratoru', '', 0, 0, 'couple', '2010-09-08 13:57:24', 2, 2, 2, 3, 'users', 1, 1, 0),
(6, 'Statistika stránek', 'statistika_stranek', '', 0, 0, 'statistic', '2010-09-09 15:31:38', 2, 2, 2, 4, 'statistic', 0, 1, 0);

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

function ShowCleditorHeader()
{
?>
  <link rel="stylesheet" type="text/css" href="cleditor/jquery.cleditor.css" />
  <script src="cleditor/jquery.cleditor.min.js" type="text/javascript"></script>
  <script src="cleditor/jquery.cleditor.table.min.js" type="text/javascript"></script>
  <script src="cleditor/jquery.cleditor.xhtml.min.js" type="text/javascript"></script>
<?
}

function ShowFancyboxHeader()
{
?>
  <link rel="stylesheet" type="text/css" href="fancybox/jquery.fancybox-1.3.4.css" />
  <script src="fancybox/jquery.fancybox-1.3.4.js" type="text/javascript"></script>
  <script src="fancybox/jquery.mousewheel-3.0.4.pack.js" type="text/javascript"></script>
<?
}

function ShowGaleryHeader()
{
?>
  <link rel="stylesheet" type="text/css" href="_css/galery.css" media="screen" /> 
<?
}

function ShowMenuHeader()
{
?>
  <link rel="stylesheet" type="text/css" href="_css/page_menu.css" media="screen" /> 
<?
}

function ShowCommentsHeader()
{
?>
  <link rel="stylesheet" type="text/css" href="_css/comments.css" media="screen" /> 
<?
}

function ShowFormHeader()
{
?>
  <link rel="stylesheet" type="text/css" href="_css/form.css" media="screen" /> 
<?
}


//Vytiskne celý HTML kód stránky - hlavičku, menu první a druhé úrovně, tělo stránky a zápatí
function ShowPage()
{
  global $_page;
  global $_inf;
  
  $menu = Menu();
  $parent = GetParent();
  $title = $_page['title'] ? $_page['title'] : $_page['name'];

?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">

<head>
  <link rel="shortcut icon" href="favicon.ico">
  
<?if($_inf['refresh'])
  {?>
  <meta http-equiv="refresh" content="<?=REFRESH_TIME?>;url=<?=$_inf['refresh']?>" />
<?}?>
  <meta http-equiv="content-type" content="text/html; charset=utf-8" />
  <title>Building For Life: <?=$_page['name']?></title>
  
  
  <link rel="stylesheet" type="text/css" href="_css/page_style.css" media="screen" />
  <script src="_javascript/main.js" type="text/javascript"></script>
  <script src="_javascript/jquery.min.js" type="text/javascript"></script>

<?
  if ($_inf['f']) ShowFormHeader();
  if ($_inf['header']) PageHeader();
?>
</head>

<body>
  <div id="logo"></div>    
  
  <!-- Menu prvni urovne -->
  <div id="main_menu">
    <div id="menu_left_background">
      <div id="menu_right_background">
        <ul>
<?      for($x = 0; $x < $menu[0][1]; $x++)
        {?>
          <li><a href="index.php?p=<?=$menu[1][$x]['adr']?>">: <?=$menu[1][$x]['name']?> :</a></li>
<?      }?>
          <li class="float_cleaner"></li>
        </ul>
      </div>
    </div>
  </div>
  
  <div id="page_container">

<?if($menu[0][2] > 0)
  {?>
    <!-- Menu druhe urovne -->
    <div id="second_menu">
      <ul id="second_menu">
<?    if($parent[0] > 0)
      {?>
        <li class="parent_link"><a href="index.php?p=<?=$parent['adr']?>">Zpět do <?=$parent['name']?></a></li>
<?    }?>
<?    for($x = 0; $x < $menu[0][2]; $x++)
      {?>
        <li><a href="index.php?p=<?=$menu[2][$x]['adr']?>"><?=$menu[2][$x]['name']?></a></li>
<?    }?>
        <li class="float_cleaner"></li>
      </ul>
    </div>

<?}?>
    <div id="page">
      <div id="excavator_background">
        <div id="shadow_background">&nbsp;</div>
        
        <h1><?=$title?></h1>
<?    if($_inf['admin'] && IsLoged())
      {?>
        <div id="page_admin_tray">        
          <div class="admin_tools">
          <?PageAdministration()?>
          </div>
          <div class="float_cleaner">&nbsp;</div>
        </div>
<?    }?>

<?      Body();?>
        
        <div id="page_shade">&nbsp;</div>
      
      </div>
    </div>
    
    <div id="background_contaier"></div>
  
  
    <!-- Zapati stranek - informace o technologiich, credits a copyright -->
    <div id="bottom">
<?  if(IsLoged())
    {?>
      <p>Přihlášený uživatel: <?=$_SESSION[name]?> | <a href="index.php?o=logout&amp;<?=GetLink(false)?>">Odhlášení</a></p>
      <p><a href="index.php?p=<?=PROF_ID?>_administrace">Administrátorské rozhranní</a></p>
<?  }
    else
    {?>
      <p><a href="index.php?o=login&amp;<?=GetLink(false)?>">Administrátorské rozhranní</a></p>
<?  }?>
      <p>
        <a href="index.php">Building For Life</a>
        | Šíře stránky: 900px
        | Pro bezchybný chod mějte aktivován <strong>Javascript</strong> a <strong>CSS</strong>
        | Testováno na: 
        <img src="_graphics/icon/20_chrome.png" alt="Google Chrome v. 6.0.472.55" title="Google Chrome v. 6.0.472.55" />
        <img src="_graphics/icon/20_firefox.png" alt="Mozzila Firefox v. 3.6.9" title="Mozzila Firefox v. 3.6.9" />
        <img src="_graphics/icon/20_IE8.png" alt="Internet Explorer 8" title="Internet Explorer 8" />
        <img src="_graphics/icon/20_opera.png" alt="Opera v. 10.62" title="Opera v. 10.62" />
      </p>
      <p>Veškerý obsah stránek buildingforlife.cz je majetkem administrátorů a nesmí být bez jejich svolení dále kopírován.</p>
      <p>Zdrojový kód PHP, CSS, HTML vytvořil Pavel Baštecký © 2012 | Designed by Filip Signature Works © 2012</p>
    </div>

  </div>
  
</body>

</html>
<?

}

//Zobrazí bloky textu na stránku
function ShowTextToPage($sql, $admin_tray = true)
{
  global $_inf;
  $admin_tray = $admin_tray && IsLoged();  
      
?>
        <div id="page_text" >
<?      while($data = mysql_fetch_array($sql))
        {
          if ($admin_tray)
          {?>

          <div class="text_admin_tray">    
            <div class="admin_tools">
              <?
              ToolButton("p=$_GET[p]&amp;a=text&amp;f=add&amp;i=$data[poz]", 'Přidat nový blok na toto místo');
              ToolButton("p=$_GET[p]&amp;a=text&amp;f=edit&amp;i=$data[poz]", 'Upravit blok textu');
              ToolButton("p=$_GET[p]&amp;a=text&amp;f=delete&amp;i=$data[poz]", 'Smazat blok textu', "");
?>
            </div>
            <div class="float_cleaner">&nbsp;</div>
          </div>

<?        }?>
          <div class="text">
            <?=stripslashes($data['text'])?>
          </div> 
<?      }?>
        </div>
<?

}

//Zobrazí obrázky do galerie
function ShowImagesToPage($image_data, $images_count, $image_position, $admin_tray = true)
{
  global $_inf;
  $admin_tray = $admin_tray && IsLoged();
  
  $index = 1;
  $imagesInRow = IMAGES_IN_ROW - 1;
  
?>
          <h2>Galerie</h2>
          <div id="galery">
<?        while ($index <= $images_count)
          {
          if ($admin_tray)
          {?>

            <div class="admin_tools" style="float:left;">
              <?
              for ($x = $index, $i = 0; $x < $images_count && $i < $imagesInRow; $x++, $i++)
              {
                ToolButton("p=$_GET[p]&amp;a=galery&amp;f=delete&amp;i=".$image_data[$x]['id'], 'Smazat obrázek');
              }
                ToolButton("p=$_GET[p]&amp;a=galery&amp;f=delete&amp;i=".$image_data[$x]['id'], 'Smazat obrázek', "");?>

            </div>
            <div class="float_cleaner">&nbsp;</div>

<?        }
            for ($x = $index, $i = 0; $x <= $images_count && $i < IMAGES_IN_ROW; $x++, $image_position++, $i++)
            {?>
             <a href="galery/<?=$image_data[$x]['adr']?>.jpg" class="galery_image" onclick="return ShowGalery(<?=$image_position?>);" style="margin-right:<?=$image_data[$x]['image_margin']?>px"><img src="galery/thumbs/<?=$image_data[$x]['adr']?>.jpg" alt="<?=$image_data[$x]['adr']?>.jpg" width="<?=$image_data[$x]['tmb_width']?>" height="<?=$image_data[$x]['tmb_height']?>" /></a>       
<?          }
            $index += IMAGES_IN_ROW;
          ?>
            <div class="float_cleaner">&nbsp;</div>
<?        }
          ?>
          </div>
<?
}

//Vypíše komentáře na stránku
function ShowCommentsToPage($comment_data, $comments_count, $admin_tray = true)
{
  global $_inf;
  $admin_tray = $admin_tray && IsLoged();
  
?>
          <div id="comments">
<?        for ($x = 1; $x <= $comments_count; $x++)
          {?>
            
            <div class="comment" id="comment<?=$comment_data[$x]['id']?>">
              <div class="item_date"><?=GetDatetime($comment_data[$x]['date'])?></div>
              <h3><strong><?=$comment_data[$x]['author']?>:</strong> <?=$comment_data[$x]['title']?></h3>
<?          if ($admin_tray)
            {?>
              <div class="admin_tools">
                <?

                ToolButton("p=$_GET[p]&amp;a=comments&amp;f=state&amp;i=".$comment_data[$x]['id']."&amp;s=$_inf[s]" , 'Způsob zobrazení', "");
?>

                <div class="float_cleaner">&nbsp;</div>
              </div>
              <div class="float_cleaner">&nbsp;</div>
<?          }?>
              <?=$comment_data[$x]['text']?>
              
<?          if ($comment_data[$x]['opinion'] != "")
            {?>
              <div class="opinion">
                <div class="item_date"><?=GetDatetime($comment_data[$x]['o_date'])?></div>
                <h4>Vyjádření Building For Life:</h4>
                <div class="op_text">
                  <p><?=$comment_data[$x]['opinion']?></p>
                </div>
              </div>
<?          }?>
            </div>
<?        }?>
          </div>
<?

}

//Zobrazí tabulku stránek
function ShowPagesToPage($page_data, $page_count, $page_position, $admin_tray = true)
{
  global $_inf;
  $admin_tray = $admin_tray && IsLoged();
  $move = $_GET['f'] == 'move';
  
?>
          <div id="rubric_menu">
<?        for ($x = 1; $x <= $page_count; $x++, $image_position++)
          {?>
            
            <div class="rubric_menu_item" id="item<?=$page_data[$x]['id']?>">
              <a href="index.php?p=<?=$page_data[$x]['adr']?>" class="rubric_link">
                <div class="rubric_image">
                  <div class="image_top"></div>
                  <div class="image_body">
                    <div class="image_crop"><img src="<?=$page_data[$x]['img']?>" alt="Obrázek článku" /></div>
                  </div>
                  <div class="image_bottom"></div>
                </div>
                <div class="item_date"><?=GetDatetime($page_data[$x]['date'])?></div>
                <h3><?=$page_data[$x]['title']?></h3>
<?          if ($admin_tray)
            {?>
              </a>
              <div class="admin_tools">
                <?
              if ($move && $data['pos'] != $_inf['i'])
                echo "<input name=\"col[2]\" value=\"".$page_data[$x]['pos']."\" type=\"submit\" title=\"Přesunout na toto místo\" />";
              else
              {
                ToolButton("p=$_GET[p]&amp;a=menu&amp;f=add&amp;i=".$page_data[$x]['pos']."&amp;s=$_inf[s]" , 'Přidat stránku na toto místo');
                ToolButton("p=$_GET[p]&amp;a=menu&amp;f=move&amp;i=".$page_data[$x]['pos']."&amp;s=$_inf[s]#item".$page_data[$x]['id'] , 'Přesunout v menu');
                
                if (!$page_data['lck'])
                {
                  ToolButton("p=".$page_data[$x]['adr']."&amp;a=".$page_data[$x]['script']."&amp;f=properties&amp;s=$_inf[s]&amp;b=$_GET[p]" , 'Upravit stránku');
                  ToolButton("p=".$page_data[$x]['adr']."&amp;a=".$page_data[$x]['script']."&amp;f=delete&amp;s=$_inf[s]&amp;b=$_GET[p]" , 'Smazat stránku', "");
                }
              }
?>

              </div>
              
              <a href="index.php?p=<?=$page_data[$x]['adr']?>" class="rubric_link">
<?          }?>
                <p><?=$page_data[$x]['text']?></p>
                <div class="float_cleaner"></div>
              </a>
            </div>
<?        }?>
          </div>
<?

}

//Zobrazí tabulku administrátorů
function ShowUsersToPage($sql)
{
?>
          <table class="list" align="center">
            <tr>
              <th width="40">ID</th>
              <th width="200">Přihlašovací jméno</th>
              <th width="200">E-mail</th>
              <th width="140">Registrován</th>
              <th width="140">Poslední přihlášení</th>
              <th width="110">Akce</th>
            </tr>
<?        while ($data = mysql_fetch_array($sql))
          {?>
            <tr>
              <td><?=$data['id']?></td>
              <td><?=$data['name']?></td>
              <td><?=($data['mail'] ? $data['mail'] : '-neuvedeno-')?></td>
              <td><?=GetDatetime($data['register'])?></td>
              <td><?=( ($t = GetDatetime($data['last_login'])) ? $t : "-nepřihlášen-" )?></td>
              <td><?ToolButton("p=$_GET[p]&amp;a=user&amp;f=delete&amp;i=$data[id]", 'Smazat uživatele', "")?></td>
            </tr>
<?        }?>          
          
          </table>
<?
}



//Vypíše komentáře na stránku
function ShowImagesListToPage($image_data, $image_count)
{
  global $_inf;
  
?>
          <div id="rubric_menu">
<?        for ($x = 1; $x <= $image_count; $x++)
          {?>
            
            <div class="rubric_image_item" id="image<?=$image_data[$x]['id']?>">
              <div class="item_date"><?=$image_data[$x]['date']?></div>
              <h3>galery/<?=$image_data[$x]['adr']?>.jpg</h3>
              <img src="galery/thumbs/<?=$image_data[$x]['adr']?>.jpg" alt="Obrázek" />
              <div class="admin_tools">
                <?

                ToolButton("p=$_GET[p]&amp;a=image&amp;f=delete&amp;i=".$image_data[$x]['id']."&amp;s=$_inf[s]" , 'Smazat obrázek', "");
?>
              </div>
              <div class="float_cleaner">&nbsp;</div>
            </div>
<?        }?>
          </div>
<?

}

