<!-- start of $Id: folders.php 2579 2013-08-19 21:31:44Z gerundt $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');

$renameoldbox = $pop->html_folder_select('renameoldbox', $_SESSION['nocc_folder']);
$removeoldbox = $pop->html_folder_select('removeoldbox', $_SESSION['nocc_folder']);
$downloadbox = $pop->html_folder_select('downloadbox', $_SESSION['nocc_folder']);

$big_list = $pop->getmailboxesnames();

$select_list = array();
if (count($big_list) > 1) {
  for ($i = 0; $i < count($big_list); $i++) {
	$selected="";
	if( $big_list[$i]==$_SESSION['nocc_folder'] ) {
		$selected="selected=\"selected\"";
	}
    array_push($select_list, "\t<option ".$selected." value=\"".$big_list[$i]."\">".mb_convert_encoding($big_list[$i], 'UTF-8', 'UTF7-IMAP')."</option>\n");
  }
}

?>
<div class="prefs">
<h3><?php echo convertLang2Html($html_folders) ?></h3>
  <form method="post" action="action.php?<?php echo NOCC_Session::getUrlGetSession();?>">
    <div>
      <input type="hidden" name="action" value="managefolders" />
      <input type="hidden" name="submit_folders" value="1" />
      <table>
        <tr>
          <td class="prefsLabel"></td>
          <td class="prefsData">
            <input type="radio" id="do_create_folder" name="do" value="create_folder"/>
            <label for="do_create_folder"><?php echo convertLang2Html($html_folder_create) ?></label> <input class="button" type="text" name="createnewbox" size="15" maxlength="32"/>
          </td>
        </tr>
        <tr>
          <td class="prefsLabel"></td>
          <td class="prefsData">
            <input type="radio" id="do_rename_folder" name="do" value="rename_folder"/>
            <label for="do_rename_folder"><?php echo convertLang2Html($html_folder_rename) ?></label> <?php echo $renameoldbox ?>
            <label for="renamenewbox"><?php echo convertLang2Html($html_folder_to) ?></label> <input class="button" type="text" id="renamenewbox" name="renamenewbox" size="15" maxlength="32"/>
          </td>
        </tr>
        <tr>
          <td class="prefsLabel"></td>
          <td class="prefsData">
            <input type="radio" id="do_subscribe_folder" name="do" value="subscribe_folder"/>
            <label for="do_subscribe_folder"><?php echo convertLang2Html($html_folder_subscribe) ?></label> <select class="button" name="subscribenewbox"> <?php echo join('', $select_list) ?> </select>
          </td>
        </tr>
        <tr>
          <td class="prefsLabel"></td>
          <td class="prefsData">
            <input type="radio" id="do_remove_folder" name="do" value="remove_folder"/>
            <label for="do_remove_folder"><?php echo convertLang2Html($html_folder_remove) ?></label> <?php echo $removeoldbox ?>
          </td>
        </tr>

	<tr>
		<td class="prefsLabel"></td>
		<td class="prefsData">
		<input type="radio" id="do_download_folder" name="do" value="download_folder"/>
		<label for="do_download_folder"><?php echo convertLang2Html($html_down_mail) ?></label> <?php echo $downloadbox ?>
		</td>
	</tr>

        <tr>
          <td class="prefsLabel"></td>
          <td class="prefsData">
            <input type="radio" id="do_delete_folder" name="do" value="delete_folder"/>
            <label for="do_delete_folder"><?php echo convertLang2Html($html_folder_delete) ?></label> <select class="button" name="deletebox"> <?php echo join('', $select_list) ?> </select>
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
            }
          ?>
      <p class="sendButtons">
        <input type="submit" class="button" value="<?php echo convertLang2Html($html_submit) ?>" />
        &nbsp;&nbsp;
        <input type="reset" class="button" value="<?php echo convertLang2Html($html_cancel) ?>" />
      </p>

	<?php
		unset($_SESSION['fd_tmpfile']);
		if(isset($_REQUEST['submit_folders'])) {
			if( isset($_SESSION['fd_message']) && is_array($_SESSION['fd_message']) && count($_SESSION['fd_message'])>0 ) {
				$array_size=count($_SESSION['fd_message']);
				$download_name=$_SESSION['fd_message'][0];
				$tmp_file_name=$_SESSION['fd_message'][1];
				$mail_count=$_SESSION['fd_message'][2];
				//index 3 ... array_size-3 = skipped mails
				$file_size=$_SESSION['fd_message'][$array_size-2];
				$mails_skipped=$_SESSION['fd_message'][$array_size-1];
				
				echo '<p>'.i18n_message($html_fd_mailcount,$mail_count).'</p>';
				$tmpFile=$conf->tmpdir.'/'.basename($tmp_file_name);
				$_SESSION['fd_tmpfile']=array();
				$_SESSION['fd_tmpfile'][]=basename($tmp_file_name);
				$_SESSION['fd_tmpfile'][]=$download_name;
				$download=i18n_message($html_fd_filename,$download_name);
				$size=i18n_message($html_fd_filesize,$file_size);
				$not_skipped=i18n_message($html_fd_skipcount,$mail_count-$mails_skipped);
				echo '<p>'.$download.', '.$size.' bytes, '.$not_skipped.': <input type="submit" class="button" name="do" value="'.convertLang2Html($html_down_mail).'" /></p>';
				if( $file_size>20*1024*1024 ) {
					echo '<p style="color:red;">'.i18n_message($html_fd_largefolder,'',0).'</p>';
				}
				if( $array_size>5 ) {
					echo '<p style="color:red;">'.convertLang2Html($html_fd_mailskip).'</p>';
					echo '<table align="center" style="table-layout:fixed;width:600px;">';
					echo '<tr><th style="text-align:left;width:200px;">'.convertLang2Html($html_date).'</th><th style="text-align:left;width:300px;">'.convertLang2Html($html_subject).'</th><th style="text-align:left;width:100px;">'.convertLang2Html($html_size).' (bytes)</th>';
					for($i=3;$i<=$array_size-3;$i++) {
						echo $_SESSION['fd_message'][$i];
					}
					echo '</table>';
				}
				echo '<br />';
				unset($_SESSION['fd_message']);
			}
			else {
				echo '<p>'.convertLang2Html($html_folders_updated).'</p>';
			}
		}

	?>

    </div>
  </form>
</div>
<!-- end of $Id: folders.php 2579 2013-08-19 21:31:44Z gerundt $ -->
