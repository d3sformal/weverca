<?

function test($a){
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
}
class A{
	function foo()
	{
		try
		{
			throw new Exp();
		}
		catch(Exp $e)
		{
			echo "Exp catchced";
		}		
	}
}
function B()
{
		try
		{
			throw new Exp();
		}
		catch(Exp $e)
		{
			echo "Exp catchced";
		}		
}

try{
	throw new Exception2("");
		
}
catch(Exception $e){
	echo "exception catcheds";
	echo "a";
}
?>