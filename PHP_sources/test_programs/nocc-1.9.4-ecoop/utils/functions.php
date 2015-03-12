<?php
/**
 * Miscellaneous functions
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
 * @subpackage Utilities
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: functions.php 2583 2013-10-29 11:11:05Z oheil $
 */

require_once './classes/class_local.php';
require_once './classes/nocc_mailreader.php';
require_once './classes/nocc_theme.php';
require_once './classes/nocc_quotausage.php';
require_once './classes/nocc_mailaddress.php';
require_once './classes/nocc_attachedfile.php';

/**
 * Get UTF-8 string length
 * @param string $string UTF-8 string
 * @return int UTF-8 string length
 */
function utf8_strlen($string) {
    return mb_strlen($string, 'UTF-8');
}

/**
 * Get UTF-8 string part
 * @param string $string UTF-8 string
 * @param int $start Start
 * @param int $length Length
 * @return string UTF-8 string part
 */
function utf8_substr($string, $start, $length = 0) {
    return mb_substr($string, $start, $length, 'UTF-8');
}

/**
 * ...
 * @param nocc_imap $pop
 * @param int $skip
 * @return array
 * @todo Rename!
 */
function inbox(&$pop, $skip = 0) {
    $msg_list = array();

    $lang = $_SESSION['nocc_lang'];
    $sort = $_SESSION['nocc_sort'];
    $sortdir = $_SESSION['nocc_sortdir'];

    $num_msg = $pop->num_msg();
    $per_page = get_per_page();

    $start_msg = $skip * $per_page;
    $end_msg = $start_msg + $per_page;

    $sorted = $pop->sort($sort, $sortdir);

    $end_msg = ($num_msg > $end_msg) ? $end_msg : $num_msg;
    if ($start_msg > $num_msg) {
        return $msg_list;
    }

    for ($i = $start_msg; $i < $end_msg; $i++) {
        $msgnum = $sorted[$i];
        $mail_reader = new NOCC_MailReader($msgnum, $pop, false);

        $newmail = $mail_reader->isUnread();
        // Check "Status" line with UCB POP Server to see if this is a new message.
        // This is a non-RFC standard line header. Set this in conf.php
        if ($_SESSION['ucb_pop_server']) {
            $newmail = $mail_reader->isUnreadUcb();
        }

        $timestamp = $mail_reader->getTimestamp();
        $date = format_date($timestamp, $lang);
        $time = format_time($timestamp, $lang);
        $msg_list[$i] =  Array(
                'index' => $i,
                'new' => $newmail,
                'number' => $msgnum,
                'attach' => $mail_reader->hasAttachments(),
                'to' => $mail_reader->getToAddress(),
                'from' => $mail_reader->getFromAddress(),
                'subject' => $mail_reader->getSubject(),
                'date' => $date,
                'time' => $time,
                'size' => $mail_reader->getSize(),
                'priority' => $mail_reader->getPriority(),
                'priority_text' => $mail_reader->getPriorityText(),
                'flagged' => $mail_reader->isFlagged(),
                'spam' => $mail_reader->isSpam());
    }
    return ($msg_list);
}

/**
 * ...
 * @global object $conf
 * @global string $html_att_label
 * @global string $html_atts_label
 * @global string $lang_invalid_msg_num
 * @param nocc_imap $pop
 * @param int $mail
 * @param bool $verbose
 * @param array $attachmentParts
 * @return array
 * @todo Rename!
 */
