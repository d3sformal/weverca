<?
$a=4;

try{
	if($a==4){
		throw new Exception("A is equal 4 and thas an error");
		echo "unreachable code";
	}
	echo "no error here";
}
catch(Exception $e){
	echo "exception catched";
	echo "a";
}

?>