<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
*************************************  Správa administrátorů  **************************************

 * Všichni registrovaní uživatelé jsou administrátoři a kromě zamčených úseků mohou měnit veškerý
 * obsah stránek.

MYSQL tabulka pro uložení uživatelů:
 * ID, přihlašovací jméno, heslo, e-mail, datum registrace, datum posledniho přihlášení, stav uživatele

CREATE TABLE IF NOT EXISTS `users` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(25) COLLATE utf8_czech_ci NOT NULL,
  `no1` varchar(5) COLLATE utf8_czech_ci NOT NULL,
  `no2` varchar(40) COLLATE utf8_czech_ci NOT NULL,
  `mail` blob NOT NULL,
  `register` datetime NOT NULL,
  `last_login` datetime NOT NULL DEFAULT '0000-00-00 00:00:00',
  `state` tinyint(4) NOT NULL DEFAULT '2',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM  DEFAULT CHARSET=utf8 COLLATE=utf8_czech_ci AUTO_INCREMENT=1 ;

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

//Vyvolání funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  switch ($_GET['f'])
  {
    case 'add':     AddUser();    break;
    case 'delete':  DeleteUser();  break;
  }
}

//Zobrazí formulář na stránce
$_inf['form'] = true;
function Form()
{ 
  //Obsah stránky
  switch ($_GET['f'])
  {
    case 'add':     AddUserForm();    break;
    case 'delete':  DeleteUserForm();  break;
    default: return false;
  }
  return true;
}

//Zobrazení stránky
function Body()
{
  global $_page;

  echo "<div class=\"toolbox\">";
  ToolButton("p=$_GET[p]", 'Zobrazit výpis administrátorů', 'page', false);
  ToolButton("p=$_GET[p]&amp;a=user&amp;f=add", 'Přidat administrátora', 'plus');
  echo "</div><div class=\"float_cl\">&nbsp;</div>";
    
  if (Form()) return;
  echo "\n<div id=\"ajax_target\" >";
  ShowUsers();
  echo "\n</div>";
}

//Zpracování AJAX požadavku
$_inf['ajax'] = true;
function Ajax()
{
  ShowUsers();
}

//Zobrazení výpisu administrátorů
function ShowUsers()
{
  global $_page; global $_inf;
  $sql = sql_select('users',"id, name, register, last_login, state ,AES_DECRYPT(mail, sha(concat('".MASTER_CODE."' , no1))) as mail",
    "where state > 0 order by id"
    , (USERS_LIMIT * ($_inf['s'] - 1) ) .",". USERS_LIMIT
  );
  
  $data = mysql_fetch_array( sql_select('users','count(*) as ct',
    "where state > 0", 1
  ));
  $count = $data ? $data['ct'] : 0;
  
  echo "\n<table class=\"list\" align=\"center\"><tr><td colspan=\"6\" class=\"stat\" >";
  if (ShowSwitcher(mysql_num_rows($sql), $count, USERS_LIMIT) )
  {
    echo "</td></tr><tr><th width=\"40px\">ID</th><th width=\"200\">Přihlašovací jméno</th><th width=\"200\">E-mail</th><th width=\"140\">Registrován</th><th width=\"140\">Poslední přihlášení</th><th width=\"30px\">&nbsp;</th></tr>";
    while ($data = mysql_fetch_array($sql))
    {
      $reg = GetDatetime($data['register']);
      $last = GetDatetime($data['last_login']);
      $mail = $data['mail'] ? $data['mail'] : '-neuvedeno-';
      if (!$last) $last = "nepřihlášen";
      
      echo "<tr><td>$data[id]</td><td>$data[name]</td><td>$mail</td><td>$reg</td><td>$last</td>";
      echo "<th><div class=\"toolbox\">";
      ToolButton("p=$_GET[p]&amp;a=user&amp;f=delete&amp;i=$data[id]", 'Smazat uživatele', 'delete');
      echo "</div></th>"; 
      echo "</tr>";
    }
  }
  echo "</table>";
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

function AddUser()
{
  if ( LenghtTest(1, $name = GetCollum(1) , 3, 20)) return;
  if ( LenghtTest(2, $pass = GetCollum(2) , 5, 20)) return;
  
  $data = mysql_fetch_array( sql_select('users', 'id', "where name = '$name' and state != 0") );
  if ($data){ ThrowFormError(1, "Uživatel tohoto jména již existuje"); return; }
  
  $salt = RandomString(5);
  
  sql_insert('users','name, no1, no2, register',
    "('$name', '$salt', sha1( concat('". SALT ."$pass', no1) ), '". date("y-m-d H:i:s") ."')");
  MakeRefresh();
}
function DeleteUser()
{
  $id = GetCollum(1);
  if (GetCollum(2))
  {
    $data = mysql_fetch_array( sql_select('users', 'id, state', "where state > 0 and id = '$id'", 1));
    if (!$data){ ThrowFormError(0, "Uživatel neexistuje"); return; }
    sql_update('users', "state = '0'", "state > 0 and id = '$id'", 1);
    MakeRefresh();
  }
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////

function AddUserForm()
{
  global $_inf;
  if (IsRefreshed("Uživatel byl přidán")) return;
  
  FormHead("Přidat uživatele");
  TextBox("Přihlašovací jméno", 1, '', 3, 20);
  TextBox("Heslo pro první přihlášení", 2, RandomString(6), 5, 20);
  FormBottom('', "'txt', 'txt'", "3,5", "20,20");
}
function DeleteUserForm()
{
  global $_inf; global $_page;
  if (IsRefreshed("Uživatel byl smazán")) return;

  $data = mysql_fetch_array( sql_select('users',"id, state, name",
    "where id = '$_inf[i]' and state != 0",1
  ));
  if(!$data){ Alert("Tento uživatel neexistuje"); return; }
  if($data['state'] == -1){ Alert("Tento uživatel nemůže být odstraněn"); return; }
  
  FormHead("Smazat uživatele <u>$data[name]</u>");
  HiddenBox2(1, $data['id']);
  YNRadio("Opravdu chcete odstranit uživatele <u>$data[name]</u>?", 2, 0);
  FormBottom('');
}
?>