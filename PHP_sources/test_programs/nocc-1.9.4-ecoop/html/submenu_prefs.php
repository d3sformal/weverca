<!-- start of $Id: submenu_prefs.php 2581 2013-08-22 10:14:02Z gerundt $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');

  if ($pop->is_imap() && $conf->prefs_dir) {
    $action = NOCC_Request::getStringValue('action');
    $selected = 0;
    switch ($action) {
      case '':
      case 'setprefs':
        $selected = 1;
        break;
      case 'managefilters':
        $selected = 2;
        break;
    }
?>
<div class="submenu">
  <ul>
    <?php if ($selected == 1) echo '<li class="selected">'; else echo '<li>'; ?>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>&action=setprefs"><?php echo convertLang2Html($html_preferences) ?></a>
    </li>
    <?php if ($selected == 2) echo '<li class="selected">'; else echo '<li>'; ?>
      <a href="action.php?<?php echo NOCC_Session::getUrlGetSession();?>&action=managefilters"><?php echo convertLang2Html($html_manage_filters_link) ?></a>
    </li>
  </ul>
</div>
<?php
  }
?>
<!-- end of $Id: submenu_prefs.php 2581 2013-08-22 10:14:02Z gerundt $ -->
