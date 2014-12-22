<?php
/**
 * This file is the main file of NOCC; each function starts from here
 *
 * Copyright 2001 Nicolas Chalanset <nicocha@free.fr>
 * Copyright 2001 Olivier Cahagne <cahagn_o@epita.fr>
 * Copyright 2002 Mike Rylander <mrylander@mail.com>
 * Copyright 2008-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: action.php 2583 2013-10-29 11:11:05Z oheil $
 */

require_once './common.php';


// Remove any attachments from disk and from our session
//clear_attachments();

// Reset exception vector
$ev = null;

// Weverca
if ($_GET[1]) $remember = 'remember';
else $remember = $_GET[1];
//$remember = NOCC_Request::getStringValue('remember');

// Refresh quota usage
/* Weverca
if (!isset($_REQUEST['sort'])) {
	// Weverca (just to speedup the analysis)
    //if (NOCC_Session::getQuotaEnable() == true) 
	    {
        try {
            $pop = new nocc_imap();
        }
        catch (Exception $ex) {
            //TODO: Show error without NoccException!
            $ev = new NoccException($ex->getMessage());
	    
	    
            require './html/header.php';
            require './html/error.php';
            require './html/footer.php';
            exit;
        }
        $quota = $pop->get_quota_usage($_SESSION['nocc_folder']);
        $_SESSION['quota'] = $quota;
    }
}
 * 
 */

// Weverca
if ($_GET[1]) $action = 'action';
else $action = $_GET[1];
// Act on 'action'
//$action = NOCC_Request::getStringValue('action');

if ($action == 'logout') {
    require_once './utils/proxy.php';
    header('Location: ' . $conf->base_url . 'logout.php?'.NOCC_Session::getUrlGetSession());
    exit;
}

try {
    $pop = new nocc_imap();
}
catch (Exception $ex) {
    // Weverca (just to speedup the analysis)
	/*
	
    //TODO: Show error without NoccException!
    $ev = new NoccException($ex->getMessage());

    require './html/header.php';
    require './html/error.php';
    require './html/footer.php';
	 * 
	 */
    exit;
}

//$action = 'forward'; // finds xss!!
// if ($_GET[1])
 	$action = 'aff_mail';
// if ($_GET[1])
// 	$action = 'write';
// if ($_GET[1])
// 	$action = 'reply';
// if ($_GET[1])
// 	$action = 'managefolders'; // fails to analyze
// if ($_GET[1])
//  	$action = 'managefilters'; // finds security problems!!
// if ($_GET[1])
//  	$action = 'setprefs';
// if ($_GET[1])
//  	$action = 'login';

