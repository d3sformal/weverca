<?
echo "program start";
switch($_POST["x"])
{
	case 0:
		echo 0;
	break;
	echo "unreachable code";
	case 1:
		echo 1;
	break;
	echo "unreachable code";
	default:
		echo "default";
	
}
echo "end of program";

?>