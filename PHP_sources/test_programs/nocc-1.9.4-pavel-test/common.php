<?php
/**
 * Stuff that is always checked or run or initialised for every hit
 *
 * Copyright 2002 Ross Golder <ross@golder.org>
 * Copyright 2008-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: common.php 2583 2013-10-29 11:11:05Z oheil $
 */

define('NOCC_DEBUG_LEVEL', 0);
if (NOCC_DEBUG_LEVEL > 0) {
    define('NOCC_START_TIME', microtime(true));
}

// Define variables
if (!isset($from_rss)) { $from_rss=false; }

if (file_exists('./config/conf.php')) {
    require_once './config/conf.php';
    
    // code extraction from conf.php, legacy code support
    if ((file_exists('./utils/config_check.php')) && (!function_exists('get_default_from_address'))) {
    require_once './utils/config_check.php';
    }
}
else {
    //TODO: Make error msg translateble and show nicer error...
    print("The main configuration file (./config/conf.php) couldn't be found! <p />Please rename the file './config/conf.php.dist' to './config/conf.php'. ");
    die();
}


require_once './classes/nocc_request.php';
require_once './classes/nocc_session.php';
require_once './classes/nocc_security.php';
require_once './classes/nocc_body.php';
require_once './classes/nocc_languages.php';
require_once './classes/nocc_themes.php';
require_once './classes/nocc_domain.php';
require_once './classes/nocc_attachedfile.php';
require_once './classes/user_prefs.php';
require_once './classes/user_filters.php';
require_once './utils/functions.php';
require_once './utils/crypt.php';
require_once './utils/translation.php';

$conf->nocc_name = 'NOCC';
$conf->nocc_version = '1.9.4-dev';
$conf->nocc_url = 'http://nocc.sourceforge.net/';

$pwd_to_encrypt = false;
if (isset($_REQUEST['actioSn']) && $_REQUEST['action'] == 'login') {
    $pwd_to_encrypt = true;
}

if ($from_rss == false) {
    NOCC_Session::start();
}

// Useful for debugging sessions
//echo "<pre>Session:";
//var_dump($_SESSION);
//echo "</pre>";

// Set defaults
// Weverca: remove tests for isset for cases that the tested variable is set in the if branch. Multiple occurences!!!!
//if (isset($_REQUEST['folder']))
    $_SESSION['nocc_folder'] = $_REQUEST['folder'];
//if (!isset($_SESSION['nocc_folder']))
    $_SESSION['nocc_folder'] = $conf->default_folder;

// Have we changed sort order?
//if (!isset($_SESSION['nocc_sort']))
    $_SESSION['nocc_sort'] = $conf->default_sort;
//if (!isset($_SESSION['nocc_sortdir']))
    $_SESSION['nocc_sortdir'] = $conf->default_sortdir;

// Override session variables from request, if supplied
if (isset($_REQUEST['user']) && !isset($_SESSION['nocc_loggedin'])) {
    unset($_SESSION['nocc_login']);
    $_SESSION['nocc_user'] = NOCC_Request::getStringValue('user');
}
if (isset($_REQUEST['passwd'])) {
    $_SESSION['nocc_passwd'] = NOCC_Request::getStringValue('passwd');
    $pwd_to_encrypt = true;
}

if ($pwd_to_encrypt == true) {
    /* encrypt session password */
    /* store into session encrypted password */
	// Weverca
	//$_SESSION['nocc_passwd'] = encpass($_SESSION['nocc_passwd'], $conf->master_key);
}

if (isset($_REQUEST['sort']))
    $_SESSION['nocc_sort'] = NOCC_Request::getStringValue('sort');
if (isset($_REQUEST['sortdir']))
    $_SESSION['nocc_sortdir'] = NOCC_Request::getStringValue('sortdir');

//--------------------------------------------------------------------------------
// Set and load the language...
//--------------------------------------------------------------------------------
$languages = new NOCC_Languages('./lang/', $conf->default_lang);

//TODO: Check $_REQUEST['lang'] also when force_default_lang?
if (isset($_REQUEST['lang'])) { //if a language is requested...
    if ($languages->setSelectedLangId($_REQUEST['lang'])) { //if the language exists...
        $_SESSION['nocc_lang'] = $languages->getSelectedLangId();
    }
}