function aff_mail(&$pop, $mail, $verbose, &$attachmentParts = null) {
    global $conf;
    global $lang_invalid_msg_num;

    $sort = $_SESSION['nocc_sort'];
    $sortdir = $_SESSION['nocc_sortdir'];
    $lang = $_SESSION['nocc_lang'];

    // Clear variables
    $body = $body_charset = $to = $cc = '';

    // Message Found boolean
    $msg_found = false;

    // Get message numbers in sorted order
    $sorted = $pop->sort($sort, $sortdir);

    // Finding the next and previous message number
    $prev_msg = $next_msg = 0;
    for ($i = 0; $i < sizeof($sorted); $i++) {
        if ($mail == $sorted[$i]) {
            $prev_msg = ($i - 1 >= 0) ? $sorted[$i - 1] : 0;
            $next_msg = ($i + 1 < sizeof($sorted)) ? $sorted[$i + 1] : 0;
            $msg_found = true;
            break;
        }
    }

    if (!$msg_found) {
        throw new Exception($lang_invalid_msg_num);
    }

    $mail_reader = new NOCC_MailReader($mail, $pop, true);

    // If we are showing all headers, gather them into a header array
    $header = '';
    if (($verbose == true) && ($conf->use_verbose == true)) {
        $header = $mail_reader->getHeader();
    }

    // Get the first part
    $body_mime = '';
    $body_transfer = '';
    $bodyPart = $mail_reader->getBodyPart();
    if (!empty($bodyPart)) { //if has body...
        $bodyPartStructure = $bodyPart->getPartStructure();

        $body_mime = (string)$bodyPart->getInternetMediaType();
        $body_transfer = (string)$bodyPart->getEncoding();
        $body = $pop->fetchbody($mail, $bodyPart->getPartNumber());

        $body = nocc_imap::decode($body, (string)$bodyPart->getEncoding());
        $body = remove_stuff($body, $body_mime);

        $body_charset = detect_body_charset($body, $bodyPartStructure->getCharset());

        // If user has selected another charset, we'll use it
        if (isset($_REQUEST['user_charset']) && $_REQUEST['user_charset'] != '') {
          $body_charset = $_REQUEST['user_charset'];
        }

        //TODO: Move to a own function!?
        $body_converted = os_iconv($body_charset, 'UTF-8', $body);
        $body = ($body_converted===false) ? $body : $body_converted;
        //tmp['charset'] = ($body_converted===false) ? $body_charset : 'UTF-8';
    }

    $link_att = GetAttachmentsTableRow($mail_reader);

    $attachmentParts = $mail_reader->getAttachmentParts();

    $timestamp = $mail_reader->getTimestamp();
    $date = format_date($timestamp, $lang);
    $time = format_time($timestamp, $lang);
    $content = Array(
        'message_id' => $mail_reader->getMessageId(),
        'from' => $mail_reader->getFromAddress(),
        'to' => $mail_reader->getToAddress(),
        'cc' => $mail_reader->getCcAddress(),
        'reply_to' => $mail_reader->getReplyToAddress(),
        'subject' => $mail_reader->getSubject(),
        'timestamp' => $timestamp,
        'date' => $date,
        'time' => $time,
        'complete_date' => $date . ' ' . $time,
        'priority' => $mail_reader->getPriority(),
        'priority_text' => $mail_reader->getPriorityText(),
        'spam' => $mail_reader->isSpam(),
        'att' => $link_att,
        'body' => graphicalsmilies($body),
        'body_mime' => convertLang2Html($body_mime),
        'body_transfer' => convertLang2Html($body_transfer),
        'header' => $header,
        'verbose' => $verbose,
        'prev' => $prev_msg,
        'next' => $next_msg,
        'msgnum' => $mail,
        'charset' => $body_charset
    );
    return ($content);
}

/**
 * Detect the charset from the body
 * @global object $conf
 * @param string $body Body
 * @param string $suspectedCharset Suspected charset
 * @return string Detected charset
 */
function detect_body_charset($body, $suspectedCharset) {
    global $conf;

    $body_charset = ($suspectedCharset == 'default') ? detect_charset($body) : $suspectedCharset;
    // Convert US-ASCII to ISO-8859-1 for systems which have difficulties with.
    if (strtolower($body_charset) == 'us-ascii') {
        $body_charset = 'ISO-8859-1';
    }
    // Use default charset if no charset is provided by the displayed mail.
    // If no default charset is defined, use ISO-8859-1.
    if ($body_charset == '' || $body_charset == null) {
        if (isset($conf->default_charset) && $conf->default_charset != '') {
            $body_charset = $conf->default_charset;
        }
        else {
            $body_charset = 'ISO-8859-1';
        }
    }
    return $body_charset;
}

/**
 * ...
 * @param NOCC_MailReader $mail_reader Mail reader
 * @param array $attach_tab Attachments
 * @todo Only temporary needed!
 */
