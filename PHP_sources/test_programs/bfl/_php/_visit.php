<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
*******************************  Funkce pro obsluhu statistiky návštěv  ****************************

MYSQL TABULKA VISITORS
 * Slouží k ukládání informací o návštěvníkovi - ID, IP, verze prohlížeče, banovací informace  

  CREATE TABLE IF NOT EXISTS `visitors` (
    `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
    `ip` varchar(25) COLLATE utf8_czech_ci NOT NULL DEFAULT '0.0.0.0',
    `agent` text COLLATE utf8_czech_ci NOT NULL,
    `zob` tinyint(3) unsigned NOT NULL DEFAULT '1',
    PRIMARY KEY (`id`)
  ) ENGINE=MyISAM  DEFAULT CHARSET=utf8 COLLATE=utf8_czech_ci AUTO_INCREMENT=1 ;


MYSQL TABULKA VISITS
 * ukládání každé návštěvy návštěvníka
 * ID, ID návštěvníka, ID stránky, datum, příznak prní návštěvy, příznak cookie 
 
  CREATE TABLE IF NOT EXISTS `visits` (
    `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
    `vid` int(10) unsigned NOT NULL,
    `uid` smallint(5) unsigned NOT NULL DEFAULT '0',
    `pid` int(10) unsigned NOT NULL,
    `datum` datetime NOT NULL,
    `visit` tinyint(3) unsigned NOT NULL DEFAULT '0',
    `cookie` tinyint(3) unsigned NOT NULL,
    PRIMARY KEY (`id`)
  ) ENGINE=MyISAM  DEFAULT CHARSET=utf8 COLLATE=utf8_czech_ci AUTO_INCREMENT=1 ;

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

  //Funkce navrací vnitřní ID uživatele podle IP adresy a klienta
  //Pokud není uživatel v databázi, je zanesen
  function GetVisitor()
  {
    $ip = GetClientIP(); $visitor = 0;
    
    if(isset($_SESSION['vid']) or $_SESSION['vid'])
      $visitor = $_SESSION['vid'];
    else
    {
      //Vyhledání uživatele
      $data = mysql_fetch_array( sql_select('visitors','id',
        "where ip = '$ip' and agent = '$_SERVER[HTTP_USER_AGENT]'"
      ));
      
      if ($data)
      {
        $visitor = $data['id'];
        session_register("vid"); $_SESSION['vid'] = $data['id'];
      }
      else
      {
        //Nový uživatel
        $agent = addslashes($_SERVER['HTTP_USER_AGENT']);
        sql_insert('visitors','ip,agent',"('$ip','$agent')");
        $data = mysql_fetch_array( sql_select('visitors','id',
          "where ip = '$ip' and agent = '$_SERVER[HTTP_USER_AGENT]'"
        ));
        
        $visitor = $data ? $data['id'] : 0;
      }
    }
    return $visitor;
  }
  
  //Funkce vytvoří nový visit pro uživatele $visitor pro statistiku návštěv
  /* Do databéze jsou uloženy i parametry
   *    $uid        - ID přihlášeného uživatele
   *    $type       - druh dotazu
   *    $visitParam - parametr dotazu (pro anketní otázky)
   */   
  function MakeVisit($visitor, $pid = 0, $uid = 0)
  {
    $newVisit = 0; $cookie = 1;
    //Neexistující COOKIE návštěvy
    if(empty($_COOKIE['visit']))               //Neexistující COOKIE návštěvy
    {
      //Test půlhodinového intervalu návštěvy
      $data = mysql_fetch_array( sql_select('visits','id',
        "where vid = '$visitor' and datum > '". date("Y-m-d H:i:s",time() - VISIT_INTERVAL) ."' and visit = 1",1
      ));
      
      if (!$data)                                //Nový visit
      {
        $newVisit = 1;
        setcookie('visit', $visitor, mktime(23,59,59, date('m'), date('d'), date('Y')));
      }
      $cookie = 0;
    }
    
    //Vložení záznamu návštěvy
    sql_insert('visits','vid, uid, pid, datum, visit, cookie',
      "('$visitor', '$uid', '$pid', '". date("y-m-d H:i:s") ."', '$newVisit', '$cookie')"
    );
    
  }
  
  //Navrací IP klienta
  function GetClientIP()
  {
    return    !empty($_SERVER['HTTP_CLIENT_IP'])        ? $_SERVER['HTTP_CLIENT_IP'] : 
            ( !empty($_SERVER['HTTP_X_FORWARDED_FOR'])  ? $_SERVER['HTTP_X_FORWARDED_FOR'] :
                                                          $_SERVER['REMOTE_ADDR'])
    ;
  }
  


?>
