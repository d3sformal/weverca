<?
echo "<h1>GOTO test</h1>";



echo "<h2>Simple goto</h2>";
goto a;
echo 'Foo';
 
a:
echo 'Bar';

echo "<h2>Cycle example</h2>";
for($i=0,$j=50; $i<100; $i++) {
  while($j--) {
    if($j==17) goto end; 
  }  
}
echo "i = $i";
end:
echo 'j hit 17';

echo "<h2>Link Example</h2>";
$link = true;

if ( $link ) goto iw_link_begin;
if(false) iw__link_begin:

if ( $link ) goto iw_link_text;
if(false) iw__link_text:

if ( $link ) goto iw_link_end;
if(false) iw__link_end:

goto iw_end_gt;


if (false) iw_link_begin:
    echo '<a href="#">';
goto iw__link_begin;

if (false) iw_link_text:
    echo 'Sample Text';
goto iw__link_text;

if (false) iw_link_end:
    echo '</a>';
goto iw__link_end;

iw_end_gt:
  
  
  

//Fatal error: 'goto' into loop or switch statement is disallowed on line 57
/*echo "<h2>Cycle - doesnt work</h2>";
goto loop;
for($i=0,$j=50; $i<100; $i++) {
  while($j--) {
    loop:
  }
}
echo "$i = $i";*/

  

//Fatal error: 'goto' to undefined label 'functionLabel' on line 69
/*echo "<h2>FunctionTest - doesnt work</h2>";
goto functionLabel; 

function someFunction()
{
  functionLabel:
  echo "I'm in FUNC;";
}*/



//Parse error: syntax error, unexpected T_VARIABLE, expecting T_STRING on line 82
/*echo "<h2>Variable test - doesnt work</h2>";
$a = 'abc';
goto $a; // PARSE ERROR
echo 'Foo';
abc: echo 'Boom'; */


?>