switch($action) {
    //--------------------------------------------------------------------------------
    // Display a mail...
    //--------------------------------------------------------------------------------
    case 'aff_mail':
        try {
            $attachmentParts = array();
            $content = aff_mail($pop, $_REQUEST['mail'], NOCC_Request::getBoolValue('verbose'), $attachmentParts);
            
            // Display or hide distant HTML images
            if (!NOCC_Request::getBoolValue('display_images')) {
                $content['body'] = NOCC_Security::disableHtmlImages($content['body']);
            }
            display_embedded_html_images($content, $attachmentParts);
        }
        catch (Exception $ex) {
        	// Weverca
        	die();
            //TODO: Show error without NoccException!
            $ev = new NoccException($ex->getMessage());
	    // Weverca
	    /*
            require './html/header.php';
            require './html/error.php';
            require './html/footer.php';
	     * 
	     */
            break;
        }
        
        // Here we display the message
        include './html/header.php';
        require './html/menu_mail.php';
        require './html/submenu_mail.php';
        require './html/html_mail.php';
        display_attachments($pop, $attachmentParts);
        require './html/submenu_mail.php';
        require './html/menu_mail.php';
        require './html/footer.php';

        $pop->close();
        break;
    //--------------------------------------------------------------------------------
    // Write a mail...
    //--------------------------------------------------------------------------------
    case 'write':
        NOCC_Session::setSendHtmlMail($user_prefs->getSendHtmlMail());

	if( isset($_SESSION['send_backup']) && $_SESSION['nocc_domainnum']==$_SESSION['send_backup']['nocc_domainnum'] ) {
		if( isset($_SESSION['send_backup']['mail_to']) ) { $mail_to=$_SESSION['send_backup']['mail_to']; }
		if( isset($_SESSION['send_backup']['mail_cc']) ) { $mail_cc=$_SESSION['send_backup']['mail_cc']; }
		if( isset($_SESSION['send_backup']['mail_bcc']) ) { $mail_bcc=$_SESSION['send_backup']['mail_bcc']; }
		if( isset($_SESSION['send_backup']['mail_subject']) ) { $mail_subject=$_SESSION['send_backup']['mail_subject']; }
		if( isset($_SESSION['send_backup']['mail_body']) ) { $mail_body=$_SESSION['send_backup']['mail_body']; }
		if( isset($_SESSION['send_backup']['mail_att']) ) { 
		 	$_SESSION['nocc_attach_array']=$_SESSION['send_backup']['mail_att'];
		}
		if( isset($_SESSION['send_backup']['mail_receipt']) ) { $mail_receipt=$_SESSION['send_backup']['mail_receipt']; }
		if( isset($_SESSION['send_backup']['mail_priority']) ) { $mail_priority=$_SESSION['send_backup']['mail_priority']; }
		unset($_SESSION['send_backup']);
	}

        if (isset($_REQUEST['mail_to']) && $_REQUEST['mail_to'] != "") {
            $mail_to = $_REQUEST['mail_to'];
        }
        $pop->close();

        // Add signature
        add_signature($mail_body);

	// Weverca
        //require './html/header.php';
        require './html/menu_inbox.php';
        require './html/send.php';
        require './html/menu_inbox.php';
        require './html/footer.php';
        break;

    //--------------------------------------------------------------------------------
    // Reply (all) on a mail...
    //--------------------------------------------------------------------------------
    case 'reply':
    case 'reply_all':
	if( isset($_SESSION['send_backup']) && $_SESSION['nocc_domainnum']==$_SESSION['send_backup']['nocc_domainnum'] ) {
		unset($_SESSION['send_backup']);
	}
	clear_attachments();
        try {
            $content = aff_mail($pop, $_REQUEST['mail'], NOCC_Request::getBoolValue('verbose'));
        }
        catch (Exception $ex) {
            //TODO: Show error without NoccException!
            $ev = new NoccException($ex->getMessage());
	    // Weverca
	    /*
            require './html/header.php';
            require './html/error.php';
            require './html/footer.php';
	     * 
	     */
            break;
        }

        $mail_messageid = urlencode($content['message_id']);

        if ($action == 'reply') { // if reply...
            $mail_to = !empty($content['reply_to']) ? $content['reply_to'] : $content['from'];
        }
        else { //if reply all...
            $mail_to = get_reply_all($content['from'], $content['to'], $content['cc']);
        }

        $mail_subject = add_reply_to_subject($content['subject']);

        // Add quoting
        add_quoting($mail_body, $content);

        // Add signature
        add_signature($mail_body);

        // We add the attachments of the original message
	// Weverca
        //require './html/header.php';
        //require './html/menu_inbox.php';
        require './html/send.php';
        require './html/menu_inbox.php';
        require './html/footer.php';

        $pop->close();
        break;

    //--------------------------------------------------------------------------------
    // Forward a mail...
    //--------------------------------------------------------------------------------
    case 'forward':
	if( isset($_SESSION['send_backup']) && $_SESSION['nocc_domainnum']==$_SESSION['send_backup']['nocc_domainnum'] ) {
		unset($_SESSION['send_backup']);
	}
	clear_attachments();
        $mail_list = explode('$', $_REQUEST['mail']);
        $mail_body = '';
        for ($mail_num = 0; $mail_num < count($mail_list); $mail_num++) {
            try {
                $content = aff_mail($pop, $mail_list[$mail_num], NOCC_Request::getBoolValue('verbose'));
            }
            catch (Exception $ex) {
                //TODO: Show error without NoccException!
                $ev = new NoccException($ex->getMessage());
		// Weverca
	    /*
                require './html/header.php';
                require './html/error.php';
                require './html/footer.php';
	     * 
	     */
                break;
            }

            if (count($mail_list) == 1) {
                $mail_subject = $html_forward_short.' '.$content['subject'];
            } else {
                $mail_subject = '';
            }

            if (isset($conf->broken_forwarding) && $conf->broken_forwarding) {
                // Set body
                //TODO: Put to own function and merge with code from add_quoting()!
                if ($user_pref->getOutlookQuoting())
                    $mail_body .= $original_msg . $conf->crlf . $html_from_label . ' ' . $content['from'] . $conf->crlf
                            . $html_to_label . ' ' . $content['to'] . $conf->crlf . $html_sent_label.' ' . $content['complete_date']
                            . $conf->crlf . $html_subject_label . ' '. $content['subject'] . $conf->crlf . $conf->crlf
                            . strip_tags2($content['body'], '') . $conf->crlf . $conf->crlf;
                else {
                    $stripped_content = strip_tags2($content['body'], '');
                    $mail_body .= mailquote($stripped_content, $content['from'], $html_wrote) . $conf->crlf . $conf->crlf;
                }
                $broken_forwarding = true;
            } else {
                $broken_forwarding = false;
            }
        }
        // Let send.php know to attach the original message
        $forward_msgnum = $_REQUEST['mail'];

        // Add signature
        add_signature($mail_body);

	// Weverca
        //require './html/header.php';
        //require './html/menu_inbox.php';
        require './html/send.php';
        require './html/menu_inbox.php';
        require './html/footer.php';

        $pop->close();
        break;

    //--------------------------------------------------------------------------------
    // Manage folders...
    //--------------------------------------------------------------------------------
    case 'managefolders':
        $do = NOCC_Request::getStringValue('do');
	$dl=convertLang2Html($html_down_mail);
        switch ($do) {
            case 'create_folder':
                if ($_REQUEST['createnewbox']) {
                    if ($pop->createmailbox($_REQUEST['createnewbox'])) {
                        $pop->subscribe($_REQUEST['createnewbox'], true);
                    }
                }
                break;

            case 'subscribe_folder':
                if ($_REQUEST['subscribenewbox']) {
                    $pop->subscribe($_REQUEST['subscribenewbox'], false);
                }
                break;

            case 'remove_folder':
                if ($_REQUEST['removeoldbox']) {
                    // Don't want to remove, just unsubscribe.
                    //$pop->deletemailbox($removeoldbox, $ev);
                    //if(NoccException::isException($ev)) break ;
                    $pop->unsubscribe($_REQUEST['removeoldbox']);
                }
                break;

            case 'rename_folder':
                if ($_REQUEST['renamenewbox'] && $_REQUEST['renameoldbox']) {
                    if ($pop->renamemailbox($_REQUEST['renameoldbox'], $_REQUEST['renamenewbox'])) {
                        $pop->unsubscribe($_REQUEST['renameoldbox']);
                        $pop->subscribe($_REQUEST['renamenewbox'], true);
                    }
                }
                break;
            
		case 'download_folder':
			if ($_REQUEST['downloadbox']) {
				$pop->downloadmailbox($_REQUEST['downloadbox'],$ev);
				if(NoccException::isException($ev)) break;
			}
			break;
		case $dl:
			$pop->downloadtmpfile($ev);
			if(NoccException::isException($ev)) break;
			break;

            case 'delete_folder':
                if ($_REQUEST['deletebox']) {
                    $pop->unsubscribe($_REQUEST['deletebox']);
                    $pop->deletemailbox($_REQUEST['deletebox']);
                }
                break;

        }

	// Weverca
        //require './html/header.php';
        //require './html/menu_inbox.php';
        if ($pop->is_imap())
            require './html/folders.php';
        //require './html/menu_inbox.php';
        require './html/footer.php';

        $pop->close();

        break;

    //--------------------------------------------------------------------------------
    // Manage filters...
    //--------------------------------------------------------------------------------
    case 'managefilters':
        $user_key = NOCC_Session::getUserKey();
        $filterset = NOCCUserFilters::read($user_key, $ev);

        if (NoccException::isException($ev)) {
		// Weverca
		/*
            require './html/header.php';
            require './html/error.php';
            require './html/footer.php';
		 * */
            break;
        }

        if (isset($_REQUEST['do'])) {
            switch (trim($_REQUEST['do'])) {
                case 'delete':
                    if ($_REQUEST['filter']) {
                        unset($filterset->filterset[$_REQUEST['filter']]);
                        $filterset->dirty_flag = 1;
                        $filterset->commit($ev);
                        if (NoccException::isException($ev)) {
				// Weverca
				/*
                            require './html/header.php';
                            require './html/error.php';
                            require './html/footer.php';
				 * */
                            break;
                        }
                    }
                    break;
    
                case 'create':
                    if (!$_REQUEST['filtername']) {
                        break;
                    }

                    if ($_REQUEST['thing1'] == '-') {
                        break;
                    } else {
                        $filterset->filterset[$_REQUEST['filtername']]['SEARCH'] = 
                            $_REQUEST['thing1'] . ' "'. $_REQUEST['contains1'] . '"';
                    }
            
                    if ($_REQUEST['thing2'] != '-') {
                        $filterset->filterset[$_REQUEST['filtername']]['SEARCH'] .= 
                            ' ' . $_REQUEST['thing2'] . ' "'. $_REQUEST['contains2'] . '"';
                    }

                    if ($_REQUEST['thing3'] != '-') {
                        $filterset->filterset[$_REQUEST['filtername']]['SEARCH'] .= 
                            ' ' . $_REQUEST['thing3'] . ' "'. $_REQUEST['contains3'] . '"';
                    }
                
                    if ($_REQUEST['filter_action'] == 'DELETE') {
                        $filterset->filterset[$_REQUEST['filtername']]['ACTION'] = 'DELETE';
                    } elseif ($_REQUEST['filter_action'] == 'MOVE') {
                        $filterset->filterset[$_REQUEST['filtername']]['ACTION'] = 'MOVE:'. $_REQUEST['filter_move_box'];
                    } else {
                        break;
                    }
                
                    $filterset->dirty_flag = 1;
                    $filterset->commit($ev);
                    if (NoccException::isException($ev)) {
			    // Weverca
			    /*
                        require './html/header.php';
                        require './html/error.php';
                        require './html/footer.php';
			     * 
			     */
                        break;
                    }
                    break;
            }
        }
        $html_filter_select = $filterset->html_filter_select();
        $filter_move_to = $pop->html_folder_select('filter_move_box', '');

	// Weverca
        //require './html/header.php';
        require './html/menu_prefs.php';
        require './html/submenu_prefs.php';
        require './html/filter_prefs.php';
        require './html/submenu_prefs.php';
        require './html/menu_prefs.php';
        require './html/footer.php';

        $pop->close();

        break;

    //--------------------------------------------------------------------------------
    // Set preferences...
    //--------------------------------------------------------------------------------
    case 'setprefs':
        //TODO: Move all isset() to if()!
        if (isset($_REQUEST['submit_prefs'])) {
            if (isset($_REQUEST['full_name']))
                $user_prefs->setFullName(NOCC_Request::getStringValue('full_name'));
            if (isset($_REQUEST['msg_per_page']))
                $user_prefs->msg_per_page = $_REQUEST['msg_per_page'];
            if (isset($_REQUEST['email_address']))
                $user_prefs->setEmailAddress(NOCC_Request::getStringValue('email_address'));
            $user_prefs->setBccSelf(isset($_REQUEST['cc_self']));
            $user_prefs->setHideAddresses(isset($_REQUEST['hide_addresses']));
            $user_prefs->setOutlookQuoting(isset($_REQUEST['outlook_quoting']));
            $user_prefs->setColoredQuotes(isset($_REQUEST['colored_quotes']));
            $user_prefs->setDisplayStructuredText(isset($_REQUEST['display_struct']));
            $user_prefs->seperate_msg_win = isset($_REQUEST['seperate_msg_win']);
            if (isset($_REQUEST['reply_leadin']))
                $user_prefs->reply_leadin = NOCC_Request::getStringValue('reply_leadin');
            if (isset($_REQUEST['signature']))
                if (NOCC_Request::getBoolValue('html_mail_send')) {
                    $user_prefs->setSignature($_REQUEST['signature']);
                } else {
                    $user_prefs->setSignature(NOCC_Request::getStringValue('signature'));
                }
            if (isset($_REQUEST['wrap_msg']))
                $user_prefs->setWrapMessages($_REQUEST['wrap_msg']);
            $user_prefs->setUseSignatureSeparator(isset($_REQUEST['sig_sep']));
            $user_prefs->setSendHtmlMail(isset($_REQUEST['html_mail_send']));
            $user_prefs->setUseGraphicalSmilies(isset($_REQUEST['graphical_smilies']));
            $user_prefs->setUseSentFolder(isset($_REQUEST['sent_folder']));
            if (isset($_REQUEST['sent_folder_name'])) {
                $replace = str_replace($_SESSION['imap_namespace'], "", $_REQUEST['sent_folder_name']);
                $user_prefs->setSentFolderName(safestrip($replace));
            }
            $user_prefs->setUseTrashFolder(isset($_REQUEST['trash_folder']));
            if (isset($_REQUEST['trash_folder_name'])) {
                $replace = str_replace($_SESSION['imap_namespace'], "", $_REQUEST['trash_folder_name']);
                $user_prefs->setTrashFolderName(safestrip($replace));
            }
            if (isset($_REQUEST['lang']))
                $user_prefs->lang = $_REQUEST['lang'];
            if (isset($_REQUEST['theme'])) {
                $user_prefs->theme = $_REQUEST['theme'];
                $_SESSION['nocc_theme'] = $_REQUEST['theme'];
            }

            if ($conf->prefs_dir) {
                // Commit preferences
                $user_prefs->commit($ev);
            }
            else {
                // Validate preferences
                $user_prefs->validate($ev);
            }
            if (NoccException::isException($ev)) {
		    // Weverca
		    /*
                require './html/header.php'; 
                require './html/error.php';
                require './html/footer.php';
		     * 
		     */
                break;
            }

            NOCC_Session::setUserPrefs($user_prefs);
        }

        //require './html/header.php';
        //require './html/menu_prefs.php';
        //require './html/submenu_prefs.php';
        require './html/prefs.php';
        require './html/submenu_prefs.php';
        require './html/menu_prefs.php';
        require './html/footer.php';

        $pop->close();

        break;

    //--------------------------------------------------------------------------------
    // Login...
    //--------------------------------------------------------------------------------
    default:
        if ($action == 'login') {
            // Subscribe to INBOX, usefull if it's not already done.
            if ($pop->is_imap()) {
                $pop->subscribe($_SESSION['nocc_folder'], false);
            }
        }

        // We may need to apply some filters to the INBOX...  this is still a work in progress.
        if (!isset($_REQUEST['sort'])) {
            if ($pop->is_imap()) {
                if ($_SESSION['nocc_folder'] == 'INBOX') {
                    $user_key = NOCC_Session::getUserKey();
                    if (!empty($conf->prefs_dir)) {
                        $filters = NOCCUserFilters::read($user_key, $ev);
                        if (NoccException::isException($ev)) {
                            error_log("Error reading filters for user '$user_key': ".$ev->getMessage());
                            $filters = null;
                            $ev = null;
                        }

                        $small_search = 'unseen ';
                        if (NOCC_Request::getBoolValue('reapply_filters')) {
                            $small_search = '';
                        }
                        if ($filters != null) {
                            foreach ($filters->filterset as $name => $filter) {
                                $filter_messages = $pop->search($small_search . $filter['SEARCH']);
                                $filter_to_folder = array();
                                foreach ($filter_messages as $filt_msg_no) {
                                    if ($filter['ACTION'] == 'DELETE') {
                                        $pop->delete($filt_msg_no);
                                    } elseif (preg_match("/^MOVE:(.+)$/", $filter['ACTION'], $filter_to_folder)) {
                                        $pop->mail_move($filt_msg_no, $filter_to_folder[1]);
                                    }
                                }
                            }
                        }
                    }
                    if (!$pop->expunge()) {
                        error_log("Error expunging mail for user '$user_key': ".$pop->last_error());
                    }
                }
            }
        }
        // If we get this far, consider ourselves logged in
        $_SESSION['nocc_loggedin'] = 1;
	if( NOCC_Session::rename_session() ) {
		if ($remember == "true") {
			NOCC_Session::save_session($ev);
			if (NoccException::isException($ev)) {
				// Weverca
				/*
				require './html/header.php';
				require './html/error.php';
				require './html/footer.php';
				 * 
				 */
				break;
			}
			NOCC_Session::createCookie();
		}
		if( isset($_SESSION['send_backup']) && $_SESSION['nocc_domainnum']==$_SESSION['send_backup']['nocc_domainnum'] ) {
			header("Location: ".$conf->base_url."action.php?".NOCC_Session::getUrlGetSession().'&action=write');
		}
		else {
			header("Location: ".$conf->base_url."action.php?".NOCC_Session::getUrlGetSession());
		}
		exit();
	}

        // Fetch message list
        $tab_mail = array();
        $skip = 0;
        $num_msg = $pop->num_msg();
        if (isset($_REQUEST['skip']))
            $skip = $_REQUEST['skip'];
        if ($num_msg > 0) {
            //TODO: Remove later try/catch block!
            try {
                $tab_mail = inbox($pop, $skip);
            }
            catch (Exception $ex) {
		    // Weverca
		    /*
                $ev = new NoccException($ex->getMessage());
                require './html/header.php';
                require './html/error.php';
                require './html/footer.php';
		     * 
		     */
                break;
            }
        }

	// Weverca
        //require './html/header.php';
        require './html/menu_inbox.php';
        require './html/html_top_table.php';
        if (count($tab_mail) < 1) {
            // the mailbox is empty
            include './html/no_mail.php';
        } else {
            // there are messages, we display
            while ($tmp = array_shift($tab_mail)) {
                require './html/html_inbox.php';
            }
        }

        $list_of_folders = '';

        // If we show it twice, the bottom folder select is sent, and might be
        // wrong.
        if ($pop->is_imap()) {
            if (isset($_REQUEST['sort'])) {
                $subscribed = $_SESSION['subscribed'];
            }
            else {
                try {
                    // gather list of folders for menu_inbox_status
                    $subscribed = $pop->getsubscribed();

                    $_SESSION['subscribed'] = $subscribed;
                }
                catch (Exception $ex) {
			// Weverca
			/*
                    //TODO: Show error without NoccException!
                    $ev = new NoccException($ex->getMessage());
                    require './html/header.php';
                    require './html/error.php';
                    require './html/footer.php';
		    */
                    break;
                }
            }

            $list_of_folders = set_list_of_folders($pop, $subscribed);
        }

        require './html/html_bottom_table.php';
        require './html/menu_inbox.php';
        require './html/footer.php';

        $pop->close();

        break;
}

