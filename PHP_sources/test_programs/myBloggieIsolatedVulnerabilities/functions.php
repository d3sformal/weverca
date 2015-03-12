<?php

function self_url( $display=0 ) {

if  (isset($_SERVER['PHP_SELF']) && isset($_SERVER['HTTP_HOST'])) {
   $me = $_SERVER['PHP_SELF'];
   $Apathweb = explode("/", $me);
$myFileName = array_pop($Apathweb);
   $pathweb = implode("/", $Apathweb);
   $myUrl = "http://".$_SERVER['HTTP_HOST'].$pathweb;
   }
elseif (isset($_SERVER['PHP_SELF']) && isset($_SERVER['SERVER_NAME'])) {
   $me = $_SERVER['PHP_SELF'];
   $Apathweb = explode("/", $me);
   $pathweb = implode("/", $Apathweb);
   $myUrl = 'http://'.$_SERVER['SERVER_NAME'].$pathweb;
   }
   //echo $pathweb;
   //echo $Apathweb[0];
 if ($display) {
        echo $myUrl;
    } else {
        return $myUrl;
    }
}


function error( $alert, $message )
    { global $mybloggie_root_path;
      ?>
     <table width="98%"  height="300" cellspacing="0" cellpadding="0"  border="0">
     <tr><td valign="middle">
     <table width="100%" class="tableborder" cellspacing="1" cellpadding="2"  border="0">
    <tr>
     <td class="tdhead" bgcolor="#6699ff"><center><?php echo $alert  ?></center></td></tr>
    <tr>
     <td class="error" valign="middle">
      <center><br /><?php echo $message  ?><br /><br /></center>
     </td></tr><tr>
     <td class="error" align="center"><a class="std" href="index.php">myBloggie Home</a>  | <a class="std" href="javascript:history.back()">Back</a></td>
     </tr>
     </table>
     </td></tr>
     </table>
   <?php
   $template = new Template('./templates/') ;
   $template->set_filenames(array(
              'footer' => $mybloggie_root_path.'footer.tpl' ));
   $template->pparse('footer');
   exit;
    }
    
function message( $alert, $message )
    { global $mybloggie_root_path;
     ?>
     <table width="98%"  height="300" cellspacing="0" cellpadding="0"  border="0">
     <tr><td valign="middle">
     <table width="100%" class="tableborder" cellspacing="1" cellpadding="2"  border="0">
    <tr>
     <td class="tdhead" bgcolor="#6699ff"><center><?php echo $alert  ?></center></td></tr>
    <tr>
     <td class="error" valign="middle">
      <center><br /><?php echo $message  ?><br /><br /></center>
     </td></tr><tr>
     <td class="error" align="center"><a class="std" href="index.php">myBloggie Home</a>  | <a class="std" href="javascript:history.back()">Back</a></td>
     </tr>
     </table>
     </td></tr>
     </table>
   <?php
    }
    
    function metaredirect($url, $sec)
{
    global $db, $template;
    
    $url = htmlspecialchars($url);
    echo "<meta http-equiv=\"refresh\" content=\"".$sec.";url=".$url."\" />" ;
}
?>
