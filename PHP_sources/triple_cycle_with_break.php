<?php

for($i=0;$i<10000;$i++)
{
echo "in first for";
	for($j=0;$j<400;$j++)
	{
		echo "in second for";
		while(true){
			echo "in third for";
			if($break=="break"){
				echo "breaking";
				break $a;
			}
		}
		
	}

}
       
echo "end";	   
?>