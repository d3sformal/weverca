<?

try{
	try{
		try{
		$x=4;
		}
		catch(Exception $e){
			echo "exception catched";
			echo "a";
		}
	}
	catch(Exception $e){
		echo "exception catched";
		echo "a";
	}
}
catch(Exception $e){
	echo "exception catched";
	echo "a";
}
?>