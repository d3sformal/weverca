<!-- start of $Id: menu_prefs.php 2567 2013-08-06 10:44:40Z oheil $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');
?>
<div class="mainmenu">
  <ul>
    <li>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>"><?php echo convertLang2Html($html_inbox); ?></a>
    </li>
    <li>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>&action=write"><?php echo convertLang2Html($html_new_msg) ?></a>
    </li>
    <?php if ($_SESSION['is_imap']) { ?>
    <li>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>&action=managefolders" title="<?php echo convertLang2Html($html_manage_folders_link); ?>"><?php echo convertLang2Html($html_folders); ?></a>
    </li>
    <?php } ?>
    <?php if ($conf->prefs_dir && isset($conf->contact_number_max) && $conf->contact_number_max != 0 ) { ?>
    <li>
      <a href="javascript:void(0);" onclick="window.open('contacts_manager.php?<?php echo NOCC_Session::getUrlGetSession();?>&<?php echo NOCC_Session::getUrlQuery(); ?>','','scrollbars=yes,resizable=yes,width=600,height=400')"><?php echo i18n_message($html_contacts, '') ?></a>
    </li>
    <?php } ?>
    <li class="selected">
      <span><?php echo convertLang2Html($html_preferences) ?></span>
    </li>
  </ul>
</div>
<!-- end of $Id: menu_prefs.php 2567 2013-08-06 10:44:40Z oheil $ -->
