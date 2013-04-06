<?
$a=4;

try{
	if($a==4){
		echo "exception is going to be thrown";
		throw new Exception("");
		echo "unreachable code";
	}
	echo "no error here";
}
catch(Exception $e){
	echo "exception catched";
	echo "a";
}
catch(Exception1 $e){
	echo "Exception1 catched";
	echo "a";
}
catch(Exception2 $e){
	echo "Exception2 catched";
	echo "a";
}
catch(Exception3 $e){
	echo "Exception3 catched";
	echo "a";
}
?>