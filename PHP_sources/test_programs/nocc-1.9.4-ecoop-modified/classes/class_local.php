<?php
/**
 * Class for IMAP/POP3 functions
 *
 * Moved all imap_* PHP calls into one, which should make it easier to write
 * our own IMAP/POP3 classes in the future.
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
 * @version    SVN: $Id: class_local.php 2580 2013-08-19 21:57:33Z gerundt $
 */

require_once 'nocc_mailstructure.php';
require_once 'nocc_headerinfo.php';
require_once 'nocc_header.php';
require_once 'exception.php';
require_once './utils/detect_cyr_charset.php';
require_once './utils/crypt.php';

class result
{
  public $text = '';
  public $charset = '';
}

//TODO: Use mail or message as name?
class nocc_imap
{
    private $server;
    private $login;
    private $passwd;
    private $conn;
    private $folder;
    private $namespace;
    private $_isImap;

    /**
     * ...
     * @global object $conf
     * @global string $lang_could_not_connect
     * @return nocc_imap Me!
     */
    public function __construct() {
        global $conf;
        global $lang_could_not_connect;

        if (!isset($_SESSION['nocc_servr']) || !isset($_SESSION['nocc_folder']) || !isset($_SESSION['nocc_login']) || !isset($_SESSION['nocc_passwd'])) {
            throw new Exception($lang_could_not_connect);
        }

        $this->server = $_SESSION['nocc_servr'];
        $this->folder = $_SESSION['nocc_folder'];
        $this->login = $_SESSION['nocc_login'];
        /* decrypt password */
        $this->passwd = decpass($_SESSION['nocc_passwd'], $conf->master_key);
        $this->namespace = $_SESSION['imap_namespace'];

        // $ev is set if there is a problem with the connection
        $conn = @imap_open('{'.$this->server.'}'.$this->folder, $this->login, $this->passwd, 0);
        if (!$conn) {
            throw new Exception($lang_could_not_connect.": ".imap_last_error());
        }
        $this->conn = $conn;

        $this->_isImap = $this->isImapCheck();
        $_SESSION['is_imap'] = $this->_isImap;

        return $this;
    }

    /**
     * Get the last IMAP error that occurred during this page request
     * @return string Last IMAP error
     * @todo Rename to getLastError()?
     */
    public function last_error() {
        return imap_last_error();
    }

    /**
     * Search messages matching the given search criteria
     * @param string $criteria Search criteria
     * @return array Messages
     */
    public function search($criteria) {
        $messages = @imap_search($this->conn, $criteria);
        if (is_array($messages)) {
            return $messages;
        }
        return array();
    }

    /**
     * Fetch mail structure
     * @param integer $msgnum Message number
     * @return NOCC_MailStructure Mail structure
     */
    public function fetchstructure($msgnum) {
        $structure = @imap_fetchstructure($this->conn, $msgnum);
        if (!is_object($structure)) {
            throw new Exception('imap_fetchstructure() did not return an object.');
        }
        return new NOCC_MailStructure($structure);
    }

    /**
     * Fetch header
     * @param integer $msgnum Message number
     * @return NOCC_Header Header
     * @todo Throw exceptions?
     */
    public function fetchheader($msgnum) {
        $header = imap_fetchheader($this->conn, $msgnum);
        return new NOCC_Header($header);
    }

    /**
     * Fetch body
     * @param integer $msgnum Message number
     * @param string $partnum Part number
     * @return string Body
     * @todo Throw exceptions?
     */
    public function fetchbody($msgnum, $partnum) {
        return @imap_fetchbody($this->conn, $msgnum, $partnum);
    }

    /**
     * Fetch the size of message
     * @param integer $msgnum Message number
     * @return int size in bytes
     */
    public function get_size($msgnum) {
	$overview=imap_fetch_overview($this->conn,$msgnum);
	$size=0;
	if( isset($overview[0]->size) ) {
		$size=$overview[0]->size;
	}
	return $size;
    }

