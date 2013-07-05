<?

namespace test;

func1();
echo "<br>";

func2(0);
echo "<br>";

func3(0);
echo "<br>";

//Fatal error: Call to undefined function cond1() on line 13
//cond1();
//echo "<br>";

if ($_GET['c'])
{
  //Fatal error: Call to undefined function cond1() on line 19
  //cond1();
  //echo "<br>";

  function cond1()
  {
    echo "cond1 - true";
  }  
  
  cond1();
  echo "<br>";
}
else
{
  //Fatal error: Call to undefined function cond1() on line 36
  //cond1();
  //echo "<br>";

  function cond1()
  {
    echo "cond1 - false";
  }
  
  cond1();
  echo "<br>";
}


cond1();
echo "<br>";


$x = 1;
while ($x < 2)
{
  //If the cycle will repeat - Fatal error: Cannot redeclare loop1() on line 54
  function loop1()
  {
    echo "loop1";
  } 
  $x++;
}

loop1();
echo "<br>";


function func1()
{
  echo "func1";
}

function func2($x, $y = 0)
{
  if ($x == $y)
  {
    echo "func2 - $x == $y";
  }
  else
  {
    echo "func2 - $x != $y";
  }
}

function func3($x)
{
  echo "func3";
  return $x;
}

function returns1($x)
{
  if ($x == 1) return 1;
  else if ($x == 2) return 2;
  else if ($x == 3) return 3;
  else return 4;
  
  return 5;
}

function returns2($x)
{
  if ($x == 1) return 1;
  return 5;
}

?>