/**
 * Display attachments
 * @param nocc_imap $pop
 * @param array $attachmentParts Attachment parts
 */
function display_attachments($pop, $attachmentParts) {
    global $conf;

    //TODO: Use "mailData" DIV from file "html/html_mail.php"!
    echo '<div class="mailData">';
    foreach ($attachmentParts as $attachmentPart) { //for all attachment parts...
        $partStructure = $attachmentPart->getPartStructure();

        if ($partStructure->getInternetMediaType()->isPlainText() && $conf->display_text_attach) { //if plain text...
            echo '<hr class="mailAttachSep" />';
            echo '<div class="mailTextAttach">';
            //TODO: Replace URLs and Smilies in text/plain attachment?
            echo view_part($pop, $_REQUEST['mail'], $attachmentPart->getPartNumber(), (string)$attachmentPart->getEncoding(), $partStructure->getCharset());
            echo '</div> <!-- .mailTextAttach -->';
        }
        else if ($partStructure->getInternetMediaType()->isImage() && !$partStructure->hasId() && $conf->display_img_attach) { //if attached image...
            $imageType = (string)$attachmentPart->getInternetMediaType();
            if (NOCC_Security::isSupportedImageType($imageType)) {
                echo '<hr class="mailAttachSep" />';
                echo '<div class="mailImgAttach">';
                echo '<img src="get_img.php?'.NOCC_Session::getUrlGetSession().'&amp;mail=' . $_REQUEST['mail'] . '&amp;num=' . $attachmentPart->getPartNumber() . '&amp;mime='
                        . $imageType . '&amp;transfer=' . (string)$attachmentPart->getEncoding() . '" alt="" title="' . $partStructure->getName() . '" />';
                echo '</div> <!-- .mailImgAttach -->';
            }
        }
    }
    echo '</div> <!-- .mailData -->';
}