    /**
     * Fetch the entire message
     * @param integer $msgnum Message number
     * @return string Message
     * @todo Throw exceptions?
     */
    public function fetchmessage($msgnum) {
        return @imap_fetchbody($this->conn, $msgnum, '');
    }

    /**
     * Get the number of messages in the current mailbox
     * @return integer Number of messages
     * @todo Rename to GetMessageCount()?
     */
    public function num_msg() {
        return imap_num_msg($this->conn);
    }

    /**
     * ...
     * @param string $sort Sort criteria
     * @param integer $sortdir Sort direction
     * @return array Sorted message list
     */
    public function sort($sort, $sortdir) {
        switch($sort) {
            case '1': $imapsort = SORTFROM; break;
            case '2': $imapsort = SORTTO; break;
            case '3': $imapsort = SORTSUBJECT; break;
            case '4': $imapsort = SORTDATE; break;
            case '5': $imapsort = SORTSIZE; break;
        }
        $sorted = imap_sort($this->conn, $imapsort, $sortdir, SE_NOPREFETCH);
        if (!is_array($sorted)) {
            throw new Exception('imap_sort() did not return an array.');
        }
        return $sorted;
    }

    /**
     * Get header info
     * @param integer $msgnum Message number
     * @param string $defaultcharset Default charset
     * @return NOCC_HeaderInfo Header info
     */
    public function headerinfo($msgnum, $defaultcharset = 'ISO-8859-1') {
        $headerinfo = @imap_headerinfo($this->conn, $msgnum);
        if (!is_object($headerinfo)) {
            throw new Exception('imap_headerinfo() did not return an object.');
        }
        return new NOCC_HeaderInfo($headerinfo, $defaultcharset);
    }

    /**
     * Delete a mailbox
     * @param string $mailbox Mailbox
     * @return boolean Successful?
     * @todo Rename to deleteMailbox()?
     */
    public function deletemailbox($mailbox) {
        return imap_deletemailbox($this->conn, '{' . $this->server . '}' . $mailbox);
    }

	/**
	 * find specific email header from complete header
	 * @param string $head_search header to find
	 * @param string $temp_header find in headers
	 * @return string header content 
	 *
	 * credits go to rklrkl, https://sourceforge.net/p/nocc/patches/149/, 2009-08-16
	 */
	function find_email_header($head_search,&$temp_header) {
		// Look for "\n<header>: " in the header
		$hpos = strpos($temp_header,"\n".$head_search.": ");
		if ($hpos!=FALSE) {
			// Now extract out the rest of the header line
			$hpos += strlen("\n".$head_search.": ");
			$hlen = strpos(substr($temp_header,$hpos),"\n");
			if ($hlen>0) {
				$hstr = substr($temp_header,$hpos,$hlen);
				if ($head_search=="Delivery-date" || $head_search=="Date") {
					// Looking for e-mail date...
					// Convert "normal" date format into bizarro mbox date format
					// (which is expressed in local time, not GMT)
					//return(date("D M d H:i:s Y",strtotime($hstr)));
					return(strtotime($hstr));
				}
				else {
					// Looking for "from" e-mail address...
					// Find out the first word which has an @ in it and return it
					$harr = explode(" ",$hstr);
					reset($harr);
					foreach($harr as $eachword) {
						// If we got an e-mail address, return it, but stripped of
						// double quotes, <, >, ( and )
						if (strpos($eachword,"@")) {
							return(trim($eachword,'"()<>'));
						}
					}
				}
			}
		}
		return "";
	}

