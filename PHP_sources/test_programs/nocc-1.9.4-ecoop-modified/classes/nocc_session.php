<?php
/**
 * Class for wrapping the $_SESSION array
 *
 * Copyright 2009-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: nocc_session.php 2583 2013-10-29 11:11:05Z oheil $
 */

require_once 'user_prefs.php';

/**
 * Wrapping the $_SESSION array
 *
 * @package    NOCC
 */
class NOCC_Session {

    /**
     * Start the session
     * @static
     */
    public static function start() {
    	session_start();
    }

	/**
	 * Remove old saved sessions
	 * @static
	 */
	public static function remove_old_sessions() {
		global $conf;
		if( ! isset($conf->prune_sessions) || ! $conf->prune_sessions==0 ) {
			if (!empty($conf->prefs_dir)) {
				foreach( glob($conf->prefs_dir.'/'."NOCCLI_*") as $filename) {
					$last_mod=filemtime($filename);
					$age=time()-$last_mod;
					$max_age=60*60*24*7*4;  //4 weeks
					if( $age>$max_age ) {
						unlink($filename);
					}
				}
			}
		}
	}

	/**
	 * Remove old session tmp files
	 * @static
	 */
	public static function remove_old_session_tmp_file() {
		global $conf;
		if( !empty($conf->tmpdir) && isset($_SESSION['sname']) && strlen($_SESSION['sname'])>0 ) {
			foreach( glob($conf->tmpdir.'/'.$_SESSION['sname']."_*") as $filename) {
				$sname=preg_replace("/\.session$/","",$filename);
				if( isset($_SESSION[$sname]) && $_SESSION[$sname]>0 ) {
					$_SESSION[$sname]=$_SESSION[$sname]-1;
				}
				else {
					unset($_SESSION[$sname]);
					unlink($filename);
				}
			}
		}
		if( !empty($conf->tmpdir) ) {
			foreach( glob($conf->tmpdir.'/'."NOCCLI_*") as $filename) {
				$last_mod=filemtime($filename);
				$age=time()-$last_mod;
				$max_age=60*60*1;  //1 hour
				if( $age>$max_age ) {
					unlink($filename);
				}
			}
			foreach( glob($conf->tmpdir.'/'."php*.att") as $filename) {
				$last_mod=filemtime($filename);
				$age=time()-$last_mod;
				$max_age=60*60*24*1;  //1 day
				if( $age>$max_age ) {
					unlink($filename);
				}
			}
		}
	}

	/**
	 * Get next session name
	 * @static
	 */
	public static function get_next_session_name() {
		$current_name=session_name();
		$set_next=false;
		$next_name="";
		foreach( $_COOKIE as $cookie_key => $cookie_value ) {
			if( preg_match("/^NOCCLI_/",$cookie_key) ) {
				if( $set_next ) {
					$next_name=$cookie_key;
					break;
				}
				if( $current_name==$cookie_key ) {
					$set_next=true;
				}
			}
		}
		if( strlen($next_name)==0 ) {
			$next_name='NOCC_'.md5(uniqid(rand(),true));
		}
		$next_name="sname=".$next_name;
		return $next_name;
	}

	/**
	 * Rename current session
	 * @static
	 */
	public static function rename_session() {
		$old_sname=session_name();
		if( preg_match("/^NOCC_/",$old_sname) ) {
			$sname='NOCCLI_'.md5(uniqid(rand(),true));
			session_name($sname);
			session_regenerate_id(true);
			$svalue=session_id();
			$_SESSION['sname']=$sname;
			$_SESSION['svalue']=$svalue;

			setcookie($old_sname, '', time() - 3600);
			return true;
		}
		else {
			return false;
		}
	}

	/**
	 * Start a new  session
	 * @static
	 */
	public static function new_session() {
		$sname='NOCC_'.md5(uniqid(rand(),true));
		session_name($sname);
		session_start();
		$svalue=session_id();
		$_SESSION['sname']=$sname;
		$_SESSION['svalue']=$svalue;
	}

