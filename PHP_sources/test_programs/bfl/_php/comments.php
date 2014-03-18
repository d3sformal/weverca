<? if(AUTORIZACE != "Q.&dJhůs63d5dS=d56sLc5%" or !AUTORIZACE)die("<h1>Chyba vstupu</h1>");

/***************************************************************************************************
************************************  Zobrazení rubrik stránek  ************************************

 * Rubriky jsou v databázi uloženy v tabulce PAGES - hodnota script je nastavena na 'rubric'
 * Každá rubrika může mít jednu galerii - rubrika, která již jednu má, má nastavenu hodnotu img na 1
 * Rubrika je uložena v hlavním menu, rubrika nemůže obsahovat jinou rubriku
 * Všechny podřízené stránky mají hodnotu PID nastavenu na ID mateřské rubriky a jejich barevné schéma
 * je odvozeno od její pozice v menu.  

****************************************************************************************************
                             Pavel Baštecký anebril(a)seznam.cz (c) 2012
***************************************************************************************************/

if (!$_inf['f']) require_once "_php/_forms.php";

//Vyvolá funkce pro zpracování formulářů
$_inf['post'] = true;
function Post()
{
  global $_inf;
  
  if ($_inf['f'] == ADD_FORM)
  {
    AddComment();
    return;
  }
  
  if (LCK && !IsLoged()) return;
  
  switch ($_inf['f'])
  {
    case PROPERTIES_FORM:  PropertiesPost();  break;
    case STATE_FORM: StateComment(); break;
  }
}

//Zobrazí formulář na stránce
$_inf['form'] = true;
function Form()
{
   global $_inf;  
   
   if ($_inf['f'] == ADD_FORM)
  {
    AddCommentForm();
    return true;
  }

  if (LCK && !IsLoged()) return false;

  switch ($_inf['f'])
  {
    case PROPERTIES_FORM:  PropertiesForm('rubriky');  break;
    case STATE_FORM: StateCommentForm(); break;
    default: return false;
  }
  return true;
}

$_inf['admin'] = true;
function PageAdministration()
{    
    ToolButton("p=$_GET[p]", 'Zobrazit komentáře');
    ToolButton("p=$_GET[p]&amp;a=rubric&amp;f=properties", 'Vlastnosti stránky', "");
}

//zapíše informace do hlavičky stránky pro vložení CSS a JS souborů
$_inf['header'] = true;
function PageHeader()
{
  global $_inf; global $_page;
  
  if (!$_inf['f']) ShowFormHeader();
  ShowCleditorHeader();
  ShowCommentsHeader();
}

//Zobrazení stránky
//Pokud nebude napsán žádný text, tak dojde k vyvolání menu
function Body()
{
  global $_page;

  if (Form()) return;

  echo "\n        <div id=\"ajax_target\">\n";
  $c = ShowComments();
  echo "\n        </div>\n";

  if ($c > 0) 
    echo "\n        <div class=\"comment_separator\"></div>";
  
  AddCommentForm();
}

//Zpracování AJAX požadavku
$_inf['ajax'] = true;
function Ajax()
{
  ShowComments();
}

//Zobrazení obsahu rubriky
function ShowComments()
{
  global $_page; global $_inf;
  $sql = sql_select('comments', 'id, vid, date, author, title, text, opinion, o_date, state',
    "where pid = '$_page[id]' and state > '0' order by date desc"
    , (COMMENTS_LIMIT * ($_inf['s'] - 1) ) .",". COMMENTS_LIMIT
  );
  
  $count = mysql_fetch_array(sql_select('comments', 'count(*) as c',
    "where pid = '$_page[id]' and state > '0'"
  ));
  
  for($x = 1; $data = mysql_fetch_array($sql); $x++)
  {
    $comments[$x]['id'] = $data['id'];
    $comments[$x]['date'] = $data['date'];
    $comments[$x]['author'] = $data['author'];
    $comments[$x]['title'] = $data['title'] ? $data['title'] : $data['author'];
    $comments[$x]['text'] = "<p>$data[text]</p>";
    $comments[$x]['opinion'] = $data['opinion'];
    $comments[$x]['o_date'] = $data['o_date'];
    
    if ($data['state'] == 2)
    {
      $comments[$x]['title'] = "<u>Skryto:</u> <span id=\"title_$data[id]\" style=\"display:none\">".$comments[$x]['title']."</span>";
      $comments[$x]['text'] = "<h5 id=\"error_$data[id]\"><a href=\"#\" onclick=\"return ShowComment($data[id]);\">Tato zpráva byla redakcí skryta - pro zobrazení klikněte na tento text</a></h5><div id=\"text_$data[id]\" style=\"display:none\">".$comments[$x]['text']."</div>";
    }
    else if ($data['state'] == 3)
    {
      $comments[$x]['author'] = "<u>Smazáno</u>";
      $comments[$x]['title'] = "<u>Smazáno</u>";
      $comments[$x]['text'] = "<h5>Tato zpráva byla redakcí označena jako nevhodná a smazána.</h5>";
    }
  }
  
  ShowCommentsToPage($comments, mysql_num_rows($sql));
  ShowSwitcher(mysql_num_rows($sql), $count['c'], COMMENTS_LIMIT);
  return $count['c'];
}