	/**
	 * Create a tmp file for downloading a complete mailbox folder
	 * @param string $download_box name of the folder to download
	 *
	 *
	 * credits go to rklrkl, https://sourceforge.net/p/nocc/patches/149/, 2009-08-16
	 */
	function downloadmailbox(&$download_box,&$ev) {

		$_SESSION['fd_message']=array();

        	global $conf;

		// Create a sanitised mbox filename based on the folder name
		$filename = preg_replace('/[\\/:\*\?"<>\|;]/','_',str_replace('&nbsp;',' ',$download_box)).".mbox";
		$_SESSION['fd_message'][]=$filename;

		$remember_folder=$_SESSION['nocc_folder'];
		$_SESSION['nocc_folder'] = $download_box;

		$ev = '';
		$pop = new nocc_imap($ev);
		if (NoccException::isException($ev)) {
			$_SESSION['nocc_folder']=$remember_folder;
			unset($_SESSION['fd_message']);
			require ('./html/header.php');
			require ('./html/error.php');
			require ('./html/footer.php');
			return;
		}

		$memory_limit=ini_get('memory_limit');
		if( preg_match("/M$/i",$memory_limit) ) {
			$memory_limit=intval($memory_limit)*1024*1024;
		}
		else if( preg_match("/K$/i",$memory_limit) ) {
			$memory_limit=intval($memory_limit)*1024;
		}
		else if( preg_match("/G$/i",$memory_limit) ) {
			$memory_limit=intval($memory_limit)*1024*1024*1024;
		}

		if( strlen($conf->tmpdir)==0 ) {
			$_SESSION['nocc_folder']=$remember_folder;
			unset($_SESSION['fd_message']);
			$ev = new NoccException("tmp folder tmpdir is not set in config/php.conf.");
			return;
		}
		else if( ! is_writable($conf->tmpdir) ) {
			$_SESSION['nocc_folder']=$remember_folder;
			unset($_SESSION['fd_message']);
			$ev = new NoccException("tmp folder ".$conf->tmpdir." is not writeable.");
			return;
		}

		$tmpFile=$_SESSION['sname']."_".md5(uniqid(rand(),true)).'.tmp';
		$_SESSION['fd_message'][]=$tmpFile;
		$tmpFile=$conf->tmpdir.'/'.$tmpFile;
		$_SESSION[$tmpFile]=1;

		$mail_skipped=0;
		if( $mbox=fopen($tmpFile,'w') ) {
			// Find out how many messages are in the folder and loop for each one
			$tot_msgs = $pop->num_msg();
			$_SESSION['fd_message'][]=$tot_msgs;
			for ($mail = 1; $mail <= $tot_msgs; $mail++) {
				// Prefix a line feed to the header so that later searches will
				// find strings if they're right at the start of the header (first
				// char in first line). Also strip any carriage returns that the
				// IMAP server spits out.

       		 		$header_obj = $pop->fetchheader($mail);
				$header=$header_obj->getHeader();
				$header = "\n" . str_replace("\r","",$header);

				$headerinfo_obj=$pop->headerinfo($mail);
				$subject=$headerinfo_obj->getSubject();

				// Find a "from" e-mail address in the headers
				$from=$pop->find_email_header("From",$header);
				if ($from=="") $from=$pop->find_email_header("Reply-To",$header);
				if ($from=="") $from=$pop->find_email_header("X-From-Line",$header);
				if ($from=="") $from="MAILER-DAEMON"; // Fallback if no From addr

				// Find the date header and convert the date into mbox format
				// Yes, Delivery-date: takes priority over Date:, which many
				// mbox creation programs forget to take into account!
				$date=$pop->find_email_header("Delivery-date",$header);
				if ($date=="") $date=$pop->find_email_header("Date",$header);
				if ($date=="") $date=0; // Time zero fallback
				$showdate = format_date($date, $lang);
				$date=date("D M d H:i:s Y",$date);

				// Add the new "From " line, the rest of the header
				// ...and the "raw" [but CR-stripped] body, but replace
				// "\nFrom " with "\n>From " as well.(yes, 2 of the 4 crazily
				// different mbox formats need this). Also append a blank line between
				// message to separate them.

				$mail_size=$pop->get_size($mail);
				$memory_usage=memory_get_usage();

				if( 2*$mail_size+$memory_usage>$memory_limit ) {
					$mail_skipped++;
					$_SESSION['fd_message'][]='<tr><td style="text-align:left;">'.$showdate.'</td><td style="text-align:left;">'.$subject.'</td><td style="text-align:left;">'.$mail_size.'</td></tr>';
				}
				else {
					$body="\n".substr(str_replace("\n\nFrom ","\n\n>From ","\n\n".str_replace("\r","",$pop->fetchmessage($mail))."\n"),2);
					fwrite($mbox,"From ".$from." ".$date.$body);
				}

			}
			fwrite($mbox,"\n");
			fclose($mbox);
		}
		$pop->close();
		$_SESSION['nocc_folder']=$remember_folder;

		if( is_file($tmpFile) ) {
			$file_size=filesize($tmpFile);
			$_SESSION['fd_message'][]=$file_size;
			$_SESSION['fd_message'][]=$mail_skipped;
		}
		else {
			unset($_SESSION['fd_message']);
			$ev = new NoccException("folder download failed.");
			return;
		}
	}
	
