<?php

  $a=4;
  switch($a){
      case 2:
		$a++; echo "2";
      case 3:
      	echo "3"; 
		echo "break";
		break;
	case 8646:
		echo "continue";
		continue;
      default: $b++;
		echo "default";
      case 4: $a++;
		echo "4";
  }
            
?>