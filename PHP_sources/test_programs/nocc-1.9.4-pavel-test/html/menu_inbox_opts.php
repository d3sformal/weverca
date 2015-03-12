<!-- start of $Id: menu_inbox_opts.php 1685 2009-01-10 00:10:33Z gerundt $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');
?>
                          <table>
                            <tr>
                              <td class="menuOpts left">
                                <input type="button" class="button" value="<?php echo convertLang2Html($html_select_all); ?>" onselect="InvertCheckedMsgs()" onclick="InvertCheckedMsgs()" />
                              </td>
                              <td class="menuOpts center">
                              <?php
                                if ($pop->is_imap() && $pop->get_folder_count() > 1) {
                                  $html_target_select = $pop->html_folder_select('target_folder', '');
                              ?>
                                <input type="submit" class="button" name="move_mode" value="<?php echo convertLang2Html($html_move); ?>" /> <?php echo convertLang2Html($html_or); ?>
                                <input type="submit" class="button" name="copy_mode" value="<?php echo convertLang2Html($html_copy); ?>" />
                                <label for="target_folder"><?php echo convertLang2Html($html_messages_to); ?></label>
                                <?php echo $html_target_select; ?>
                              <?php
                                }
                              ?>
                              </td>
                              <td class="menuOpts right">
                                <?php
                                  if ($pop->is_imap()) {
                                ?>
                                <input type="submit" name="mark_read_mode" class="button" value="<?php echo convertLang2Html($html_mark_as); ?>" />
                                <select class="button" name="mark_mode">
                                  <option value="read"><?php echo convertLang2Html($html_read); ?></option>
                                  <option value="unread"><?php echo convertLang2Html($html_unread); ?></option>
                                </select>
                                <?php
                                  }
                                ?>
                                <input type="submit" name="forward_mode" class="button" value="<?php echo $html_forward; ?>" />
                                <input type="submit" name="delete_mode" class="button" value="<?php echo $html_delete; ?>" onclick="if (confirm('<?php echo $html_del_msg; ?>')) return true; else return false;"/>
                              </td>
                            </tr>
                          </table>
<!-- end of $Id: menu_inbox_opts.php 1685 2009-01-10 00:10:33Z gerundt $ -->