	/**
	 * Download the tmp file for a complete mailbox folder
	 *
	 * credits go to rklrkl, https://sourceforge.net/p/nocc/patches/149/, 2009-08-16
	 */
	function downloadtmpfile(&$ev) {
        	global $conf;
		if( isset($_SESSION['fd_tmpfile']) && is_array($_SESSION['fd_tmpfile']) &&
			isset($_SESSION['fd_tmpfile'][0]) && strlen($_SESSION['fd_tmpfile'][0])>0 &&
			isset($_SESSION['fd_tmpfile'][1]) && strlen($_SESSION['fd_tmpfile'][1])>0 )
		{
			$tmpFile=$conf->tmpdir.'/'.basename($_SESSION['fd_tmpfile'][0]);
			$filename=$_SESSION['fd_tmpfile'][1];
			if( is_file($tmpFile) ) {
				$file_size=filesize($tmpFile);

				// If no messages were found in the folder, don't offer the download
				// and simply fall into displaying the Folder page again. Maybe a warning
				// message should go here (JavaScripted "<foldername> folder contains
				// no messages")?
				//if ($file != "") {
				// This is a repeat of a large chunk of code fromm down_mail.php -
				// perhaps that should be put in a function somewhere and shared here
				// too? Would need to take $filename and $file as parameters.
				$isIE = $isIE6 = 0;

				if (!isset($HTTP_USER_AGENT)) {
					$HTTP_USER_AGENT = $_SERVER['HTTP_USER_AGENT'];
				}

				// Set correct http headers.
				// Thanks to Squirrelmail folks :-)
				if (strstr($HTTP_USER_AGENT, 'compatible; MSIE ') !== false && strstr($HTTP_USER_AGENT, 'Opera') === false) {
					$isIE = 1;
				}
	
				if (strstr($HTTP_USER_AGENT, 'compatible; MSIE 6') !== false && strstr($HTTP_USER_AGENT, 'Opera') === false) {
					$isIE6 = 1;
				}

				if ($isIE) {
					$filename=rawurlencode($filename);
					header ("Pragma: public");
					header ("Cache-Control: no-store, max-age=0, no-cache, must-revalidate"); // HTTP/1.1
					header ("Cache-Control: post-check=0, pre-check=0", false);
					header ("Cache-Control: private");
	
					//set the inline header for IE, we'll add the attachment header later if we need it
					header ("Content-Disposition: inline; filename=$filename");
				}
	
				header ("Content-Type: application/octet-stream; name=\"$filename\"");
				header ("Content-Disposition: attachment; filename=\"$filename\"");

				if ($isIE && !$isIE6) {
					header ("Content-Type: application/download; name=\"$filename\"");
				}
				else {
					header ("Content-Type: application/octet-stream; name=\"$filename\"");
				}
				header('Content-Length: '.$file_size);

				$_SESSION[$tmpFile]=$_SESSION[$tmpFile]+1;

				$chunksize = 1 * (1024 * 1024); // how many bytes per chunk
				if( $file_size > $chunksize ) {
					$handle = fopen($tmpFile, 'rb');
					$buffer = '';
					while (!feof($handle)) {
						$buffer = fread($handle, $chunksize);
						echo $buffer;
						ob_flush();
						flush();
					}
					fclose($handle);
				} else {
					readfile($tmpFile);
				}

				exit; // Don't fall into HTML page - we're downloading and need to exit
			}
			else {
				unset($_SESSION['fd_tmpfile']);
				$ev = new NoccException("download file does not exits.");
				return;
			}
		}
	}


