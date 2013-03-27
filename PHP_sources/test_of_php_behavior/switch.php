<?

/*******************************************************************************
 * Program output:
 * ***************************************************************************** 
TestSwitch 1
1
TestSwitch 2
23
TestSwitch 3
3
TestSwitch 4
4
TestSwitch 5
d
TestSwitch a
a
TestSwitch b
b
TestSwitch my class T
1
TestSwitch my class T
1
TestSwitch my class T
1
********************************************************************************/

testSwitch(1, "b");
testSwitch(2, "b");
testSwitch(3, "b");
testSwitch(4, 4);
testSwitch(5, "b");
testSwitch("a", "b");
testSwitch("b", "b");

$y = new T();
testSwitch($y, "b");
testSwitch($y, $y);
testSwitch($y, new T());


function testSwitch($x, $b)
{
  echo "<br>TestSwitch $x<br>";

  switch ($x)
  {
    case 1: echo "1"; break;
    case 2: echo "2";
    case 3: echo "3"; break;
    case "a": echo "a"; break;
    case $b: echo "$b"; break;
    default: echo "d";
  };
}

class T
{ 
    public function __toString()
    {
        return "my class T";
    }
}


?>