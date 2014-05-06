<!-- start of $Id: html_mail.php 2567 2013-08-06 10:44:40Z oheil $ -->
<div class="mailNav">
   <table>
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');

$has_images = NOCC_Security::hasDisabledHtmlImages($content['body']);
$display_images = (isset($_REQUEST['display_images']) && $_REQUEST['display_images'] == 1) ? '1' : '0';

// Show/hide header link
$verbose = (isset($_REQUEST['verbose']) && $_REQUEST['verbose'] == 1) ? '1' : '0';
if ($conf->use_verbose)
  if($verbose == '1')
    echo '<tr><td class="mailSwitchHeaders dontPrint"><a href="action.php?'.NOCC_Session::getUrlGetSession().'&action=aff_mail&amp;mail=' . $content['msgnum'] . '&amp;verbose=0&amp;display_images='.$display_images.'">' . $html_remove_header . '</a></td>';
  else
    echo '<tr><td class="mailSwitchHeaders dontPrint"><a href="action.php?'.NOCC_Session::getUrlGetSession().'&action=aff_mail&amp;mail=' . $content['msgnum'] . '&amp;verbose=1&amp;display_images='.$display_images.'">' . $html_view_header . '</a></td>';
else
    echo '<tr><td>&nbsp;</td>';

// Next/prev message links
echo '<td class="right dontPrint">';
if (($content['prev'] != '') && ($content['prev'] != 0))
  echo '<a href="action.php?'.NOCC_Session::getUrlGetSession().'&action=aff_mail&amp;mail=' . $content['prev'] . '&amp;verbose=' . $verbose . '" title="' . $title_prev_msg . '" rel="prev">&laquo; ' . $alt_prev . '</a>';
echo "&nbsp;";
if (($content['next'] != '') && ($content['next'] != 0))
  echo '<a href="action.php?'.NOCC_Session::getUrlGetSession().'&action=aff_mail&amp;mail=' . $content['next'] . '&amp;verbose=' . $verbose . '" title="' . $title_next_msg . '" rel="next">' . $alt_next . ' &raquo;</a>';
echo "</td></tr>";

if ($conf->use_verbose && $verbose == '0') { //If displaying "normal" header...
  echo '<tr><th class="mailHeaderLabel">'.$html_from_label.'</th><td class="mailHeaderData">'.htmlspecialchars(NOCC_MailAddress::simplify($content['from'])).'</td></tr>';
  if (NOCC_MailAddress::compareAddress($content['from'], $content['reply_to']) == 0) { //if different 'From' and 'Reply-To' address...
    //TODO: Change $html_reply_to to $html_reply_to_label and add ':'!
    echo '<tr><th class="mailHeaderLabel">'.$html_reply_to.':</th><td class="mailHeaderData">'.htmlspecialchars(NOCC_MailAddress::simplify($content['reply_to'])).'</td></tr>';
  }
  if ($content['to'] != '') {
    echo '<tr><th class="mailHeaderLabel">'.$html_to_label.'</th><td class="mailHeaderData">'.htmlspecialchars(NOCC_MailAddress::simplify($content['to'])).'</td></tr>';
  }
  if ($content['cc'] != '') {
    echo '<tr><th class="mailHeaderLabel">'.$html_cc_label.'</th><td class="mailHeaderData">'.htmlspecialchars(NOCC_MailAddress::simplify($content['cc'])).'</td></tr>';
  }

  if ($content['subject'] == '')
    $content['subject'] = $html_nosubject;
  echo '<tr><th class="mailHeaderLabel">'.$html_subject_label.'</th><td class="mailHeaderData">'.htmlspecialchars($content['subject']).'</td></tr>';
  echo '<tr><th class="mailHeaderLabel">'.$html_date_label.'</th><td class="mailHeaderData">'.$content['complete_date'].'</td></tr>';
  if ($content['att'] != '') {
    echo $content['att'];
  }
  //TODO: Get "priority text" from MailReader class?
  $priority = '';
  switch ($content['priority']) {
    case 1: $priority = $html_highest; break;
    case 2: $priority = $html_high; break;
    case 4: $priority = $html_low; break;
    case 5: $priority = $html_lowest; break;
  }
  if ($priority != '') {
    echo '<tr><th class="mailHeaderLabel">'.$html_priority_label.'</th><td class="mailHeaderData">'.$priority.'</td></tr>';
  }
  echo '<tr><th class="mailHeaderLabel">' . $html_encoding_label . '</th><td class="mailHeaderData">';
  echo '<form id="encoding" action="action.php?'.NOCC_Session::getUrlGetSession().'" method="post"><div>';
  echo '<input type="hidden" name="action" value="' . $_REQUEST['action'] . '"/>';
  echo '<input type="hidden" name="mail" value="' . $_REQUEST['mail'] . '"/>';
  echo '<input type="hidden" name="verbose" value="' . $_REQUEST['verbose'] . '"/>';
  echo '<select class="button" name="user_charset">';
  $group = ''; $optgroupOpen = false;
  for ($i = 0; $i < sizeof($charset_array); $i++) { //for each charset...
    if ($charset_array[$i]->group != $group) { //if group changed...
      $group = $charset_array[$i]->group;
      if ($optgroupOpen) { //if <optgroup> open...
        echo '</optgroup>';
        $optgroupOpen = false;
      }
      if ($group != '') { //if group exists...
        echo '<optgroup label="' . $group . '">';
        $optgroupOpen = true;
      }
    }
    echo '<option value="'.$charset_array[$i]->charset.'"';
    if (isset($_REQUEST['user_charset']) && $_REQUEST['user_charset'] == $charset_array[$i]->charset
        || ((!isset($_REQUEST['user_charset']) || $_REQUEST['user_charset'] == '') && strtolower($content['charset']) == strtolower($charset_array[$i]->charset)))
      echo ' selected="selected"';
    echo '>'.$charset_array[$i]->label.'</option>';
  }
  if ($optgroupOpen) { //if <optgroup> open...
    echo '</optgroup>';
  }
  echo '</select>&nbsp;&nbsp;<input name="submit" class="button" type="submit" value="' . $html_submit . '" />';
  echo '</div></form>';
  echo '</td></tr>';
}
else { //If displaying "verbose" header...
  echo '<tr><td colspan="2">';
  echo '<pre class="mailVerboseHeader">'.htmlspecialchars(trim($content['header']), true).'</pre>';
  echo '</td></tr>';
  if ($content['att'] != '') {
    echo $content['att'];
  }
}
?>
   </table>
</div>
<?php
if ($has_images && $display_images != 1) {
  //TODO: USe better CSS class name as "nopic"!
  echo('<div class="nopic">');
  echo($html_images_warning);
  echo('<br/>');
  echo('<a href="action.php?'.NOCC_Session::getUrlGetSession().'&action=aff_mail&mail='.$content['msgnum'].'&verbose='.$verbose.'&display_images=1">'.$html_images_display.'</a>');
  echo('</div>');
}
if ($content['spam']) {
  echo('<div class="spamWarning">' . $html_spam_warning . '</div>');
}
?>
<div class="mailData">
<?php
//TODO: Rename this CSS class "mail" to "mailBody"?
echo '<div class="mail">'.$content['body'].'</div>';

?>
</div> <!-- .mailData -->
<!-- end of $Id: html_mail.php 2567 2013-08-06 10:44:40Z oheil $ -->
