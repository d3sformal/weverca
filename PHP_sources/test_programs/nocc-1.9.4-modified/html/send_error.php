<!-- start of $Id: send_error.php 1685 2009-01-10 00:10:33Z gerundt $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');
?>
<p class="inbox"><?php echo convertLang2Html($html_error_occurred) . ' : ' . convertLang2Html($ev->getMessage()); ?></p>
<!-- end of $Id: send_error.php 1685 2009-01-10 00:10:33Z gerundt $ -->