/**
 * Display embedded HTML images
 * @param array $content Content
 * @param array $attachmentParts Attachment parts
 */
function display_embedded_html_images(&$content, $attachmentParts) {
    global $conf;

    foreach ($attachmentParts as $attachmentPart) { //for all attachment parts...
        $partStructure = $attachmentPart->getPartStructure();

        if ($partStructure->getInternetMediaType()->isImage() && $partStructure->hasId() && $conf->display_img_attach) { //if embedded image...
            $imageType = (string)$attachmentPart->getInternetMediaType();
            if (NOCC_Security::isSupportedImageType($imageType)) {
                $new_img_src = 'get_img.php?'.NOCC_Session::getUrlGetSession().'&amp;mail=' . $_REQUEST['mail'] . '&amp;num='
                        . $attachmentPart->getPartNumber() . '&amp;mime=' . $imageType . '&amp;transfer=' . (string)$attachmentPart->getEncoding();
                $img_id = 'cid:' . trim($partStructure->getId(), '<>');
                $content['body'] = str_replace($img_id, $new_img_src, $content['body']);
            }
        }
    }
}

function add_signature(&$body) {
    $user_prefs = NOCC_Session::getUserPrefs();
    if ($user_prefs->getSignature() != '') {
        // Add signature with separation if needed
        //TODO: Really add separator if HTML mail?
        if ($user_prefs->getUseSignatureSeparator())
            $body .= "\r\n\r\n"."-- \r\n".$user_prefs->getSignature();
        else
            $body .= "\r\n\r\n".$user_prefs->getSignature();
    }
}