function fillAttachTabFromMailReader($mail_reader, &$attach_tab) {
    global $html_part_x;

    $parts = $mail_reader->getAttachmentParts();
    foreach ($parts as $part) { //for all parts...
        $defaultname = sprintf($html_part_x, $part->getPartNumber());
        $partstructure = $part->getPartStructure();
        $tmp = Array(
            'number' => $part->getPartNumber(),
            'id' => $partstructure->getId(),
            'name' => $partstructure->getName($defaultname),
            'mime' => (string)$part->getInternetMediaType(),
            'transfer' => (string)$part->getEncoding(),
            'disposition' => $partstructure->getDisposition(),
            'charset' => $partstructure->getCharset(),
            'size' => $part->getSize()
        );

        array_unshift($attach_tab, $tmp);
    }
}

/**
 * ...
 * @param NOCC_MailReader $mail_reader Mail reader
 * @todo Only temporary needed!
 */
function GetAttachmentsTableRow($mail_reader) {
    global $html_att_label, $html_atts_label;

    $attach_tab = array();
    fillAttachTabFromMailReader($mail_reader, $attach_tab);

    $link_att = '';
    if ($mail_reader->hasAttachments()) {
        switch (sizeof($attach_tab)) {
            case 0:
                break;
            case 1:
                $link_att = '<tr><th class="mailHeaderLabel right">' . $html_att_label . '</th><td class="mailHeaderData">' . link_att($mail_reader->getMessageNumber(), $attach_tab) . '</td></tr>';
                break;
            default:
                $link_att = '<tr><th class="mailHeaderLabel right">' . $html_atts_label . '</th><td class="mailHeaderData">' . link_att($mail_reader->getMessageNumber(), $attach_tab) . '</td></tr>';
                break;
        }
    }
    return $link_att;
}

/**
 * ...
 * @param string $body
 * @param string $mime
 * @return string
 */
function remove_stuff($body, $mime) {
    if (preg_match('|html|i', $mime)) {
        $body = NOCC_Security::cleanHtmlBody($body);
        $body = NOCC_Security::purifyHtml($body);
        //TODO: Move to NOCC_Security::cleanHtmlBody() too?
        $body = preg_replace("|href=\"(.*)script:|i", 'href="nocc_removed_script:', $body);
        $body = preg_replace("|<([^>]*)java|i", '<nocc_removed_java_tag', $body);
        $body = preg_replace("|<([^>]*)&{.*}([^>]*)>|i", "<&{;}\\3>", $body);
        $body = NOCC_Body::prepareHtmlLinks($body);
    }
    elseif (preg_match('|plain|i', $mime)) {
        $user_prefs = NOCC_Session::getUserPrefs();
        $body = htmlspecialchars($body);
        $body = NOCC_Body::prepareTextLinks($body);
        if ($user_prefs->getColoredQuotes()) {
            $body = NOCC_Body::addColoredQuotes($body);
        }
        if ($user_prefs->getDisplayStructuredText()) {
            $body = NOCC_Body::addStructuredText($body);
        }

        // Disable incoming message wordwrapping. Plain text message formatting should be done by the writter.
        // The client don't have to change the way the message is display (think to ASCII schemes).
        // $body = wordwrap($body, 80);

        $body = nl2br($body);
    }
    return ($body);
}

/**
 * ...
 * @global string $html_kb
 * @param int $mail
 * @param array $attach_tab
 * @return string
 * @todo Rewrite to use direct a NOCC_MailReader object!
 */
function link_att($mail, $attach_tab) {
    global $html_kb;
    sort($attach_tab);
    $html = '<ul class="attachments">';
    while ($tmp = array_shift($attach_tab)) {
        if (!empty($tmp['name'])) {
            $mime = str_replace('/', '-', $tmp['mime']);
            $att_name = nocc_imap::utf8($tmp['name']);
            $att_name_dl = $att_name;
            $att_name = htmlentities($att_name, ENT_COMPAT, 'UTF-8');
            if (empty($att_name)) { //if we got a problem with the $att_name encoding...
                $att_name = htmlentities($att_name_dl, ENT_COMPAT);
            }
            $html .= '<li><a href="download.php?'.NOCC_Session::getUrlGetSession().'&amp;mail=' . $mail . '&amp;part=' . $tmp['number'] . '&amp;transfer=' . $tmp['transfer'] . '&amp;filename=' . base64_encode($att_name_dl) . '&amp;mime=' . $mime . '">' . $att_name . '</a> <em>' . $tmp['size'] . ' ' . $html_kb . '</em></li>';
        }
    }
    $html .= '</ul>';
    return ($html);
}

