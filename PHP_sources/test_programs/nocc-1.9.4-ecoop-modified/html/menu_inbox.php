<!-- start of $Id: menu_inbox.php 2567 2013-08-06 10:44:40Z oheil $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');

$action = NOCC_Request::getStringValue('action');
$selected = 0;
switch ($action) {
  case '':
  case 'login':
  case 'cookie':
    $selected = 1;
    $line = '<a href="action.php?'.NOCC_Session::getUrlGetSession().'&action=write">'.$html_new_msg.'</a>';
    break;
  case 'write':
    $selected = 2;
    $line = '<span>' . $html_new_msg . '</span>';
    break;
  case 'reply':
    $selected = 2;
    $line = '<span>' . $html_reply . '</span>';
    break;
  case 'reply_all':
    $selected = 2;
    $line = '<span>' . $html_reply_all . '</span>';
    break;
  case 'forward':
    $selected = 2;
    $line = '<span>' . $html_forward . '</span>';
    break;
  case 'managefolders':
    $selected = 3;
    $line = '<a href="action.php?'.NOCC_Session::getUrlGetSession().'&action=write">'.$html_new_msg.'</a>';
    break;
}
?>
<div class="mainmenu">
  <ul>
    <?php if ($selected == 1) echo '<li class="selected">'; else echo '<li>'; ?>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>"><?php echo convertLang2Html($html_inbox); ?></a>
    </li>
    <?php if ($selected == 2) echo '<li class="selected">'; else echo '<li>'; ?>
      <?php echo $line ?>
    </li>
    <?php if ($_SESSION['is_imap']) { ?>
    <?php if ($selected == 3) echo '<li class="selected">'; else echo '<li>'; ?>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>&action=managefolders" title="<?php echo convertLang2Html($html_manage_folders_link); ?>"><?php echo convertLang2Html($html_folders); ?></a>
    </li>
    <?php } ?>
    <?php if ($conf->prefs_dir && isset($conf->contact_number_max) && $conf->contact_number_max != 0 ) { ?>
    <li>
      <a href="javascript:void(0);" onclick="window.open('contacts_manager.php?<?php echo NOCC_Session::getUrlGetSession();?>&<?php echo NOCC_Session::getUrlQuery(); ?>','','scrollbars=yes,resizable=yes,width=600,height=400')"><?php echo i18n_message($html_contacts, ''); ?></a>
    </li>
    <?php } ?>
    <?php if (isset($_GET['successfulsend']) && $_GET['successfulsend']) { ?>
    <li>
      <?php echo convertLang2Html($html_send_confirmed); ?>
    </li>
    <?php } ?>
  </ul>
</div>
<!-- end of $Id: menu_inbox.php 2567 2013-08-06 10:44:40Z oheil $ -->
