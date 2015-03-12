<?php
/**
 * This file just delete the selected message(s)
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
 * @version    SVN: $Id: delete.php 2580 2013-08-19 21:57:33Z gerundt $
 */

require_once './common.php';
require_once './classes/class_local.php';

$ev = "";
try {
    $pop = new nocc_imap();
}
catch (Exception $ex) {
    //TODO: Show error without NoccException!
    $ev = new NoccException($ex->getMessage());
    require './html/header.php';
    require './html/error.php';
    require './html/footer.php';
    return;
}

$num_messages = $pop->num_msg();
$url = "action.php?".NOCC_Session::getUrlGetSession();
$user_prefs = NOCC_Session::getUserPrefs();

// Work out folder and target_folder
$folder = $_SESSION['nocc_folder'];
$target_folder = "";
if (isset($_REQUEST['target_folder']) && $_REQUEST['target_folder'] != $folder)
    $target_folder = $_REQUEST['target_folder'];

if (isset($_REQUEST['bottom_target_folder']) && $_REQUEST['bottom_target_folder'] != $folder)
    $bottom_target_folder = $_REQUEST['bottom_target_folder'];

if (isset($_REQUEST['only_one'])) {
    $mail = $_REQUEST['mail'];
    if (isset($_REQUEST['move_mode'])) {
        if ($target_folder != $folder) {
            $pop->mail_move($mail, $target_folder);
        }
    }
    if (isset($_REQUEST['copy_mode'])) {
        if ($target_folder != $folder) {
            $pop->mail_copy($mail, $target_folder);
        }
    }
    if (isset($_REQUEST['delete_mode'])) {
        // If messages are opened in a new windows, we will reload the opener window
        // i.e. the one with messages list
        $_SESSION['message_deleted'] = "true";
        $target_folder = $_SESSION['imap_namespace'].$user_prefs->getTrashFolderName();
        if ($pop->is_imap()
                && $user_prefs->getUseTrashFolder()
                && $_SESSION['nocc_folder'] != $target_folder ) {
            $pop->mail_move($mail, $target_folder);
        } else {
            $pop->delete($mail);
        }
        if ($mail - 1) {
            $url = "action.php?".NOCC_Session::getUrlGetSession()."&action=aff_mail&mail=".--$mail."&verbose=0";
        }
        else {
            $url = "action.php?".NOCC_Session::getUrlGetSession();
        }
    }
} else {
    $msg_to_forward = '';
    for ($i = $num_messages; $i >= 1 ; $i--) {

        if (isset($_REQUEST['msg-'.$i])) {
            if (isset($_REQUEST['move_mode'])) {
                if ($target_folder != $folder) {
                    $pop->mail_move($i, $target_folder);
                }
            }
            if (isset($_REQUEST['bottom_move_mode'])) {
                if ($bottom_target_folder != $folder) {
                    $pop->mail_move($i, $bottom_target_folder);
                }
            }
            if (isset($_REQUEST['copy_mode'])) {
                if ($target_folder != $folder) {
                    $pop->mail_copy($i, $target_folder);
                }
            }
            if (isset($_REQUEST['bottom_copy_mode'])) {
                if ($bottom_target_folder != $folder) {
                    $pop->mail_copy($i, $bottom_target_folder);
                }
            }
            if (isset($_REQUEST['forward_mode']) || isset($_REQUEST['bottom_forward_mode'])) {
                $msg_to_forward .= '$'.$i;
            }
            if (isset($_REQUEST['delete_mode']) || isset($_REQUEST['bottom_delete_mode'])) {
                // If messages are opened in a new windows, we will reload the opener window
                // i.e. the one with messages list
                $_SESSION['message_deleted'] = "true";
                $target_folder = $_SESSION['imap_namespace'].$user_prefs->getTrashFolderName();
                if ($pop->is_imap()
                        && $user_prefs->getUseTrashFolder()
                        && $_SESSION['nocc_folder'] != $target_folder ) {
                    $pop->mail_move($i, $target_folder);
                } else {
                    $pop->delete($i);
                }
            }
            if (isset($_REQUEST['mark_read_mode']) && $_REQUEST['mark_mode'] == 'read') {
                $pop->mail_mark_read($i);
            }
            if (isset($_REQUEST['bottom_mark_read_mode']) && $_REQUEST['bottom_mark_mode'] == 'read') {
                $pop->mail_mark_read($i);
            }
            if (isset($_REQUEST['mark_read_mode']) && $_REQUEST['mark_mode'] == 'unread') {
                $pop->mail_mark_unread($i);
            }
            if (isset($_REQUEST['bottom_mark_read_mode']) && $_REQUEST['bottom_mark_mode'] == 'unread') {
                $pop->mail_mark_unread($i);
            }
        }
    }
    if ($msg_to_forward != '') {
      $msg_to_forward = substr($msg_to_forward, 1);
      $url = 'action.php?'.NOCC_Session::getUrlGetSession().'&action=forward&mail='.$msg_to_forward;
    }
}

$pop->close();

if (NoccException::isException($ev)) {
    require './html/header.php';
    require './html/error.php';
    require './html/footer.php';
    return;
}

// Redirect user to index
// TODO: redirect user to next message
require_once './utils/proxy.php';
header('Location: ' . $conf->base_url . $url);