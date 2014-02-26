<?php
  mysql_connect("localhost", "root") or die("Neda sa pripojit k databaze: " . mysql_error());
      mysql_select_db("metalcup") or die ("Neda sa pracovat s databazov : " . mysql_error());
      mysql_query("set names utf8");
$l=mysql_query("select * from admin")or die(mysql_error());
$zaznam=mysql_fetch_array($l);
$zostava=$_POST["Zostava"];
$lw=$_POST["LW"];
$rw=$_POST["RW"];
$c=$_POST["C"];
$ld=$_POST["LD"];
$rd=$_POST["RD"];
$g=$_POST["G"];
$formacie="<table width=\"700\"><th align=\"center\">";
$formacie.=$zostava."</th></table>";
$formacie.="<table width=\"700\"<tr><td width=\"50%\" align=\"left\"><img src=\"obrazky/utocnik.bmp\"></td><td width=\"50%\" align=\"right\"><img src=\"obrazky/utocnik.bmp\"></td></tr>";
$formacie.="<tr><td width=\"50%\" align=\"left\">".$lw."</td><td width=\"50%\" align=\"right\">".$rw."</td></tr></table>";
$formacie.="<table width=\"700\"><tr><td width=\"100%\" align=\"center\"><img src=\"obrazky/utocnik.bmp\"></td></tr>";
$formacie.="<tr><td width=\"100%\" align=\"center\">".$c."</td></tr></table>";
$formacie.="<table width=\"700\"><tr><td width=\"50%\" align=\"left\"><img src=\"obrazky/obranca.bmp\"></td><td width=\"50%\" align=\"right\"><img src=\"obrazky/obranca.bmp\"></td></tr>";
$formacie.="<tr><td width=\"50%\" align=\"left\">".$ld."</td><td width=\"50%\" align=\"right\">".$rd."</td></tr></table>";
$formacie.="<table width=\"700\"><tr><td width=\"100%\" align=\"center\"><img src=\"obrazky/brankar.bmp\"></td></tr>";
$formacie.="<tr><td width=\"100%\" align=\"center\">".$g."</td></tr></table>";
$text=$_POST["text"];
$text=str_replace(chr(13),"<br>",$text);
$text=$text."<br><br>".$formacie;
$poradie="<table><th>Poradie tipujúcich</th>";
$a=mysql_query("select * from uzivatelia where suma>1000 order by suma desc")or die (mysql_error());
while($vypis=mysql_fetch_array($a))
{
$poradie=$poradie."<tr><td>".$vypis["meno"]."</td><td>".$vypis["suma"]." €</td></tr>";
}
$poradie=$poradie."</table>";
$text=$text."<br><br><table width=\"600\"><tr><td align=\"left\">".$poradie."</td><td align=\"center\">";
$_plus=0;
$minus=0;
$t=mysql_query("select * from uzivatelia")or die (mysql_error());
while($vypis=mysql_fetch_array($t))
{
$_plus+=$vypis["plus"];
$minus+=$vypis["minus"];
}
$zisk=$_plus-$minus;
if($zisk>0)
{
$farba="green";
$plus="+";
}
elseif($zisk<0)
{
$farba="red";
$plus="-";
$zisk=$zisk-$zisk-$zisk;
}
else
{
$farba="gainsboro";
$plus="";
}
$tipovanie="<b>Štatistka tipovania</b><table><tr class=\"green\"><td>Celkové výhry:</td><td>+</td><td> ".$_plus."</td><td>€</td></tr>
<tr class=\"red\"><td>Celkové vklady:</td><td>-</td><td>".$minus."</td><td>€</td></tr>
<tr class=\"".$farba."\"><td>Celkový zisk:</td><td>".$plus."</td><td>".$zisk."</td><td>€</td></tr>
</table>";
$text=$text.$tipovanie."</td><td align=\"right\">";

$text=$text."<table><th>Top 10 kurz</th>";

$dd=mysql_query("select * from vyherne_tikety order by kurz desc limit 0,10")or die (mysql_error());
while($vypis=mysql_fetch_array($dd))
{
$text=$text."<tr><td>".$vypis["meno"]."</td><td>".$vypis["kurz"]."</td></tr>";
}
$text=$text."</table></td></tr></table>";




mysql_query("insert into hist_stat (text,rocnik) values ('".$text."','".$zaznam["rocnik"]."')")or die(mysql_error());
header("Location:historia.php"); 
?>