    /**
     * Rename a mailbox
     * @param string $oldMailbox Old mailbox
     * @param string $newMailbox New mailbox
     * @return boolean Successful?
     * @todo Rename to renameMailbox()?
     */
    public function renamemailbox($oldMailbox, $newMailbox) {
        return imap_renamemailbox($this->conn, '{' . $this->server . '}' . $oldMailbox, '{' . $this->server . '}' . $this->namespace . mb_convert_encoding($newMailbox, 'UTF7-IMAP', 'UTF-8'));
    }

    /**
     * Create a mailbox
     * @param srtring $mailbox Mailbox
     * @return boolean Successful?
     * @todo Rename to createMailbox()?
     */
    public function createmailbox($mailbox) {
        return imap_createmailbox($this->conn, '{' . $this->server . '}' . $this->namespace . mb_convert_encoding($mailbox, 'UTF7-IMAP', 'UTF-8'));
    }

    /**
     * Copy a mail to a mailbox
     * @param integer $msgnum Message number
     * @param string $mailbox Destination mailbox
     * @return boolean Successful?
     * @todo Rename to copyMail()?
     */
    public function mail_copy($msgnum, $mailbox) {
        return imap_mail_copy($this->conn, $msgnum, $mailbox, 0);
    }

    /**
     * Subscribe to a mailbox
     * @param string $mailbox Mailbox
     * @param bool $isNewMailbox Is new mailbox?
     * @return bool Successful?
     * @todo Is $isNewMailbox really nedded?
     */
    public function subscribe($mailbox, $isNewMailbox) {
        if ($isNewMailbox) {
            return @imap_subscribe($this->conn, '{' . $this->server . '}' . $this->namespace . $mailbox);
        } else {
            return @imap_subscribe($this->conn, '{' . $this->server . '}' . $mailbox);
        }
    }

    /**
     * Unsubscribe from a mailbox
     * @param string $mailbox Mailbox
     * @return bool Successful?
     */
    public function unsubscribe($mailbox) {
        return @imap_unsubscribe($this->conn, '{' . $this->server . '}' . $mailbox);
    }

    /**
     * Move a mail to a mailbox
     * @param integer $msgnum Message number
     * @param string $mailbox Destination mailbox
     * @return boolean Successful?
     * @todo Rename to moveMail()?
     */
    public function mail_move($msgnum, $mailbox) {
        return imap_mail_move($this->conn, $msgnum, $mailbox, 0);
    }

    /**
     * Delete all messages marked for deletion
     * @return boolean Successful?
     */
    public function expunge() {
        return imap_expunge($this->conn);
    }

    /**
     * Delete a mail
     * @param integer $msgnum Message number
     * @return boolean Successful?
     * @todo Rename to deleteMail()?
     */
    public function delete($msgnum) {
        return imap_delete($this->conn, $msgnum, 0);
    }

    public function close() {
        return imap_close($this->conn, CL_EXPUNGE);
    }

    /**
     * ...
     * @return bool Is IMAP?
     * @todo Rename to isImap()?
     */
    public function is_imap() {
        return $this->_isImap;
    }

    /**
     * ...
     * @return bool Is IMAP?
     */
    private function isImapCheck() {
        //--------------------------------------------------------------------------------
        // Check IMAP keywords...
        //--------------------------------------------------------------------------------
        $keywords = array('/imap', '/service=imap', ':143');
        foreach ($keywords as $keyword) { //for each IMAP keyword...
            if (stripos($this->server, $keyword) !== false) {
                return true;
            }
        }

        //--------------------------------------------------------------------------------
        // Check POP3 keywords...
        //--------------------------------------------------------------------------------
        $keywords = array('/pop3', '/service=pop3', ':110');
        foreach ($keywords as $keyword) { //for each POP3 keyword...
            if (stripos($this->server, $keyword) !== false) {
                return false;
            }
        }
        //--------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------
        // Check driver...
        //--------------------------------------------------------------------------------
        $check = imap_check($this->conn);
        if ($check) {
          return ($check->{'Driver'} == 'imap');
        }
        //--------------------------------------------------------------------------------

        return false;
    }

