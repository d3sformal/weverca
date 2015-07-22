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

if ($_GET[1]) $remember = 'remember';
else $remember = $_GET[1];


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
    exit;
}

$action = 'aff_mail';
				
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
        	die();
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

        if (isset($_REQUEST['mail_to']) && $_REQUEST['mail_to'] != "") {
            $mail_to = $_REQUEST['mail_to'];
        }
        $pop->close();

        // Add signature
        add_signature($mail_body);

        //require './html/header.php';
        require './html/menu_inbox.php';
        require './html/send.php';
        require './html/menu_inbox.php';
        require './html/footer.php';
        break;

    //--------------------------------------------------------------------------------
    // Manage filters...
    //--------------------------------------------------------------------------------
    case 'managefilters':
        $user_key = NOCC_Session::getUserKey();
        $filterset = NOCCUserFilters::read($user_key, $ev);

        if (NoccException::isException($ev)) {
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
                        break;
                    }
                    break;
            }
        }
        $html_filter_select = $filterset->html_filter_select();
        $filter_move_to = $pop->html_folder_select('filter_move_box', '');

        //require './html/header.php';
        require './html/menu_prefs.php';
        require './html/submenu_prefs.php';
        require './html/filter_prefs.php';
        require './html/submenu_prefs.php';
        require './html/menu_prefs.php';
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