if (isset($_SESSION['nocc_lang'])) { //if session language already set...
    $languages->setSelectedLangId($_SESSION['nocc_lang']);
}
else { //if session language NOT already set...
    if (!isset($conf->force_default_lang) || !$conf->force_default_lang) { //if NOT force default language...
        $languages->detectFromBrowser();
    }
    $_SESSION['nocc_lang'] = $languages->getSelectedLangId();
}
$lang = $languages->getSelectedLangId();

require './lang/en.php';
if ($lang != 'en') { //if NOT English...
	$lang_file='./lang/'.basename($lang).'.php';
	if( is_file($lang_file) ) {
		require $lang_file;
	}
}
//--------------------------------------------------------------------------------

//--------------------------------------------------------------------------------
// Set the theme...
//--------------------------------------------------------------------------------
$themes = new NOCC_Themes('./themes/', $conf->default_theme);

//TODO: Check $_REQUEST['theme'] also when NOT use_theme?
if (isset($_REQUEST['theme'])) { //if a theme is requested...
    if ($themes->setSelectedThemeName($_REQUEST['theme'])) { //if the theme exists...
        $_SESSION['nocc_theme'] = $themes->getSelectedThemeName();
    }
}

if (!isset($_SESSION['nocc_theme'])) { //if session theme NOT already set...
    $_SESSION['nocc_theme'] = $themes->getDefaultThemeName();
}
//--------------------------------------------------------------------------------

// Start with default smtp server/port, override later
//if (empty($_SESSION['nocc_smtp_server']))
    $_SESSION['nocc_smtp_server'] = $conf->default_smtp_server;
//if (empty($_SESSION['nocc_smtp_port']))
    $_SESSION['nocc_smtp_port'] = $conf->default_smtp_port;

// Default login to just the username
if (isset($_SESSION['nocc_user']) && !isset($_SESSION['nocc_login']))
    $_SESSION['nocc_login'] = $_SESSION['nocc_user'];

// Check allowed chars for login
if (isset($_SESSION['nocc_login']) && $_SESSION['nocc_login'] != ''
        && isset($conf->allowed_char) && $conf->allowed_char != ''
        && !preg_match("|".$conf->allowed_char."|", $_SESSION['nocc_login'])) {
    $ev = new NoccException($html_wrong);
    require './html/header.php';
    require './html/error.php';
    require './html/footer.php';
    exit;
}

// Were we provided with a fillindomain to use?
if (isset($_REQUEST['fillindomain']) && isset( $conf->typed_domain_login )) {
    for ($count=0; $count < count($conf->domains); $count++) {
        if ($_REQUEST['fillindomain'] == $conf->domains[$count]->domain)
            $_REQUEST['domainnum'] = $count;
    }
}

// Were we provided with a domainnum to use
if (isset($_REQUEST['domainnum']) && !(isset($_REQUEST['server']))) {
    $domainnum = $_REQUEST['domainnum'];
    if (!isset($conf->domains[$domainnum])) {
        $ev = new NoccException($lang_could_not_connect);
        require './html/header.php';
        require './html/error.php';
        require './html/footer.php';
        exit;
    }
    
    $domain = new NOCC_Domain($conf->domains[$domainnum]);
    
    $_SESSION['nocc_domainnum'] = $domainnum;
    $_SESSION['nocc_domain'] = $conf->domains[$domainnum]->domain;
    $_SESSION['nocc_servr'] = $conf->domains[$domainnum]->in;
    $_SESSION['nocc_smtp_server'] = $conf->domains[$domainnum]->smtp;
    $_SESSION['nocc_smtp_port'] = $conf->domains[$domainnum]->smtp_port;
    $_SESSION['smtp_auth'] = $conf->domains[$domainnum]->smtp_auth_method;
    $_SESSION['imap_namespace'] = $conf->domains[$domainnum]->imap_namespace;
    $_SESSION['ucb_pop_server'] = $conf->domains[$domainnum]->have_ucb_pop_server;
    $_SESSION['quota_enable'] = $conf->domains[$domainnum]->quota_enable;
    $_SESSION['quota_type'] = $conf->domains[$domainnum]->quota_type;

    // Check allowed logins
    if (!$domain->isAllowedLogin($_SESSION['nocc_login'])) {
        $ev = new NoccException($html_login_not_allowed);
        require './html/header.php';
        require './html/error.php';
        require './html/footer.php';
        exit;
    }

    //Do we have login aliases?
    $_SESSION['nocc_login'] = $domain->replaceLoginAlias($_SESSION['nocc_login']);

    // Do we provide the domain with the login?
    if ($domain->useLoginWithDomain()) {
        if ($domain->hasLoginWithDomainCharacter()) {
            $_SESSION['nocc_login'] .= $domain->getLoginWithDomainCharacter() . $_SESSION['nocc_domain'];
        } else if (preg_match("|([A-Za-z0-9]+)@([A-Za-z0-9]+)|", $_SESSION['nocc_login'], $regs)) {
            $_SESSION['nocc_login'] = $_SESSION['nocc_login'];
            $_SESSION['nocc_domain'] = $regs[2];
        } else {
            $_SESSION['nocc_login'] .= '@' . $_SESSION['nocc_domain'];
        }
        $_SESSION['nocc_login_mailaddress'] = $_SESSION['nocc_login'];
        //TODO: Drop $_SESSION['nocc_login_with_domain'] first, if we drop get_default_from_address() and "config_check.php"!
        $_SESSION['nocc_login_with_domain'] = true;
    }

    //append prefix to login
    $_SESSION['nocc_login'] = $domain->addLoginPrefix($_SESSION['nocc_login']);

    //append suffix to login
    $_SESSION['nocc_login'] = $domain->addLoginSuffix($_SESSION['nocc_login']);
    
    unset($domain);
}