/**
 * Return date formatted as a string, according to locale
 * @global string $default_date_format
 * @global string $lang_locale
 * @global string $no_locale_date_format
 * @param int $date
 * @param string $lang
 * @return string
 * @todo Why is $lang not used?
 * @todo Strict Standards: strftime() [function.strftime]: It is not safe to rely on the system's timezone settings. Please use the date.timezone setting, the TZ environment variable or the date_default_timezone_set() function. In case you used any of those methods and you are still getting this warning, you most likely misspelled the timezone identifier. We selected 'Europe/Berlin' for 'CEST/2.0/DST' instead in /www/htdocs/v155331/lgmsworld-shop/nocc/utils/functions.php on line ...
 */
function format_date(&$date, &$lang) {
    global $default_date_format;
    global $lang_locale;
    global $no_locale_date_format;

    // handle bad inputs
    if (empty($date))
        return '';

    // if locale can't be set, use default for no locale
    if (!setlocale(LC_TIME, $lang_locale))
        $default_date_format = $no_locale_date_format;

    // format dates
    return strftime($default_date_format, $date);
}

/**
 * ...
 * @global string $default_time_format
 * @global string $lang_locale
 * @param int $time
 * @param string $lang
 * @return string
 * @todo Why is $lang not used?
 * @todo Strict Standards: strftime() [function.strftime]: It is not safe to rely on the system's timezone settings. Please use the date.timezone setting, the TZ environment variable or the date_default_timezone_set() function. In case you used any of those methods and you are still getting this warning, you most likely misspelled the timezone identifier. We selected 'Europe/Berlin' for 'CEST/2.0/DST' instead in /www/htdocs/v155331/lgmsworld-shop/nocc/utils/functions.php on line ...
 */
function format_time(&$time, &$lang) {
    global $default_time_format;
    global $lang_locale;

    // handle bad inputs
    if (empty($time))
        return '';

    // if locale can't be set, use default for no locale
    setlocale(LC_TIME, $lang_locale);

    // format dates
    return strftime($default_time_format, $time);
}

/**
 * Convert text smilies to graphical smilies
 * @param string $body Body
 * @return string Body
 */
function graphicalsmilies($body) {
    $user_prefs = NOCC_Session::getUserPrefs();
    if ($user_prefs->getUseGraphicalSmilies()) {
        $theme = new NOCC_Theme($_SESSION['nocc_theme']);
        $body = $theme->replaceTextSmilies($body);
    }
    return $body;
}

/**
 * This function build an array with all the recipients of the message for later reply or reply all
 * @param string $from
 * @param string $to
 * @param string $cc
 * @return string
 */
function get_reply_all(&$from, &$to, &$cc) {
    $login = $_SESSION['nocc_login'];
    $domain = $_SESSION['nocc_domain'];
    if (!preg_match("|".$login.'@'.$domain."|i", $from))
        $rcpt = $from.'; ';
    $tab = explode(',', $to);
    while ($tmp = array_shift($tab))
        if (!preg_match("|".$login.'@'.$domain."|i", $tmp))
            $rcpt .= $tmp.'; ';
    $tab = explode(',', $cc);
    while ($tmp = array_shift($tab))
        if (!preg_match("|".$login.'@'.$domain."|i", $tmp))
            $rcpt .= $tmp.'; ';
    $rcpt = isset($rcpt) ? substr($rcpt, 0, strlen($rcpt) - 2) : $from;
    return ($rcpt);
}

/**
 * We need that to build a correct list of all the recipient when we send a message
 * @param string $addr
 * @param string $charset
 * @return array
 * TODO: Move to NOCC_MailAddress as static function and rename?
 */
