<?
/***************************************************************************************************
**************************  Vyvolávací stránka pro web BUILDING FOR LIFE  **************************

 * Script je spuštěn pri zadání adresy webu
 * Provede počáteční iniciaci, zavolá ostatní scripty, vyvolá zpracování postů a zaznamená přístup
 * Nakonec vykreslí stránku

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

  //zámek scriptů
  define("AUTORIZACE","Q.&dJhůs63d5dS=d56sLc5%");
  
  $_inf['page'] = "index.php";
  
  session_start();
  require_once "conf.php";
  require_once "_php/_definitions.php";
  require_once "_php/_database.php";
  require_once "_php/_visit.php";
  require_once "_php/_pages.php";
  
  if ($_inf['f']) require_once "_php/_forms.php";
  
  $_inf['visitor'] = GetVisitor();
  $_page = GetPageData();
  
  $_page['script'] = "rubric";
  require_once "_php/$_page[script].php";
  
  //Zpracování formulářů
  if (!empty($_POST) and $_inf['post'])
  {
    Post();
  }
  
  MakeVisit($_inf['visitor'], $_page['id'], isset($_SESSION['id']) ? $_SESSION['id'] : 0);

  //if($_inf['refresh']) header("Refresh: ".REFRESH_TIME."; $_inf[refresh]");
  
  
  ShowPage();
  
?>