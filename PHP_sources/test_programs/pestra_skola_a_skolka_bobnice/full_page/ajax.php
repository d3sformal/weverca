<?
/***************************************************************************************************
************************  Vyvolávací stránka AJAX po žadavku pro stránkování  **********************

 * Navrácená data jsou zobrazována v elementu s ID 'ajax_target'
 * Po zavolání provede iniciaci, vložení ostatních scriptů a zavolá metodu Ajax(), která navrátí
 * požadovaná data

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

  //zámek scriptů
  define("AUTORIZACE","Q.&dJhůs63d5dS=d56sLc5%");
  $_inf['page'] = "index.php";
  
  session_start();
  require_once "conf.php";
  require_once "_php/_definitions.php";
  require_once "_php/_database.php";
  require_once "_php/_forms.php";
  require_once "_php/_pages.php";
  
  $_page = GetPageData();
  require_once "_php/$_page[script].php";
  
  
  if ($_inf['ajax'])
  {
    Ajax();
  }
?>