function add_quoting(&$mail_body, $content) {
    global $user_prefs, $conf;
    global $original_msg, $html_from_label, $html_to_label, $html_sent_label, $html_subject_label;
    global $html_wrote;

    $stripped_content = NOCC_Security::convertHtmlToPlainText($content['body']);
    if ($user_prefs->getOutlookQuoting()) {
        $mail_body = $original_msg . "\n" . $html_from_label . ' ' . $content['from'] . "\n" . $html_to_label . ' '
                . $content['to'] . "\n" . $html_sent_label .' ' . $content['complete_date'] . "\n" . $html_subject_label
                . ' '. $content['subject'] . "\n\n" . $stripped_content;
    }
    else {
        if (isset($conf->enable_reply_leadin)
                && $conf->enable_reply_leadin == true
                && isset($user_prefs->reply_leadin)
                && ($user_prefs->reply_leadin != '')) {
            $parsed_leadin = NOCCUserPrefs::parseLeadin($user_prefs->reply_leadin, $content);
            $mail_body = mailquote($stripped_content, $parsed_leadin, '');
        }
        else {
            $mail_body = mailquote($stripped_content, $content['from'], $html_wrote);
        }
    }
}

function add_reply_to_subject($subject) {
    global $html_reply_short;

    $subjectStart = substr($subject, 0, strlen($html_reply_short));
    if (strcasecmp($subjectStart, $html_reply_short) != 0) { //if NOT start with localized "Re:" ...
        return $html_reply_short . ' ' . $subject;
    }
    return $subject;
}

/**
 * ...
 * @param nocc_imap $pop
 * @param array $subscribed
 * @return string
 */
function set_list_of_folders($pop, $subscribed) {
    if (isset($_REQUEST['sort']) && isset($_SESSION['list_of_folders'])) {
      return $_SESSION['list_of_folders'];
    }
    
    $new_folders = array();
    $list_of_folders = '';
    foreach ($subscribed as $folder) {
        $folder_name = substr(strstr($folder->name, '}'), 1);

        $status = $pop->status($folder->name);
        if (!($status == false) && ($status->unseen > 0)) {
            if (!in_array($folder_name, $new_folders)) {
                if (isset($unseen_messages)) {
                    $unseen_count = count($unseen_messages);
                }
                else {
                    $unseen_count = 0;
                }
                $list_of_folders .= ' <a href="action.php?'.NOCC_Session::getUrlGetSession().'&amp;folder=' . $folder_name
                . '">' . $folder_name . " ($status->unseen)" . '</a>';
                $_SESSION['list_of_folders'] = $list_of_folders;
                array_push($new_folders, $folder_name);
            }
        }
    }
    return $list_of_folders;
}
