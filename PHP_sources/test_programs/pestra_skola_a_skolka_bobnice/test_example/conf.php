<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
**************************************  Konfigurační soubor  ***************************************

 * Script obsahuje definice symbolických konstant pro nastavení chování stránek

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

  //Klic pouzity pro sifrovani e-mailu
  define("MASTER_CODE","yBhER344XC1f0fhsjn7844fhhs6GF");
  //Spolecna sul pridavana k heslum
  define("SALT","Jg4hE");
  //Zamek administratorskeho pristupu
  define("LCK",1);
  //Pocet tlacitek pro strankovani
  define("SW_SIZE", 5);
  //Pocet prvku v hlavnim menu 
  define("MAIN_MENU_ITEMS", 5);
  //Pocet prvku v menu druhe urovne
  define("MENU_ITEMS", 10);
  //Pocet sloupcu obsahovych tabulek
  define("TABLE_ITEMS", 4);
  
  //Limity prvku pro strankovani
  define("IMAGE_LIMIT", 20);
  define("ALBUM_LIMIT", 20);
  define("PAGE_LIMIT", 20);
  define("USERS_LIMIT", 20);
  
  //Doba mezi zapocitanim vizitu navstevnika pri neexistujicim cookie
  define("VISIT_INTERVAL", 30 * 60);
    
  //Nastaveni velikosti nahledu a obrazku v galerii
  define("IMG_WIDTH",   1000);
  define("IMG_HEIGHT",  1000);
  define("THUMB_WIDTH",  200);
  define("THUMB_HEIGHT", 250);
  define("ADD_IMAGES_CT",  5);
  
  //Nastaveni pripojeni k databazi
  define("SQL_HOST","localhost");
  define("SQL_DBNAME","zs_bobnice");
  define("SQL_USERNAME","root");
  define("SQL_PASSWORD","12345");
  define("SQL_NAMES","utf8");
  define("SQL_ERROR_LOG",1);
  
  //ID vyznamnych stranek - tyto zaznamy musi korespondovat s ID prislusnych polozek v databazi
  define("UVOD_ID",1);
  define("ADMN_ID",2);
  define("MENU_ID",3);
  define("PROF_ID",4);

?>