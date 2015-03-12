<!-- start of $Id: no_mail.php 1685 2009-01-10 00:10:33Z gerundt $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');
?>
<tr>
  <td colspan="<?php echo count($conf->column_order) + 1; ?>" class="inbox center">
    <?php echo convertLang2Html($html_no_mail) ?>
  </td>
</tr>
<!-- end of $Id: no_mail.php 1685 2009-01-10 00:10:33Z gerundt $ -->
