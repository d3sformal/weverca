<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');
?>

<div class="prefs">
<h3><?php echo convertLang2Html($html_manage_filters_link) ?></h3>
<?php if ($html_filter_select) { ?>
<form method="post" action="action.php?<?php echo NOCC_Session::getUrlGetSession();?>">
<div>
<input type="hidden" name="action" value="managefilters" />
<input type="hidden" name="do" value="delete" />
<table>
<tr>
  <td class="center">
    <?php echo $html_filter_select ?>
  </td>
</tr>
<tr>
  <td class="center" colspan="4">
    <input type="submit" class="button" value="<?php echo convertLang2Html($html_filter_remove) ?>" />
  </td>
</tr>
</table>
</div>
</form>
<?php } ?>
<?php 
if ($html_filter_select) {
  echo convertLang2Html($html_filter_change_tip);
}
?>
<form method="post" action="action.php?<?php echo NOCC_Session::getUrlGetSession();?>">
<div>
<input type="hidden" name="action" value="managefilters" />
<input type="hidden" name="do" value="create" />
<table>
<tr>
  <td class="prefsLabel">
    <select class="button" name="thing1">
      <option value="-" selected="selected"><?php echo convertLang2Html($html_select_one) ?></option>
      <option value="BODY"><?php echo convertLang2Html($html_filter_body) ?></option>
      <option value="SUBJECT"><?php echo convertLang2Html($html_filter_subject) ?></option>
      <option value="TO"><?php echo convertLang2Html($html_filter_to) ?></option>
      <option value="FROM"><?php echo convertLang2Html($html_filter_from) ?></option>
      <option value="CC"><?php echo convertLang2Html($html_filter_cc) ?></option>
    </select>
  </td>
  <td class="prefsData">
    <label for="contains1"><?php echo convertLang2Html($html_filter_contains) ?></label>
    <input class="button" type="text" id="contains1" name="contains1" size="20" maxlength="80"/>
  </td>
</tr>
<tr>
  <td class="prefsLabel">
    <label for="thing2"><?php echo convertLang2Html($html_and) ?>&nbsp;</label>
    <select class="button" id="thing2" name="thing2">
      <option value="-" selected="selected"><?php echo convertLang2Html($html_select_one) ?></option>
      <option value="BODY"><?php echo convertLang2Html($html_filter_body) ?></option>
      <option value="SUBJECT"><?php echo convertLang2Html($html_filter_subject) ?></option>
      <option value="TO"><?php echo convertLang2Html($html_filter_to) ?></option>
      <option value="FROM"><?php echo convertLang2Html($html_filter_from) ?></option>
      <option value="CC"><?php echo convertLang2Html($html_filter_cc) ?></option>
    </select>
  </td>
  <td class="prefsData">
    <label for="contains2"><?php echo convertLang2Html($html_filter_contains) ?></label>
    <input class="button" type="text" id="contains2" name="contains2" size="20" maxlength="80"/>
  </td>
</tr>
<tr>
  <td class="prefsLabel">
    <label for="thing3"><?php echo convertLang2Html($html_and) ?>&nbsp;</label>
    <select class="button" id="thing3" name="thing3">
      <option value="-" selected="selected"><?php echo convertLang2Html($html_select_one) ?></option>
      <option value="BODY"><?php echo convertLang2Html($html_filter_body) ?></option>
      <option value="SUBJECT"><?php echo convertLang2Html($html_filter_subject) ?></option>
      <option value="TO"><?php echo convertLang2Html($html_filter_to) ?></option>
      <option value="FROM"><?php echo convertLang2Html($html_filter_from) ?></option>
      <option value="CC"><?php echo convertLang2Html($html_filter_cc) ?></option>
    </select>
  </td>
  <td class="prefsData">
    <label for="contains3"><?php echo convertLang2Html($html_filter_contains) ?></label>
    <input class="button" type="text" id="contains3" name="contains3" size="20" maxlength="80"/>
  </td>
</tr>
<tr>
  <td class="prefsLabel">
    <label for="filtername"><?php echo convertLang2Html($html_filter_name) ?>:</label>
  </td>
  <td class="prefsData">
    <input class="button" type="text" id="filtername" name="filtername" size="40" maxlength="80"/>
  </td>
</tr>
<tr>
  <td class="prefsLabel">
    <?php echo convertLang2Html($html_filter_action) ?>:
  </td>
  <td class="prefsData">
    <input type="radio" id="filter_action_move" name="filter_action" value="MOVE" checked="checked" /><label for="filter_action_move"><?php echo convertLang2Html($html_filter_moveto) ?></label>
    <?php echo $filter_move_to ?>
    &nbsp;&nbsp;&nbsp;
    <input type="radio" id="filter_action_delete" name="filter_action" value="DELETE"/><label for="filter_action_delete"><?php echo convertLang2Html($html_filter_remove) ?></label>
  </td>
</tr>
</table>
    <?php
      if (NoccException::isException($ev)) {
    ?>
      <div class="error">
    <table class="errorTable">
      <tr class="errorTitle">
        <td><?php echo convertLang2Html($html_error_occurred) ?></td>
      </tr>
      <tr class="errorText">
        <td>
          <p><?php echo convertLang2Html($ev->getMessage()); ?></p>
        </td>
      </tr>
    </table>
      </div>
    <?php
      } else {
    if(isset($_REQUEST['do']))
      echo '<p>' . convertLang2Html($html_prefs_updated) . '</p>';
      }
    ?>
<p class="sendButtons">
  <input type="submit" class="button" value="<?php echo convertLang2Html($html_submit) ?>" />
  &nbsp;&nbsp;
  <input type="reset" class="button" value="<?php echo convertLang2Html($html_cancel) ?>" />
</p>
</div>
</form>
</div>
<!-- end of $Id: filter_prefs.php 2581 2013-08-22 10:14:02Z gerundt $ -->
