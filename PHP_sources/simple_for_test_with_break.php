<?

$n=$_GET["n"];
$res=1;
for($i=2;$i<$n;$i++)
{
	$res*=$i;
	if($i==4587){
		echo "breaking";
		break;
		}
}
?>