function cut_address($addr, $charset) {
    // Strip slashes from input
    $addr = safestrip($addr);

    // Break address line into individual addresses, taking
    // quoted addresses into account
    $addresses = array();
    $token = '';
    $quote_esc = false;
    for ($i = 0; $i < strlen($addr); $i++) {
        $c = substr($addr, $i, 1);

        // Are we entering/leaving escaped mode
        if ($c == '"') {
            $quote_esc = !$quote_esc;
        }

        // Is this an address seperator (comma/semicolon)
        if ($c == ',' || $c == ';') {
            if (!$quote_esc) {
                $token = trim($token);
                if ($token != '') {
                    $addresses[] = $token;
                }
                $token = '';
                continue;
            }
        }

        $token .= $c;
    }
    if (!$quote_esc) {
        $token = trim($token);
        if ($token != '') {
            $addresses[] = $token;
        }
    }

    // Loop through addresses
    for ($i = 0; $i < sizeof($addresses); $i++) {
        // Wrap address in brackets, if not already
        $pos = strrpos($addresses[$i], '<');
        if (!is_int($pos))
            $addresses[$i] = '<'.$addresses[$i].'>';
        else {
            $name = '';
            if ($pos != 0)
                $name = '=?'.$charset.'?B?'.base64_encode(substr($addresses[$i], 0, $pos - 1)).'?=';
            $addr = substr($addresses[$i], $pos);
            $addresses[$i] = '"'.$name.'" '.$addr.'';
        }
    }
    return ($addresses);
}

/**
 * ...
 * @param object $pop
 * @param int $mail
 * @param string $part_no
 * @param string $transfer
 * @param string $msg_charset
 * @return string
 * @todo Why is $msg_charset not used?
 */
function view_part(&$pop, &$mail, $part_no, $transfer, $msg_charset) {
    if (isset($ev) && NoccException::isException($ev)) {
        return '<p class="error">' . $ev->getMessage . '</p>';
    }
    $text = $pop->fetchbody($mail, $part_no);
    return nl2br(htmlspecialchars(nocc_imap::decode($text, $transfer)));
}

/**
 * This function removes temporary attachment files and
 * removes any attachment information from the session
 */
function clear_attachments() {
	if ( !isset($_SESSION['send_backup']) && isset($_SESSION['nocc_attach_array']) && is_array($_SESSION['nocc_attach_array'])) {
		while ($tmp = array_shift($_SESSION['nocc_attach_array'])) {
			$tmp->delete();
		}
		unset($_SESSION['nocc_attach_array']);
	}
}

/**
 * This function chops the <mail@domain.com> bit from a
 * full 'Blah Blah <mail@domain.com>' address, or not
 * depending on the 'hide_addresses' preference.
 * @global object $html_unknown
 * @param string $address
 * @return string
 * TODO: Move to NOCC_MailAddress as static function and rename?
 */
function display_address($address) {
    global $html_unknown;

    // Check for null
    if ($address == '')
        return $html_unknown;
    
    $address = NOCC_MailAddress::simplify($address);
    
    // Get preference
    $user_prefs = NOCC_Session::getUserPrefs();

    // If not set, return full address.
    if (!$user_prefs->getHideAddresses())
        return $address;

    return NOCC_MailAddress::chopAddress($address);
}

/**
 * ...
 * @param string $body
 * @param string $from
 * @param string $html_wrote
 * @return string
 */