	/**
	 * Save a session
	 * @static
	 */
	public static function save_session() {
		global $conf;
		if (!empty($conf->prefs_dir)) {
			 // generate string with session information
			unset ($cookie_string);
			$cookie_string = session_id();
			$cookie_string .= " " . $_SESSION['nocc_user'];
			$cookie_string .= " " . $_SESSION['nocc_passwd'];
			$cookie_string .= " " . $_SESSION['nocc_login'];
			$cookie_string .= " " . $_SESSION['nocc_lang'];
			$cookie_string .= " " . $_SESSION['nocc_smtp_server'];
			$cookie_string .= " " . $_SESSION['nocc_smtp_port'];
			$cookie_string .= " " . $_SESSION['nocc_theme'];
			$cookie_string .= " " . $_SESSION['nocc_domain'];
			$cookie_string .= " " . $_SESSION['nocc_domainnum'];
			$cookie_string .= " " . $_SESSION['imap_namespace'];
			$cookie_string .= " " . $_SESSION['nocc_servr'];
			$cookie_string .= " " . $_SESSION['nocc_folder'];
			$cookie_string .= " " . $_SESSION['smtp_auth'];
			$cookie_string .= " " . $_SESSION['ucb_pop_server'];
			$cookie_string .= " " . $_SESSION['quota_enable'];
			$cookie_string .= " " . $_SESSION['quota_type'];

			// encode cookie string to base64
			$cookie_string = base64_encode($cookie_string);

			// save string to file
			//$filename = $conf->prefs_dir . '/' . NOCC_Session::getUserKey() . '.session';
			$filename = $conf->prefs_dir . '/' . $_SESSION['sname'].'.session';

			if (file_exists($filename) && !is_writable($filename)) {
				$ev = new NoccException($html_session_file_error);
				return false;
			}
			if (!is_writable($conf->prefs_dir)) {
				$ev = new NoccException($html_session_file_error);
				return false;
			}
			$file = fopen($filename, 'w');
			if (!$file) {
				$ev = new NoccException($html_session_file_error);
				return false;
			}
			fwrite($file, $cookie_string . "\n");
			fclose($file);
			return true;
		}
		return false;
	}


	/**
	 * Load a saved session
	 * @static
	 */
	public static function load_session() {
		global $conf;
		if (empty($conf->prefs_dir)) {
			return false;
		}
	
		$sname=session_name();
		$filename=$conf->prefs_dir.'/'.$sname.'.session';
		if (!file_exists($filename)) {
			return false;
		}
	
		$file=fopen($filename, 'r');
		if (!$file) {
			return false;
		}
	
		$line = trim(fgets($file, 1024));
		fclose($file);

		/*
		list($session_id,$_SESSION['nocc_user'], $_SESSION['nocc_passwd'],
			$_SESSION['nocc_login'], $_SESSION['nocc_lang'],
			$_SESSION['nocc_smtp_server'], $_SESSION['nocc_smtp_port'],
			$_SESSION['nocc_theme'], $_SESSION['nocc_domain'], $_SESSION['nocc_domainnum'],
			$_SESSION['imap_namespace'], $_SESSION['nocc_servr'],
			$_SESSION['nocc_folder'], $_SESSION['smtp_auth'],
			$_SESSION['ucb_pop_server'], $_SESSION['quota_enable'],
			$_SESSION['quota_type']) = explode(" ", base64_decode($line));
		 * */
		$_SESSION['nocc_folder'] = isset($_REQUEST['nocc_folder']) ? $_REQUEST['nocc_folder'] : 'INBOX';

		if( session_id()==$session_id ) {
			return true;	
		}
		else {
			return false;
		}
	}

	/**
	 * Remove a saved session file
	 * @static
	 */
	public static function remove_session_file() {
		global $conf;
		if (empty($conf->prefs_dir)) {
			return false;
		}
		$sname=session_name();
		$filename=$conf->prefs_dir.'/'.$sname.'.session';
		if( file_exists($filename) ) {
			unlink($filename);
		}
		return true;
	}
    
    /**
     * Destroy the session
     * @param bool $forceSessionStart Force session start?
     * @static
     */
    public static function destroy($forceSessionStart = false) {
	$sname='NOCCSESSID';
	if( isset($_SESSION['sname']) && strlen($_SESSION['sname'])>0 ) {
		$sname=$_SESSION['sname'];
	}
        session_name($sname);
        if ($forceSessionStart) {
            session_start();
        }
	setcookie($sname, '', time() - 3600);

        $_SESSION = array();
        session_destroy();
    }
    
