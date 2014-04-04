<?php
/**
 * Logout
 *
 * Copyright 2001 Nicolas Chalanset <nicocha@free.fr>
 * Copyright 2001 Olivier Cahagne <cahagn_o@epita.fr>
 * Copyright 2008-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: logout.php 2583 2013-10-29 11:11:05Z oheil $
 */

require_once './classes/nocc_session.php';

NOCC_Session::start();

if( isset($_SESSION['send_backup']) ) {
	$send_backup=$_SESSION['send_backup'];
}

if (file_exists('./config/conf.php')) {
    require_once './config/conf.php';
    
    // code extraction from conf.php, legacy code support
    if ((file_exists('./utils/config_check.php')) && (!function_exists('get_default_from_address'))) {
        require_once './utils/config_check.php';
    }
}
else {
    print("The main configuration file (./config/conf.php) couldn't be found! <p />Please rename the file './config/conf.php.dist' to './config/conf.php'. ");
    die();
}

require_once './utils/functions.php';
clear_attachments();
NOCC_Session::remove_session_file();
NOCC_Session::destroy();
NOCC_Session::start();
if( isset($send_backup) ) {
	$_SESSION['send_backup']=$send_backup;
}
require_once './utils/proxy.php';
Header('Location: ' . $conf->base_url . 'index.php?'.NOCC_Session::getUrlGetSession());

