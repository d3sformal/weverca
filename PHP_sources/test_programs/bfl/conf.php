<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
**************************************  Konfigurační soubor  ***************************************

 * Script obsahuje definice symbolických konstant pro nastavení chování stránek

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

  //Klic pouzity pro sifrovani e-mailu
  define("MASTER_CODE","yBhER344XC1f0fhsjn7844fhhs6GF");
  //Spolecna sul pridavana k heslum
  define("SALT","Jg4hE");
  //Zamek administratorskeho pristupu
  define("LCK",1);
  //Pocet tlacitek pro strankovani
  define("SW_SIZE", 3);
  //Pocet prvku v hlavnim menu 
  define("MAIN_MENU_ITEMS", 5);
  //Pocet prvku v menu druhe urovne
  define("MENU_ITEMS", 10);
  //Pocet sloupcu obsahovych tabulek
  define("TABLE_ITEMS", 4);
  //interval presmerovani po vyplneni formulare (v sekundach)
  define("REFRESH_TIME", 3);
  
  //Limity prvku pro strankovani
  define("IMAGE_LIMIT", 20);
  define("PAGE_LIMIT", 5);
  define("USERS_LIMIT", 20);
  define("NEW_PAGES_LIMIT", 5);
  define("COMMENTS_LIMIT", 10);
  
  //Doba mezi zapocitanim vizitu navstevnika pri neexistujicim cookie
  define("VISIT_INTERVAL", 30 * 60);
    
  //Nastaveni velikosti nahledu a obrazku v galerii
  define("IMG_WIDTH",   1000);
  define("IMG_HEIGHT",  1000);
  define("IMAGES_IN_ROW", 5);             //Obrázků na řádek galerie
  define("GALERY_WIDTH", 890);            //Šířka celé galerie
  define("IMAGE_MARGIN", 5);              //MInimální odsazení mezi obrázky
  define("IMAGE_MARGIN_LIMIT", 100);      //Maximální odsazení mezi obrázky
  define("THUMB_WIDTH",  100);
  define("THUMB_HEIGHT", 150);
  
  //Maximální rozměry obrázku pro náhled alba
  define("PAGE_IMG_WIDTH",   100);
  define("PAGE_IMG_HEIGHT",  100);
  
  //Počet políček v přidávacím formuláři
  define("ADD_IMAGES_CT",  5);
  
  //Nastaveni pripojeni k databazi
  if (true)
  {
    define("SQL_HOST","localhost");
    define("SQL_DBNAME","bfl");
    define("SQL_USERNAME","pavel");
    define("SQL_PASSWORD","12345");
  }
  else
  {
    define("SQL_HOST","localhost");
    define("SQL_DBNAME","buildingforlife_cz");
    define("SQL_USERNAME","buildingforlife.cz");
    define("SQL_PASSWORD","hotmuse11");
  }
  define("SQL_NAMES","utf8");
  define("SQL_ERROR_LOG",1);
  
  //ID vyznamnych stranek - tyto zaznamy musi korespondovat s ID prislusnych polozek v databazi
  define("UVOD_ID",1);
  define("ADMN_ID",2);
  define("MENU_ID",3);
  define("PROF_ID",4);

?>