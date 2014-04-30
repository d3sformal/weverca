<?php

/*
 * Mimic false positives and false negatives of the Pixy tool.
 * 
 * 1. False positive due to the path-insensitiveness
 *  - variables $comment_id and $post_id are defined conditionally and than used. 
 *      However, it holds that if they are used, they are defined.
 * 
 * 2. Vulnerability (the variable $comment_id is not sanitized)
 * 
 * 
 */

if (isset($_GET['comment_id'])) {intval($comment_id = $_GET['comment_id']); }
if (isset($_GET['post_id'])) {$post_id = intval($_GET['post_id']); } else { $post_id = ""; }
if (isset($_POST['confirm'])) {$confirm = $_POST['confirm']; }

if (!isset($confirm)  && isset($post_id) && isset($comment_id)) {
message($lang['Confirm']," <form action=\"".$_SERVER['PHP_SELF']."?mode=delcom\" method=\"post\"><br />". $lang['Msg_Del_error3']."<br />
                           <input type=\"hidden\" name=\"post_id\" value=\"".$post_id."\" />
                           <input type=\"hidden\" name=\"comment_id\" value=\"".$comment_id."\" />
                           <input type=\"submit\" name=\"confirm\" value=\"yes\" />&nbsp;&nbsp;<input type=\"submit\" name=\"confirm\" value=\"no\" /></form>");
}
elseif ($confirm=="yes"){
if (isset($_POST['comment_id'])) {intval($comment_id = $_POST['comment_id']); }
if (isset($_POST['post_id'])) {$post_id = intval($_POST['post_id']); }
// Data Base Connection  //
$sql = "DELETE FROM ".COMMENT_TBL." WHERE comment_id=$comment_id";
$result = $db->sql_query($sql);
$result = mysql_query($sql);
if( !($result) )
   {
     $sql_error = $db->sql_error();         //214
     error($lang['Error'], 'SQL Query Error : '.$sql_error['message'].' !');
   }
$confirm ="";
message($lang['Del'], $lang['Msg_Del']."<br /><br />Click <a href=\"".self_url()."/admin.php?mode=all_com\">>Here<</a> if redirect failed ");
//metaredirect(self_url()."/admin.php?mode=all_com",1);
} elseif ($confirm=="no"){
//metaredirect(self_url()."/admin.php?mode=all_com",0);
} else {
message($lang['Error'], 'Abnormal Operation ! Request Aborted.');
//metaredirect(self_url()."/admin.php",0);
}

?>
