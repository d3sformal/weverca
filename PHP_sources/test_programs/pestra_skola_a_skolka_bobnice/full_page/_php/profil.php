<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
************************  Přihlášení a správa vlastního uživatelského účtu  ************************

 * Pracuje s MYSQL tabulkou USERS
 * Realizuje přihlášení a odhlášení uživatelů
 * Po přihlášení je zde možno změnit přihlašovací údaje 

****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/

//Odhlášení
if ($_GET['o'] == 'logout') Logout();
      
//vyvolní funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  if (IsLoged())
  {
    Profil();
  }
  else
  {
    Login();
  }
}

//Zobrazí formulář na stránce
$_inf['form'] = true;
function Form()
{
  global $_inf;
  if ($_GET['o'] == 'logout' and IsRefreshed("Byl jste odhlášen")) return true;
  if (IsLoged() and $_inf['log'] != 'login')
  {
    ProfilForm();
  }
  else
  {
    LoginForm();
  }
  return true;
}

//Zobrazení stránky
function Body()
{
  global $_page;

  Form();
}

//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

//Přihlášení do systému
function Login()
{
  global $_inf;
  if ( LenghtTest(1, $name = GetCollum(1) , 3, 20)) return;
  if ( LenghtTest(2, $pass = GetCollum(2) , 5, 20, false) ) return;
  
  $data = mysql_fetch_array( sql_select('users','id, name, last_login, state',
    "where state != 0 and name = '$name' and no2 = sha1( concat('". SALT ."$pass', no1) )", 1
  ));
  
  if ($data)
  {
    session_register("id"); $_SESSION['id'] = $data['id'];
    session_register("name"); $_SESSION['name'] = $data['name'];
    session_register("state"); $_SESSION['state'] = $data['state'];
    
    sql_update('users', "last_login = '". date("y-m-d H:i:s") ."'", "state != 0 and name = '$name'");
    
    if ($data['state'] == 2) MakeRefresh("$_inf[page]?$_SERVER[QUERY_STRING]", 1);
    else MakeRefresh("$_inf[page]?". preg_replace("/o=login&amp;?/", "", $_SERVER['QUERY_STRING']));
    
    $_inf['log'] = 'login';
  }
  else ThrowFormError(0, "Chybné uživatelské jméno nebo heslo"); 
}

//Změna přihlašovacích údajů
function Profil()
{
  if ( LenghtTest(1, $oldPas = GetCollum(1) , 5, 20, false) ) return;
  if ( LenghtTest(2, $name = GetCollum(2) , 3, 20)) return;
  if ( LenghtTest(3, $pass = GetCollum(3) , 5, 20, false)) return;
  if (GetCollum(4) != $pass) {ThrowFormError(4, "Heslo a opakované heslo se musí shodovat"); return;}
  if ( MailTest(5, $mail = GetCollum(5) , false)) return;
  
  //Ověření hesla
  $data = mysql_fetch_array( sql_select('users',"state, id, name, AES_DECRYPT(mail, sha1(concat('".MASTER_CODE."' , no1))) as mail",
    "where state != 0 and id = '$_SESSION[id]' and no2 = sha1( concat('". SALT ."$oldPas', no1) )", 1
  ));
  if(!$data){ ThrowFormError(1, "Přístup zamítnut - chybné heslo"); return; }
  
  //Změna jména
  $dotaz = "";
  if ($name != $data['name'])
  {
    $dotaz = "name = '$name'";
    $_SESSION['name'] = $name;
  }
  //Změna hesla - jen pokud je zadáno
  if ($pass)
  {
    if ($dotaz) $dotaz .= ", ";
    $dotaz .= "no2 = sha1( concat('". SALT ."$pass', no1) )";
    if ($data['state'] == 2)
    {
      $dotaz .= ", state = '1'";
      $_SESSION['state'] = 1;
    }
  }
  //Změna e-mailu
  if ($mail != $data['mail'])
  {
    if ($dotaz) $dotaz .= ", ";
    if ($mail) $dotaz .= "mail = AES_ENCRYPT('$mail', sha1(concat('".MASTER_CODE."' , no1)) )";
    else $dotaz .= "mail = ''";
  }

  if (!$dotaz){ ThrowFormError(0, "Neprovedl jste žádnou změnu"); return; }
  
  sql_update('users', $dotaz, "state != 0 and id = '$_SESSION[id]'", 1);
  MakeRefresh();
}
//Odhlášení
function Logout()
{
  session_unset();
  MakeRefresh("index.php?". GetLink(false) );
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////

function LoginForm()
{
  if (IsRefreshed("Byl jste přihlášen")) return;
  FormHead("Přihlášení", "$_inf[page]?$_SERVER[QUERY_STRING]");
  TextBox("Přihlašovací jméno", 1, '', 3, 20);
  PasswordBox("Heslo", 2, '', 5, 20);
  FormBottom('', "'txt', 'txt'", "3,5", "20,20");
}
function ProfilForm()
{
  $data = mysql_fetch_array( sql_select('users',"id, name, AES_DECRYPT(mail, sha1(concat('".MASTER_CODE."' , no1))) as mail",
    "where state != 0 and id = '$_SESSION[id]'", 1
  ));
  if(!$data){ Alert("Chyba, nelze nalézt uživatele"); return; }
  
  if (IsRefreshed("Údaje změněny")) return;
  FormHead("Změna přihlašovacích údajů", "$_inf[page]?$_SERVER[QUERY_STRING]");
  if ($_SESSION['state'] == 2) Alert("Změňte si prosím heslo");
  PasswordBox("Původní heslo", 1, '', 5, 20);
  TextBox("Přihlašovací jméno", 2, $data['name'], 3, 20);
  PasswordBox("Nové heslo", 3, '', 5, 20, false);
  PasswordBox("Potvrzení hesla", 4, '');
  TextBox("E-mailová adresa", 5, $data['mail']);
  FormBottom("Pokud ponecháte pole 'Nové heslo' nevyplněné, nedojde ke změně hesla.",
    "'txt','txt', 'not', 'psck', 'mail'", "5,3,5,0,0", "20,20,20,0,0"
  );
}


?>