function FormatText($text)
{
  $reg[] = "/\<b\>(.+?)?\<\/b\>/si"; $nah[] = "[b]\\1[/b]";
  $reg[] = "/\<strong\>(.+?)?\<\/strong\>/si"; $nah[] = "[b]\\1[/b]";
  $reg[] = "/\<u\>(.+?)?\<\/u\>/si"; $nah[] = "[u]\\1[/u]";
  $reg[] = "/\<i\>(.+?)?\<\/i\>/si"; $nah[] = "[i]\\1[/i]";
  $reg[] = "/\<em\>(.+?)?\<\/em\>/si"; $nah[] = "[i]\\1[/i]";
  $reg[] = "/\<br *\/\>/si"; $nah[] = "[br]";
  $reg[] = "/&gt;/si"; $nah[] = ">";
  $reg[] = "/&lt;/si"; $nah[] = "<";
  
  $text = preg_replace($reg,$nah,$text);
  $text = htmlspecialchars($text);
  
  $reg2[] = "/\[b\](.+?)?\[\/b\]/si"; $nah2[] = "<strong>\\1</strong>";
  $reg2[] = "/\[u\](.+?)?\[\/u\]/si"; $nah2[] = "<u>\\1</u>";
  $reg2[] = "/\[i\](.+?)?\[\/i\]/si"; $nah2[] = "<em>\\1</em>";
  $reg2[] = "/\[br\]/si"; $nah2[] = "<br />";
  $text = preg_replace($reg2,$nah2,$text);
  return $text;
}


//////////////////////////////////////  Zpracování formulářů  //////////////////////////////////////

function DeleteComment()
{
  global $_inf; global $_page;
  $id = GetCollum(1);
  if (GetCollum(2))
  {
    $data = mysql_fetch_array( sql_select('comments',"id,date,title,author,text",
      "where pid = '$_page[id]' and state > 0 and id = '$_inf[i]'",1
    ));
    if (!$data){ ThrowFormError(0, "Nelze nalézt vybraný komentář"); return; }
    sql_update('comments',"state = 0","id = '$data[id]'");
    
    MakeRefresh();
  }

}

function StateComment()
{
  global $_inf; global $_page;
  $id = GetCollum(1);
  if ( NumberTest(2, $state = GetCollum(2) , 1, 3)) return;
  if ( LenghtTest(3, $text = GetTextCollum(3) , 0, 2000)) return;
  
  $data = mysql_fetch_array( sql_select('comments',"id,date,opinion,state",
    "where pid = '$_page[id]' and state > 0 and id = '$_inf[i]'",1
  ));
  if (!$data){ ThrowFormError(0, "Nelze nalézt vybraný komentář"); return; }
  
  $dotaz = "";
  if ($data['state'] != $state) $dotaz = "state = '$state'";
  if ($data['opinion'] != $text)
  {
    if ($dotaz != "") $dotaz .= ", ";
    $dotaz .= "opinion = '$text', o_date = '". date("y-m-d H:i:s") ."'";
  }
  
  if (!$dotaz) { ThrowFormError(0, "Neprovedl jste žádnou změnu"); return; }

  sql_update('comments',"$dotaz","id = '$data[id]'");
  MakeRefresh("index.php?p=$_GET[p]&amp;s=$_inf[s]#comment$_inf[i]");

}

