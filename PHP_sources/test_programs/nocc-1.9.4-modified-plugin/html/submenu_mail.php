<!-- start of $Id: submenu_mail.php 2567 2013-08-06 10:44:40Z oheil $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');
?>
<div class="submenu">
  <ul>
    <li>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>&action=write"><?php echo convertLang2Html($html_new_msg) ?></a>
    </li>
    <li>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>&action=reply&amp;mail=<?php echo $content['msgnum'] ?>"><?php echo convertLang2Html($html_reply) ?></a>
    </li>
    <li>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>&action=reply_all&amp;mail=<?php echo $content['msgnum'] ?>"><?php echo convertLang2Html($html_reply_all) ?></a>
    </li>
    <li>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>&action=forward&amp;mail=<?php echo $content['msgnum'] ?>"><?php echo convertLang2Html($html_forward) ?></a>
    </li>
    <li>
      <a href="down_mail.php?<?php echo NOCC_Session::getUrlGetSession();?>&mail=<?php echo $content['msgnum'] ?>"><?php echo convertLang2Html($html_down_mail) ?></a>
    </li>
    <li>
      <a href="delete.php?<?php echo NOCC_Session::getUrlGetSession();?>&delete_mode=1&amp;mail=<?php echo $content['msgnum'] ?>&amp;only_one=1" onclick="if (confirm('<?php echo $html_del_msg ?>')) return true; else return false;"><?php echo convertLang2Html($html_delete) ?></a>
    </li>
  </ul>
</div>
<!-- end of $Id: submenu_mail.php 2567 2013-08-06 10:44:40Z oheil $ -->
