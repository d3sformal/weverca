<?


	$a=4;

	try{
		if($a==4){
			echo "exception is going to be thrown";
			throw new Exception("");
			echo "unreachable code";
			}
		try{
			echo "exception2 is going to be thrown";
			throw new Exception2("");
			echo "unreachable code";
		}catch(Exception2 $e){
			echo "exception2 catched";
		}
	}
	catch(Exception $e){
		echo "exception catched";
		echo "a";
	}

?>