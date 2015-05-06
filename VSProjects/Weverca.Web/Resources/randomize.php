<html>
 <head>
  <title>PHP Test</title>
 </head>
 <body style="font:14px Courier New">
 <?
 
    $vratit="";
    for ($i=1;$i<=12;$i++){
      $nah=rand(0,62);
      switch($nah){
        case 10: $znak=0; break;
        case 11: $znak='a'; break; case 12: $znak='b'; break; case 13: $znak='c'; break;
        case 14: $znak='d'; break; case 15: $znak='e'; break; case 16: $znak='f'; break;
        case 17: $znak='g'; break; case 18: $znak='h'; break; case 19: $znak='i'; break;
        case 20: $znak='j'; break; case 21: $znak='k'; break; case 22: $znak='l'; break; 
        case 23: $znak='m'; break; case 24: $znak='n'; break; case 25: $znak='o'; break; 
        case 26: $znak='p'; break; case 27: $znak='q'; break; case 28: $znak='r'; break; 
        case 29: $znak='s'; break; case 30: $znak='t'; break; case 31: $znak='u'; break; 
        case 32: $znak='v'; break; case 33: $znak='w'; break; case 34: $znak='x'; break; 
        case 35: $znak='y'; break; case 36: $znak='z'; break; 
        case 37: $znak='A'; break; case 38: $znak='B'; break; case 39: $znak='C'; break; 
        case 40: $znak='D'; break; case 41: $znak='E'; break; case 42: $znak='F'; break; 
        case 43: $znak='G'; break; case 44: $znak='H'; break; case 45: $znak='I'; break;
        case 46: $znak='J'; break; case 47: $znak='K'; break; case 48: $znak='L'; break; 
        case 49: $znak='M'; break; case 50: $znak='N'; break; case 51: $znak='O'; break; 
        case 52: $znak='P'; break; case 53: $znak='Q'; break; case 54: $znak='R'; break; 
        case 55: $znak='S'; break; case 56: $znak='T'; break; case 57: $znak='U'; break; 
        case 58: $znak='V'; break; case 59: $znak='W'; break; case 60: $znak='X'; break; 
        case 61: $znak='Y'; break; case 62: $znak='Z'; break; 

        default: $znak = $nah;
      }
      $vratit .= $znak;
    }
  echo $vratit;
 
 
 ?>
</body>
</html>