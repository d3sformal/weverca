<!-- start of $Id: menu_inbox_status.php 2077 2010-01-10 20:49:40Z gerundt $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');
?>
<?php if (isset($list_of_folders) && $list_of_folders != '') { ?>
                      <div id="inboxStatus">
                        <?php echo convertLang2Html($html_new_msg_in) . $list_of_folders ?>
                      </div>
<?php } ?>
<!-- end of $Id: menu_inbox_status.php 2077 2010-01-10 20:49:40Z gerundt $ -->