function mailquote(&$body, &$from, $html_wrote) {
    $user_prefs = NOCC_Session::getUserPrefs();

    $crlf = "\r\n";
    $from = ucwords(trim(preg_replace("|&lt;.*&gt;|", "", str_replace("\"", "", $from))));

    $wrap_msg = $user_prefs->getWrapMessages();
    if ($wrap_msg) { //If we must wrap the message...
        $msg = '';
        //Break message in table with "\r\n" as separator
        $tbl = explode("\r\n", $body);
        for ($i = 0, $buffer = ''; $i < count($tbl); ++$i) { //For each line...
            unset($buffer);
            // Number of "> "
            $q = substr_count($tbl[$i], "> ");

            $tbl[$i] = rtrim($tbl[$i]);
            // Erase the "> "
            $tbl[$i] = str_replace("> ", "", $tbl[$i]);
            // Erase the break line
            $tbl[$i] = str_replace("\n", " ", $tbl[$i]);
            // length of "> > ...."
            $length = ($q + 1) * strlen("> ");
            // Add the quote if ligne is not to long
            if (strlen($tbl[$i]) + $length <= $wrap_msg)
                $msg .= str_pad($tbl[$i], strlen($tbl[$i]) + $length, "> ", STR_PAD_LEFT) . $crlf;
            // If line is to long, create new line
            else {
                $words = explode(" ", $tbl[$i]);
                $buffer = '';
                for ($j = 0; $j < count($words); ++$j) {
                    if (strlen($buffer) + strlen($words[$j]) + $length <= $wrap_msg)
                        $buffer .= $words[$j] . " ";
                    else {
                        $msg .=  str_pad(rtrim($buffer), strlen(rtrim($buffer)) + $length, "> ", STR_PAD_LEFT) . $crlf;
                        $buffer = $words[$j] . " ";
                    }
                }
                //if ($q != substr_count($tbl[$i + 1], "> "))
                $msg .= str_pad(rtrim($buffer), strlen(rtrim($buffer)) + $length, "> ", STR_PAD_LEFT) . $crlf;
            }
        }
        $body = $msg;
    } else {
        $body = "> " . preg_replace("|\n|", "\n> ", trim($body));
    }

    return($from . ' ' . $html_wrote . " :\n\n" . $body);

}

/**
 * If running with magic_quotes_gpc (get/post/cookie) set
 * in php.ini, we will need to strip slashes from every
 * field we receive from a get/post operation.
 * @param string $string
 * @return string
 */
function safestrip(&$string) {
    if(get_magic_quotes_gpc())
        $string = stripslashes($string);
    return $string;
}

/**
 * Wrap outgoing messages to
 * @param string $txt
 * @param int $length
 * @param string $newline
 * @todo Move to class_send.php?
 * @return string
 */
function wrap_outgoing_msg($txt, $length, $newline) {
    $msg = '';
    // cut message in segment
    $tbl = explode("\r\n", $txt);
    // Clean the end of the line
    for ($i = 0, $buffer = ''; $i < count($tbl); ++$i) {
        $tbl[$i] = rtrim($tbl[$i]);
        if (strlen($tbl[$i]) <= $length)
            $msg .= $tbl[$i] . $newline;
        else {
            unset($buffer);
            $words = explode(" ", $tbl[$i]);
            for ($j = 0; $j < count($words); ++$j) {
                if ((strlen($buffer) + strlen($words[$j])) <= $length)
                    $buffer .= $words[$j] . " ";
                else {
                    $msg .= rtrim($buffer) . $newline;
                    $buffer = $words[$j] . " ";
                }
            }
            $msg .= rtrim($buffer) . $newline;
        }
    }
    return $msg;
}

/**
 * ...
 * @param string $txt
 * @return string
 * @todo Move to class_send.php?
 */
function escape_dots($txt) {
    $crlf = "\r\n";
    $msg = '';

    // cut message in segment
    $tbl = explode($crlf, $txt);

    for ($i = 0; $i < count($tbl); ++$i) {
        if (strlen($tbl[$i]) != 0 && $tbl[$i][0] == '.')
            $tbl[$i] = "." . $tbl[$i];

        $msg .= $tbl[$i] . $crlf;
    }

    return $msg;
}

/**
 * ...
 * @param string $string
 * @param string $allow
 * @return string
 */
function strip_tags2(&$string, $allow) {
    $string = preg_replace('|<<|', '<nocc_less_than_tag><', $string);
    $string = preg_replace('|>>|', '><nocc_greater_than_tag>;', $string);
    $string = strip_tags($string, $allow . '<nocc_less_than_tag><nocc_greater_than_tag>');
    $string = preg_replace('|<nocc_less_than_tag>|', '<', $string);
    return preg_replace('|<nocc_greater_than_tag>|', '>', $string);
}

/**
 * ...
 * @global object $conf
 * @return int
 */