    public static function utf7_decode($text) {
        return imap_utf7_decode($text);
    }

    public static function utf7_encode($data) {
        return imap_utf7_encode($data);
    }

    public static function utf8($mime_encoded_text) {
        //TODO: Fixed in PHP 5.3.2!
        //Since PHP 5.2.5 returns imap_utf8() only capital letters!
        //See bug #44098 for details: http://bugs.php.net/44098
        if (version_compare(PHP_VERSION, '5.2.5', '>=')) { //if PHP 5.2.5 or newer...
            return nocc_imap::decode_mime_string($mime_encoded_text);
        }
        else { //if PHP 5.2.4 or older...
            return imap_utf8($mime_encoded_text);
        }
    }

    /**
     * Decode MIME string
     * @param string $string MIME encoded string
     * @param string $charset Charset
     * @return string Decoded string
     * @static
     */
    public static function decode_mime_string($string, $charset = 'UTF-8') {
        $decodedString = '';
        $elements = imap_mime_header_decode($string);
        foreach ($elements as $element) { //for all elements...
            if ($element->charset == 'default') { //if 'default' charset...
                $element->charset = mb_detect_encoding($element->text);
            }
            $decodedString .= mb_convert_encoding($element->text, $charset, $element->charset);
        }
        return $decodedString;
    }

    /**
     * ...
     * @return array Mailboxes
     */
    public function getmailboxes() {
        $mailboxes = @imap_getmailboxes($this->conn, '{' . $this->server . '}', '*');
        if (!is_array($mailboxes)) {
            throw new Exception('imap_getmailboxes() did not return an array.');
        } else {
            sort($mailboxes);
        }
        return $mailboxes;
    }

    /**
     * ...
     * @return array Mailboxes names
     * @todo Return UTF-8 names?
     */
    public function getmailboxesnames() {
        try {
            $mailboxes = $this->getmailboxes();

            $names = array();
            foreach ($mailboxes as $mailbox) { //for all mailboxes...
                $name = str_replace('{' . $this->server . '}', '', $mailbox->name);
                //TODO: Why not add names with more the 32 chars?
                if (strlen($name) <= 32) {
                    array_push($names, $name);
                }
            }
            return $names;
        }
        catch (Exception $ex) {
            return array();
        }
    }

    /**
     * ...
     * @return array Subscribed mailboxes
     * @todo Really throw an exception?
     */
    public function getsubscribed() {
        $subscribed = @imap_getsubscribed($this->conn, '{' . $this->server . '}', '*');
        if (!is_array($subscribed)) {
            throw new Exception('imap_getsubscribed() did not return an array.');
        } else {
            sort($subscribed);
        }
        return $subscribed;
    }

    /**
     * ...
     * @return array Subscribed mailboxes names
     * @todo Return UTF-8 names?
     */
    public function getsubscribednames() {
        try {
            $subscribed = $this->getsubscribed();

            $names = array();
            foreach ($subscribed as $mailbox) { //for all mailboxes...
                $name = str_replace('{' . $this->server . '}', '', $mailbox->name);
                if (!in_array($name, $names)) {
                    array_push($names, $name);
                }
            }
            return $names;
        }
        catch (Exception $ex) {
            return array();
        }
    }

    /**
     * Mark mail as read
     * @param integer $msgnum Message number
     * @return boolean Successful?
     * @todo Rename to markMailRead()?
     */
    public function mail_mark_read($msgnum) {
        return imap_setflag_full($this->conn, $msgnum, '\\Seen');
    }

