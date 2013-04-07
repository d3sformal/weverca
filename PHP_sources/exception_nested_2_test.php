<?
$a=4;

try{

	try{
		echo "exception2 is going to be thrown";
		throw new Exception2("");
		echo "unreachable code";
	}catch(Exception2 $e){
		echo "exception2 catched";
	}
	
	try{
		echo "exception3 is going to be thrown";
		throw new Exception3("");
		echo "unreachable code";
	}catch(Exception3 $e){
		echo "exception3 catched";
	}
	echo "exception1 is going to be thrown";
	throw new Exception1("");
	echo "unreachable code";
}
catch(Exception $e){
	echo "exception catched";
	echo "a";
}

?>