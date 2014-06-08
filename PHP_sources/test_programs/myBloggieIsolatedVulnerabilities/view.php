<?php

/*
 * Mimic potential false positives and false negatives in file view.php of 
 * mybloggie.
 * 
 * 1. False positive due to the path-insensitivity
 *  - use of the variable $post_id (pixy reports that is can be undefined but it
 *      is defined in all places in which it is used)
 * 
 * 2. False negative due to the use of variables from session
 */




switch ($_GET['mode']) {
    case "editcom":
    case "viewid":
        $mode = $_GET['mode'];
        break;
    default :
        exit();
}
 
// 1. Definition of the variable
if ($mode == "viewid") {
    if (isset($_GET['post_id'])) {
        $post_id = intval($_GET['post_id']);
    } elseif (isset($_POST['post_id'])) {
        $post_id = intval($_POST['post_id']);
    } else {
        die("Error , Post no selected");
    }
    $template->assign_vars(array(
        'COMMENT_ACTION' => $_SERVER['PHP_SELF'] . "?mode=viewid&amp;post_id=" . $post_id,
            )
    );
    $comment_id = "";
    $user_ip = "";
}



if ($mode == "editcom") {
    if (!isset($_SESSION['username']) && !isset($_SESSION['passwd'])) {
        echo "<meta http-equiv=\"Refresh\" content=\"2;url=" . self_url() . "/login.php\" />";
    }
    $username = $_SESSION['username'];
    // VULNERABILITY: data from the session are used in the query
    $sql = "SELECT id, user, level FROM " . USER_TBL . " WHERE user='$username'";                                         ///214
    $result = $db->sql_query($sql);                                                                                        ///214
    $result = mysql_query($sql);
    $userid = $db->sql_fetchrow($result);                                                                                  ///214
    $level = $userid['level'];                                                                                           ///214
    if (isset($_GET['comment_id'])) {
        $comment_id = intval($_GET['comment_id']);
    } elseif (isset($_POST['comment_id'])) {
        $comment_id = intval($_POST['comment_id']);
    } else {
        die("Error , Post no selected");
    }
    if (isset($_GET['post_id'])) {
        $post_id = intval($_GET['post_id']);
    } elseif (isset($_POST['post_id'])) {
        $post_id = intval($_POST['post_id']);
    } else {
        // exits the script after printing the message -: no vulnerability
        error('Error', 'invalid Post ID ');
    }

    if (!isset($post_id) || empty($post_id) || $post_id == "") {
        error('Error', 'invalid Post_ID');
    }
    check_postid($post_id);
    check_commentid($comment_id);

    $sql = "SELECT * FROM " . COMMENT_TBL . " WHERE " . COMMENT_TBL . ".comment_id = " . $comment_id;
    $result = $db->sql_query($sql);
    $result = mysql_query($sql);
    if ($db->sql_numrows($result) == 1) {
        $edit = $db->sql_fetchrow($result);
        if (!isset($_POST["submit"])) {
            $template->assign_vars(array(
                'COMMENT_ACTION' => $_SERVER['PHP_SELF'] . "?mode=editcom&amp;post_id=" . $post_id . "&amp;comment_id=" . $comment_id,
                'COMMENTSUBJECT' => $edit['comment_subject'],
                'COMMENTTEXT' => $edit['comments'],
                'COMMENTNAME' => $edit['poster'],
                'COMMENTEMAIL' => $edit['email'],
                'COMMENTHOME' => $edit['home'],
                    )
            );
        }
    }
    $comment_id = "";
    $user_ip = "";
}

$user_ip = $HTTP_SERVER_VARS['REMOTE_ADDR'];

if (isset($_POST['commentemail'])) {$commentemail = $_POST['commentemail'];} else { $commentemail="" ;}
  $commentemail = trim($commentemail);
$commentsubject = $_POST['commentsubject'];
$commenttext = $_POST['commenttext'];
$commentname = $_POST['commentname'];
$commenthome = $_POST['commenthome'];
if ($commenthtmlsafe=="no") {
     $commentname = trim((stripslashes($commentname)));
     $commentsubject = trim((stripslashes($commentsubject)));
     $commenttext = trim((stripslashes($commenttext)));
     $commentemail = trim((stripslashes($commentemail)));
     $commenthome = trim((stripslashes($commenthome)));
}

else {
	/*
    $commentname = preg_replace($html_entities_match, $html_entities_replace,$commentname); 
    $commentsubject = preg_replace($html_entities_match, $html_entities_replace,$commentsubject); 
    $commenttext = preg_replace($html_entities_match, $html_entities_replace,$commenttext); 
    $commentemail = preg_replace($html_entities_match, $html_entities_replace,$commentemail); 
    $commenthome = preg_replace($html_entities_match, $html_entities_replace,$commenthome);
    */ 
	$commentname = mysql_real_escape_string($commentname);
	$commentsubject = mysql_real_escape_string($commentsubject);
	$commenttext = mysql_real_escape_string($commenttext);
	$commentemail = mysql_real_escape_string($commentemail);
	$commenthome = mysql_real_escape_string($commenthome);
}
$timestamp = time();


// some code (see view.php in original sources of mybloggie)

if ($mode == "viewid") {
    // FALSE POSITIVE ($post_id is initialized, $commentsubject is sanitized, ...), $post_id is not null 
    // due to the path-insensitiveness
    $sqladd = "INSERT INTO " . COMMENT_TBL . " SET post_id='$post_id', comment_subject='$commentsubject', comments='$commenttext', com_tstamp='$timestamp' ,
          poster = '$commentname', email='$commentemail' , home='$commenthome', ip='$user_ip'";
    $result = $db->sql_query($sqladd);
    $result = mysql_query($sqladd);
    if (!($result)) {
        $sql_error = $db->sql_error();
        error($lang['Error'], 'SQL Query Error : ' . $sql_error['message'] . ' !'); //214
    }
} elseif ($mode == "editcom") {
    // FALSE POSITIVE due to the path-insensitiveness ($comment_id is initialized, ...)
    $sqledit = "UPDATE " . COMMENT_TBL . " SET  comment_subject='$commentsubject', comments='$commenttext',
              poster = '$commentname', email='$commentemail' , home='$commenthome' Where comment_id='$comment_id' ";
    $result = $db->sql_query($sqledit);
    $result = mysql_query($sqledit);
    if (!($result)) {
        $sql_error = $db->sql_error();
        error($lang['Error'], 'SQL Query Error : ' . $sql_error['message'] . ' !'); //214
    }
    metaredirect(self_url() . "/index.php?mode=viewid&post_id=" . $post_id, 0);
//      $result = $db->sql_query($sqledit) or die("Cannot query the database.<br>" . mysql_error());
}
?>
