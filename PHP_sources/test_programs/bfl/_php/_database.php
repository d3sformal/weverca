<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
         Pro stránky buildingforlife.cz napsal Pavel Baštecký (c) 2010 - anebril<a>seznam.cz
***************************************************************************************************/

/***************připojení k databázi*******************************************/

  //připojení k databázi a nastavení znakové sady pro komunikaci
  mysql_connect(SQL_HOST, SQL_USERNAME, SQL_PASSWORD) or die("<h2>Nelze se připojit k MySQL!</h2><br>".mysql_error());
  mysql_select_db(SQL_DBNAME) or die("<h2>Nelze vybrat databázi!</h2><br>".mysql_error());
  mysql_query("SET NAMES '".SQL_NAMES."';");
/***************připojení k databázi*******************************************/

/*
FUNKCE PRO PRÁCI S DATABÁZÍ:
Vstupní hodnoty:
  $tab - tabulky
  $cols - řádky
  $values - hodnoty
  $limit - limit řádků
  $where - omezení
  $moznosti - další volby SQL
*/
/******************************************************************************/


//vybírání libovolného řádků
  function sql_select($tab,$cols,$moznosti,$limit = 0){
    $dotaz="
SELECT $cols
FROM $tab
$moznosti 
".($limit ? "LIMIT $limit" : "");
    //echo "\n<br>$dotaz";
    
    $data=mysql_query($dotaz) or die("<h2>Chyba databáze - NELZE VYBRAT DATA</h2><br>".(SQL_ERROR_LOG ? mysql_error()."<br>$dotaz" : ""));
    return $data;
  };


//vkládání  
  function sql_insert($tab,$cols,$values){
    $dotaz="
INSERT INTO $tab ($cols)
VALUES $values";
    //echo "\n<br>$dotaz";
    
    $data=mysql_query($dotaz) or die("<h2>Chyba databáze - NELZE VLOŽIT DATA</h2><br>".(SQL_ERROR_LOG ? mysql_error()."<br>$dotaz" : ""));
    return $data;
  };


//upravování dat s omezením na 1 řádek
  function sql_update($tab,$values,$where = 0,$limit = 0){
    $dotaz="
UPDATE $tab 
SET $values
".($where ? "WHERE $where" : "")."
".($limit ? "LIMIT $limit" : "");
    //echo "\n<br>$dotaz";
    
    $data=mysql_query($dotaz) or die("<h2>Chyba databáze - NELZE ZMĚNIT DATA</h2><br>".(SQL_ERROR_LOG ? mysql_error()."<br>$dotaz" : ""));
    return $data;
  };


//mazání dat s omezením na 1 řádek  
  function sql_delete($tab,$where,$limit = 0){
    $dotaz="
DELETE FROM $tab
WHERE $where
".($limit ? "LIMIT $limit" : "");
    //echo "\n<br>$dotaz";
    
    $data=mysql_query($dotaz) or die("<h2>Chyba databáze - NELZE SMAZAT DATA</h2><br>".(SQL_ERROR_LOG ? mysql_error()."<br>$dotaz" : ""));
    return $data;
  }
?>