function get_per_page() {
    global $conf;

    $user_prefs = NOCC_Session::getUserPrefs();
    $msg_per_page = 0;
    if (isset($conf->msg_per_page))
        $msg_per_page = $conf->msg_per_page;
    if (isset($user_prefs->msg_per_page))
        $msg_per_page = $user_prefs->msg_per_page;
    // Failsafe
    if ($msg_per_page < 1)
        $msg_per_page = 25;

    return $msg_per_page;
}

/**
 * Convert html entities to normal characters
 * @param string $string
 * @return string
 */
function unhtmlentities($string) {
    $trans_tbl = get_html_translation_table(HTML_ENTITIES);
    $trans_tbl = array_flip($trans_tbl);
    return strtr($string, $trans_tbl);
}

/**
 * Convert mail data (from, to, ...) to HTML
 * @param string $maildata
 * @param int $cutafter
 * @return string
 */
function convertMailData2Html($maildata, $cutafter = 0) {
    if (($cutafter > 0) && (utf8_strlen($maildata) > $cutafter)) {
        return htmlspecialchars(utf8_substr($maildata, 0, $cutafter)) . '&hellip;';
    } else {
        return htmlspecialchars($maildata);
    }
}

/**
 * Convert a language string to HTML
 * @param string $langstring
 * @return string
 */
function convertLang2Html($langstring) {
    return htmlentities($langstring, ENT_COMPAT, 'UTF-8');
}

/**
 * Wrapper for iconv if GNU iconv is not used
 * @param string $input_charset
 * @param string $output_charset
 * @param string $text
 * @return string
 */
function os_iconv($input_charset, $output_charset, &$text) {
    if (strlen($text) == 0) {
        return $text;
    }

    if (PHP_OS == 'AIX') {
        // AIX has its own small selection of names.
        $input_charset = strtolower($input_charset);
        if ($input_charset == 'x-unknown' || $input_charset == 'us-ascii') {
            $input_charset = 'ISO8859-1';
        } else if (preg_match('|^iso[\-_]?8859[\-_]?([1-9][0-9]?)|', $input_charset, $groups)) {
            $input_charset = 'ISO8859-' . $groups[0];
        } else if (preg_match('|^(windows|cp|ibm)[\-_]?([0-9]+)$|', $input_charset, $groups)) {
            $input_charset = 'IBM-' . str_pad($groups[1], 3, '0', STR_PAD_LEFT);
        }
    } else {
        // Assume default GNU iconv.
        if ($input_charset == 'x-unknown') {
            $input_charset = 'ISO-8859-1';
        }
    }

    return @iconv($input_charset, $output_charset, $text);
}

/**
 * Build a folder breadcrumb navigation...
 * @param string $folder
 */
function buildfolderlink($folder) {
    $folderpath = '';
    // split the string at the periods
    $elements = explode('.', $folder);
    for ($i = 0; $i < count($elements); $i++) {
        if ($i > 0) {
            $folderpath = $folderpath . '.';
            echo ".";
        }
        $folderpath = $folderpath . $elements[$i];
        echo "<a href=\"action.php?".NOCC_Session::getUrlGetSession()."&folder=" . $folderpath . "\">" . mb_convert_encoding($elements[$i], 'UTF-8', 'UTF7-IMAP') . "</a>";
    }
    echo "\n";
}

/**
 * ...
 * @global string $html_page
 * @global string $html_of
 * @global string $alt_prev
 * @global string $title_prev_page
 * @global string $alt_next
 * @global string $title_next_page
 * @param int $pages
 * @param int $skip
 * @return string
 */
