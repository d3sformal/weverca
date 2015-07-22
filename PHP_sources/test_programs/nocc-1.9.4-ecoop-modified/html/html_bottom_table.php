<!-- start of $Id: html_bottom_table.php 2078 2010-01-11 18:42:42Z gerundt $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');
?>
                      </table>
                    <?php include 'menu_inbox_status.php'; ?>
                    <?php include 'menu_inbox_bottom_opts.php'; ?>
                  <!-- end of Message list bloc -->
                </div>
                <?php if ($pages > 1) { ?>
                <div class="bottomNavigation">
                  <table>
                    <tr>
                      <td class="inbox">&nbsp;</td>
                      <td class="inbox right">
                    <?php echo $page_line ?>
                      </td>
                    </tr>
                  </table>
                </div>
                <?php } ?>
<!-- end of $Id: html_bottom_table.php 2078 2010-01-11 18:42:42Z gerundt $ -->