    /**
     * Create session cookie
     * @static
     */
    public static function createCookie() {
        //store cookie for thirty days
	$sname='NOCCSESSID';
	if( isset($_SESSION['sname']) && strlen($_SESSION['sname'])>0 ) {
		$sname=$_SESSION['sname'];
	}
	$svalue=session_id();
        setcookie($sname,$svalue,time()+2592000);
    }
    
    /**
     * Delete session cookie
     * @static
     */
    public static function deleteCookie() {
	$sname='NOCCSESSID';
	if( isset($_SESSION['sname']) && strlen($_SESSION['sname'])>0 ) {
		$sname=$_SESSION['sname'];
	}
        setcookie($sname, '', time() - 3600);
    }
    
    /**
     * Get the URL query from the session
     * @return string URL query
     * @static
     */
    public static function getUrlQuery() {
        #return session_name() . '=' . session_id();
	return "";
    }
    
    /**
     * Get the URL session GET part
     * @return string URL GET part
     * @static
     */
    public static function getUrlGetSession() {
	return "sname=".session_name();
    }
    
    /**
     * Get the user key from the session
     * @return string User key
     * @static
     */
    public static function getUserKey() {
        return $_SESSION['nocc_user'] . '@' . $_SESSION['nocc_domain'];
    }
    
    /**
     * Get the SMTP server from the session
     * @return string SMTP server
     * @static
     */
    public static function getSmtpServer() {
        if (isset($_SESSION['nocc_smtp_server'])) {
            return $_SESSION['nocc_smtp_server'];
        }
        return '';
    }
    
    /**
     * Set the SMTP server from the session
     * @param string $value SMTP server
     * @static
     */
    public static function setSmtpServer($value) {
        $_SESSION['nocc_smtp_server'] = $value;
    }
    
    /**
     * Get quota enabling from the session
     * @return bool Quota enabled?
     * @static
     */
    public static function getQuotaEnable() {
        if (isset($_SESSION['quota_enable']) && $_SESSION['quota_enable']) {
            return true;
        }
        return false;
    }
    
    /**
     * Set quota enabling from the session
     * @param bool $value Quota enabled?
     * @static
     */
    public static function setQuotaEnable($value) {
        $_SESSION['quota_enable'] = $value;
    }
    
    /**
     * Get quota type (STORAGE or MESSAGE) from the session
     * @return string Quota type
     * @static
     * @todo Check for STORAGE or MESSAGE?
     */
    public static function getQuotaType() {
        if (isset($_SESSION['quota_type'])) {
            return $_SESSION['quota_type'];
        }
        return 'STORAGE';
    }
    
    /**
     * Set quota type (STORAGE or MESSAGE) from the session
     * @param string $value Quota type
     * @static
     * @todo Check for STORAGE or MESSAGE?
     */
    public static function setQuotaType($value) {
        $_SESSION['quota_type'] = $value;
    }

    /**
     * Exists user preferences in the session?
     * @return boolean Exists user preferences?
     * @static
     */
    public static function existsUserPrefs() {
        if (isset($_SESSION['nocc_user_prefs'])) {
            if ($_SESSION['nocc_user_prefs'] instanceof NOCCUserPrefs) {
                return true;
            }
        }
        return false;
    }

    /**
     * Get user preferences from the session
     * @return NOCCUserPrefs User preferences
     * @static
     */
    public static function getUserPrefs() {
        if (NOCC_Session::existsUserPrefs()) {
            return $_SESSION['nocc_user_prefs'];
        }
        return new NOCCUserPrefs('');
    }

    /**
     * Set user preferences from the session
     * @param NOCCUserPrefs $value User preferences
     * @static
     * @todo Check for NOCCUserPrefs?
     */
    public static function setUserPrefs($value) {
        $_SESSION['nocc_user_prefs'] = $value;
    }

    /**
     * Get HTML mail sending from the session
     * @return bool User preferences
     * @static
     */
    public static function getSendHtmlMail() {
        if (isset($_SESSION['html_mail_send']) && $_SESSION['html_mail_send']) {
            return true;
        }
        return false;
    }

    /**
     * Set HTML mail sending from the session
     * @param bool $value User preferences
     * @static
     */
    public static function setSendHtmlMail($value) {
        $_SESSION['html_mail_send'] = $value;
    }
}