function AddComment()
{
  global $_page;
  
  if ( LenghtTest(1, $name = GetCollum(1) , 3, 30)) return;
  if ( LenghtTest(2, $title = GetCollum(2) , 0, 100)) return;
  if ( LenghtTest(3, $text = FormatText(GetTextCollum(3)) , 1, 2000)) return;
  if ( NumberTest(4, $spam = FormatText(GetCollum(4)) , 0, 0)) return;
  
  if (!isset($_SESSION['antispam']) || $_SESSION['antispam'] != $spam)
  {
    ThrowFormError(4, "Antispamová kontrola - zadejte prosím součet čísel v popisku");
    return false;
  }
  
  $vid = $_SESSION['vid'];  
  
  sql_insert('comments','pid, vid, date, author, title, text, state',
    "('$_page[id]' ,'$vid', '". date("y-m-d H:i:s") ."', '$name', '$title', '$text', '1')"
  );
  MakeRefresh("index.php?p=$_GET[p]");
}

/////////////////////////////////////// Formuláře na stránce ///////////////////////////////////////

function GetNumberText($x)
{
  switch ($x)
  {
    case 1: return "Jedna";
    case 2: return "Dva";
    case 3: return "Tři";
    case 4: return "Čtyři";
    case 5: return "Pět";
    case 6: return "Šest";
    case 7: return "Sedm";
    case 8: return "Osm";
    case 9: return "Devět";
  }
}

function AddCommentForm()
{  
  $a = rand(1,9);
  $b = rand(1,9);
  session_register("antispam"); $_SESSION['antispam'] = $a + $b;
  
  $aa = "<strong>". GetNumberText($a) ."</strong>";
  $bb = "<strong>". GetNumberText($b) ."</strong>";
  
  if (IsRefreshed("Komentář byl přidán")) return true;
  FormHead("Přidat komentář", "index.php?p=$_GET[p]&amp;f=add");
  TextBox("Jméno", 1, '', 1, 30);
  TextBox("Titulek", 2, '', 0, 100);
  TextBox("Zadejte <strong>součet</strong> čísel $aa a $bb", 4, '', 1, 2);
  CommentTextArea("Text komentáře", 3, '');
  FormBottom('Odesláním tohoto formuláře se zavazujete k tomu, že tuto možnost zanechání komentáře na webu nezneužijete k vlastní, nebo cizí reklamě a že Váš příspěvek nebude obsahoval vulgární výrazy a to v jakémkoliv jazyce!', "'txt', 'txt', 'txt', 'spam'", "3,0,1,$a", "30,100,2000,$b");
  return false;
}

function DeleteCommentForm()
{  
  global $_page;
  global $_inf;
  if (IsRefreshed("Komentář byl odstraněn")) return;
  
  $data = mysql_fetch_array( sql_select('comments',"id,date,title,author,text",
    "where pid = '$_page[id]' and state > 0 and id = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádný komentář"); return; }
  
  FormHead("Odstranit komentář");
  HiddenBox2(1,$data['id']);
  YNRadio("Opravdu chcete odstranit následující komentář?", 2, 0);
  FormBottom("$data[author]: $data[title] z $data[date]");
}

function StateCommentForm()
{  
  global $_page;
  global $_inf;
  if (IsRefreshed("Způsob zobrazení příspěvku byl změněno")) return;
  
  $data = mysql_fetch_array( sql_select('comments',"id,date,title,author,text,opinion,state",
    "where pid = '$_page[id]' and state > 0 and id = '$_inf[i]'",1
  ));
  if(!$data){ Alert("Na této pozici není žádný komentář"); return; }
  
  FormHead("Způsob zobrazení příspěvku");
  HiddenBox2(1,$data['id']);
  RadioButtons("Zvolte způsob zobrazení příspěvku", 2, $data['state'], Array(3, 2, 1) , Array("Smazaný", "Skrytý", "Normální") );
  SmallTextArea("Vyjádření k příspěvku", 3, $data['opinion']);
  FormBottom("$data[author]: $data[title] z $data[date]");
}




?>