function get_page_nav($pages, $skip) {
  global $html_page, $html_of, $alt_prev, $title_prev_page, $alt_next, $title_next_page;

  $html = '';
  if ($pages > 1) { // if there several pages...
    $form_select = '<select class="button" name="skip" onchange="submit();">';
    $selected = '';
    for ($i = 0; $i < $pages; $i++) {
        $xpage = $i + 1;
        if ($i == $skip) {
            $selected = 'selected="selected"';
        } else {
            $selected = '';
        }
        $form_select .= '<option '.$selected.' value="'.$i.'">'.$xpage.'</option>';
    }
    $form_select .= '</select>';

    $page = $skip + 1;
    $pskip = $skip - 1;
    $nskip = $skip + 1;

    $start_page = $page - 2;
    $end_page = $page + 2;
    if ($page < 4) { // if first three pages...
      $start_page = 1;
      $end_page = 6;
    }
    elseif ($page > ($pages - 3)) { // if last three pages...
      $start_page = $pages - 5;
      $end_page = $pages;
    }
    if ($start_page < 1) {
      $end_page = $end_page - $start_page;
      $start_page = 1;
    }
    if ($end_page > $pages) {
      $end_page = $pages;
    }

    $html = '<form method="post" action="action.php?'.NOCC_Session::getUrlGetSession().'">';
    $html .= '<div class="pagenav"><ul>';
    $html .= '<li class="pagexofy"><span>' . $html_page . ' ' . $form_select . ' ' . $html_of . ' ' . $pages . '</span></li>';
    if ($pskip > -1 ) // if NOT first page...
      $html .= '<li class="prev"><a href="action.php?'.NOCC_Session::getUrlGetSession().'&skip=' . $pskip . '" title="' . $title_prev_page . '" rel="prev">&laquo; ' . $alt_prev . '</a></li>';
    else // if first page...
      $html .= '<li class="prev"><span> &laquo; ' . $alt_prev . '</span></li>';
    if ($start_page > 1) {
      $html .= '<li class="page"><a href="action.php?'.NOCC_Session::getUrlGetSession().'&skip=0" title="' . $html_page . ' 1" rel="first">1</a></li>';
      if ($start_page > 2) {
        $html .= '<li class="extend"><span>&hellip;</span></li>';
      }
    }
    for ($xpage = $start_page; $xpage <= $end_page; $xpage++) { // for all visible pages...
      $xskip = $xpage - 1;
      if ($xpage == $page) // if current page...
        $html .= '<li class="current"><span>' . $xpage . '</span></li>';
      else // if NOT current page...
        $html .= '<li class="page"><a href="action.php?'.NOCC_Session::getUrlGetSession().'&skip=' . $xskip . '" title="' . $html_page . ' ' . $xpage . '">' . $xpage . '</a></li>';
    }
    if ($end_page < $pages) {
      if ($end_page < $pages - 1) {
        $html .= '<li class="extend"><span>&hellip;</span></li>';
      }
      $html .= '<li class="page"><a href="action.php?'.NOCC_Session::getUrlGetSession().'&skip=' . ($pages - 1) . '" title="' . $html_page . ' ' . $pages . '" rel="last">' . $pages . '</a></li>';
    }
    if ($nskip < $pages) // if NOT last page...
      $html .= '<li class="next"><a href="action.php?'.NOCC_Session::getUrlGetSession().'&skip=' . $nskip . '" title="' . $title_next_page . '" rel="next">' . $alt_next . ' &raquo;</a></li>';
    else // if last page...
      $html .= '<li class="next"><span>' . $alt_next . ' &raquo;</span></li>';
    $html .= '</ul></div>';
    $html .= '</form>';
  }
  return $html;
}

/**
 * Remove Unicode "byte order mark" (BOM)...
 * @param string $data
 * @return string
 */
function removeUnicodeBOM($data) {
    if (substr($data, 0, 3) == pack('CCC', 0xEF, 0xBB, 0xBF)) { //UTF-8...
        return substr($data, 3);
    }
    elseif (substr($data, 0, 2) == pack('CC', 0xFE, 0xFF)) { //UTF-16 (BE)...
        return substr($data, 2);
    }
    elseif (substr($data, 0, 2) == pack('CC', 0xFF, 0xFE)) { //UTF-16 (LE)...
        return substr($data, 2);
    }
    elseif (substr($data, 0, 4) == pack('CCCC', 0x00, 0x00, 0xFE, 0xFF)) { //UTF-32 (BE)...
        return substr($data, 4);
    }
    elseif (substr($data, 0, 4) == pack('CCCC', 0x00, 0x00, 0xFF, 0xFE)) { //UTF-32 (LE)...
        return substr($data, 4);
    }
    return $data;
}