// Or did the user provide the details themselves
if (isset($_REQUEST['server'])) {
    $server = NOCC_Request::getStringValue('server');
    $servtype = strtolower($_REQUEST['servtype']);
    $port = NOCC_Request::getStringValue('port');
    $servr = $server.'/'.$servtype.':'.$port;

    // Use as default domain for user's address
    $_SESSION['nocc_domain'] = $server;
    $_SESSION['nocc_servr'] = $servr;
}

// Cache the user's preferences/filters
if (isset($_SESSION['nocc_user']) && isset($_SESSION['nocc_domain'])) {
    //TODO: Move to NOCC_Session::loadUserPrefs()?
    $ev = null;
    $user_key = NOCC_Session::getUserKey();

    // Preferences
    if (!NOCC_Session::existsUserPrefs()) {
        //TODO: Move to NOCC_Session::loadUserPrefs()?
        NOCC_Session::setUserPrefs(NOCCUserPrefs::read($user_key, $ev));
        if(NoccException::isException($ev)) {
            echo "<p>User prefs error ($user_key): ".$ev->getMessage()."</p>";
            exit(1);
        }
    }
    $user_prefs = NOCC_Session::getUserPrefs();

    //--------------------------------------------------------------------------------
    // Set and load the user prefs language...
    //--------------------------------------------------------------------------------
    //TODO: Move to normal language loading!
    if (isset($user_prefs->lang) && $user_prefs->lang != '') {
        $userLang = $languages->getSelectedLangId();
        if ($languages->setSelectedLangId($user_prefs->lang)) { //if the language exists...
            $userLang = $languages->getSelectedLangId();
            if (($userLang != 'en') && ($userLang != $lang)) { //if NOT English AND current language...
                $_SESSION['nocc_lang'] = $languages->getSelectedLangId();
                $lang = $languages->getSelectedLangId();
                
                require './lang/'. $lang . '.php';
            }
        }
        unset($userLang);
    }
    unset($languages);
    //--------------------------------------------------------------------------------

    //--------------------------------------------------------------------------------
    // Set the user prefs theme...
    //--------------------------------------------------------------------------------
    //TODO: Move to normal theme loading!
    if (isset($conf->use_theme) && ($conf->use_theme == true)) { //if allow theme changing...
        if (isset($user_prefs->theme) && $user_prefs->theme != '') {
            if ($themes->setSelectedThemeName($user_prefs->theme)) { //if the theme exists...
                $_SESSION['nocc_theme'] = $themes->getSelectedThemeName();
            }
        }
    }
    unset($themes);
    //--------------------------------------------------------------------------------

    // Filters
    if (!empty($conf->prefs)) {
        if (!isset($_SESSION['nocc_user_filters'])) {
            $_SESSION['nocc_user_filters'] = NOCCUserFilters::read($user_key, $ev);
            if (NoccException::isException($ev)) {
                echo "<p>User filters error ($user_key): ".$ev->getMessage()."</p>";
                exit(1);
            }
        }
        $user_filters = $_SESSION['nocc_user_filters'];
    }
}

require_once './config/conf_lang.php';
require_once './config/conf_charset.php';

// allow PHP script to consume more memory than default setting for
// big attachments
if (isset($conf->memory_limit) && $conf->memory_limit != '') {
    @ini_set("memory_limit", $conf->memory_limit);
}
