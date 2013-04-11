<?
/***************************************************************************************************
****************************  Vyvolávací stránka pro zobrazení formulářů  **************************

 * Script je na stránkách zobrazován v tagu IFRAME - ten je vyvoláván v administračním módu pro
 * zobrazení formulářů - zrychlení načítání (neprovádí se takové množství dotazů na databázi)
 * Po zavolání provede iniciaci a vložení ostatních scriptů, zpracování postů a zobrazení
 * formuláře, nebo hlášky

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

  //zámek scriptů
  define("AUTORIZACE","Q.&dJhůs63d5dS=d56sLc5%");
  $_inf['page'] = "form.php";
  
  session_start();
  require_once "conf.php";
  require_once "_php/_definitions.php";
  require_once "_php/_database.php";
  require_once "_php/_forms.php";
  require_once "_php/_pages.php";
  
  $_page = GetPageData();
  require_once "_php/$_page[script].php";
  
  
  if (!empty($_POST) and $_inf['post'])
  {
    Post();
  }
  
  echo "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">";
  echo "\n<html><head>";
  echo "\n  <meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\">";
  echo "\n  <title>$_page[ndp] - Pestřá základní škola Bobnice :-))</title>";
  echo "\n  <script src=\"_javascript/main.js\"></script>";
  echo "\n  <link rel=\"stylesheet\" type=\"text/css\" href=\"_css/main.css\">";
  if($_inf['refresh'])
  echo "\n<script>setTimeout( \"MakeRefresh('$_inf[refresh]',$_inf[refrresh_mode])\", 5000);</script>";
  echo "\n</head><body class=\"formWindow ". GetColor($_page["color"]) ."\"><div id=\"form_div\"><div class=\"float_cl\">&nbsp;</div>";
  
  if (!$_inf['form'] or !Form())
    Alert("Tento formulář se na stránkách nenachází");
  

  
  echo "\n<div class=\"float_cl\">&nbsp;</div></div>\n<script> FormInitIframe('$_GET[wnd]'); RefreshIframeSize(); </script>\n</body></html>";
?>