    /**
     * Mark mail as unread
     * @param integer $msgnum Message number
     * @return boolean Successful?
     * @todo Rename to markMailUnread()?
     */
    public function mail_mark_unread($msgnum) {
        return imap_clearflag_full($this->conn, $msgnum, '\\Seen');
    }

    //TODO: Rewrite to throw exception!
    public function copytosentfolder($maildata, &$ev, $sent_folder_name) {
        if (!(imap_append($this->conn, '{'.$this->server.'}'.$this->namespace.$sent_folder_name, $maildata, "\\Seen"))) {
            $ev = new NoccException("could not copy mail into $sent_folder_name folder: ".imap_last_error());
            return false;
        }
        return true;
    }

    /*
     * These functions are static, but if we could re-implement them without
     * requiring PHP IMAP support, more people can use NOCC.
     */
    public static function base64($file) {
        return imap_base64($file);
    }

    public static function i8bit($file) {
        return imap_8bit($file);
    }

    public static function qprint($file) {
        return imap_qprint($file);
    }

    /**
     * Decode  BASE64 or QUOTED-PRINTABLE data
     * @param string $data Encoded data
     * @param string $transfer BASE64 or QUOTED-PRINTABLE?
     * @return string Decoded data
     * TODO: Better name?
     */
    public static function decode($data, $transfer) {
        if ($transfer == 'BASE64') { //if BASE64...
            return nocc_imap::base64($data);
        }
        elseif ($transfer == 'QUOTED-PRINTABLE') { //if QUOTED-PRINTABLE...
            return nocc_imap::qprint($data);
        }
        else { //if NOT BASE64/QUOTED-PRINTABLE...
            return $data;
        }
    }

    public static function mime_header_decode($header) {
        $source = imap_mime_header_decode($header);
        //$result[] = new result;
        $result[0] = new result;
        $result[0]->text='';
        $result[0]->charset='ISO-8859-1';
        for ($j = 0; $j < count($source); $j++ ) {
            $element_charset =  ($source[$j]->charset == 'default') ? detect_charset($source[$j]->text) : $source[$j]->charset;

            $element_converted = os_iconv($element_charset, 'UTF-8', $source[$j]->text);
            $result[$j] = new stdClass();
            $result[$j]->text = $element_converted;
            $result[$j]->charset = 'UTF-8';
        }
        return $result;
    }

    /*
     * These are general utility functions that extend the imap interface.
     */
    public function html_folder_select($value, $selected = '') {
        $folders = $this->getsubscribednames();
        if (!is_array($folders) || count($folders) < 1) {
            return "<p class=\"error\">Not currently subscribed to any mailboxes</p>";
        }
        reset($folders);

        $html_select = "<select class=\"button\" id=\"$value\" name=\"$value\">\n";
        foreach ($folders as $folder) {
            $html_select .= "\t<option ".($folder == $selected ? "selected=\"selected\"" : "")." value=\"$folder\">".mb_convert_encoding($folder, 'UTF-8', 'UTF7-IMAP')."</option>\n";
        }
        $html_select .= "</select>\n";
        return $html_select;
    }

    public function get_folder_count() {
        try {
            return count($this->getsubscribed());
        }
        catch (Exception $ex) {
            return 0;
        }
    }

    /**
     * ...
     * @param int $num_messages Number of messages
     * @return int Page count
     */
    public function get_page_count($num_messages) {
        if (!is_int($num_messages)) { //if NO integer...
            return 0;
        }
        if ($num_messages == 0) { //if 0 messages...
            return 0;
        }
        return ceil($num_messages / get_per_page());
    }

    /**
     * Retrieve the quota settings
     * @param string $quotaRoot Quota root (mailbox)
     * @return array Quota settings
     */
    public function get_quota_usage($quotaRoot) {
        return @imap_get_quotaroot($this->conn, $quotaRoot);
    }

    /**
     * Return status information from a mailbox
     * @param string $mailbox Mailbox
     * @return object Status information
     */
    public function status($mailbox) {
        return @imap_status($this->conn, $mailbox, SA_ALL);
    }
}