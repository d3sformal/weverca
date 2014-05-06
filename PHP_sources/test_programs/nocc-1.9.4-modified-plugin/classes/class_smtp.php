<?php
/**
 * Class for sending a mail with SMTP
 *
 * Class based on a work from Unk <rgroesb_garbage@triple-it_garbage.nl>
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
 * @version    SVN: $Id: class_smtp.php 2580 2013-08-19 21:57:33Z gerundt $
 */

require_once 'exception.php';

/**
 * Sending a mail with SMTP
 * @package    NOCC
 */
class smtp {
    var $smtp_server;
    var $port;
    var $from;
    var $to;
    var $cc;
    var $bcc;
    var $subject;
    var $data;
    var $sessionlog = '';
    
    /**
     * Initialize the class
     */
    public function __construct() {
        $this->smtp_server = '';
        $this->port = '';
        $this->from = '';
        $this->to = Array();
        $this->cc = Array();
        $this->bcc = Array();
        $this->subject = '';
        $this->data = '';
    }

    public function smtp_open() {
        $smtp = fsockopen($this->smtp_server, $this->port, $errno, $errstr); 
        if (!$smtp)
            return new NoccException($html_smtp_no_con . ' : ' . $errstr); 
        $line = fgets($smtp, 1024);

        if (substr($line, 0, 1) != '2')
            return new NoccException($html_smtp_error_unexpected . ' : ' . $line); 
        
        return $smtp;
    }

    public function smtp_helo($smtp) {
        /* 'localhost' not always works [Unk] */ 
        fputs($smtp, "helo " . $_SERVER['SERVER_NAME'] . "\r\n"); 
        $this->sessionlog .= "Sent: helo " . $_SERVER['SERVER_NAME'];
        $line = fgets($smtp, 1024); 
        $this->sessionlog .= "Rcvd: $line";

        if (substr($line, 0, 1) != '2')
            return new NoccException($html_smtp_error_unexpected . ' : ' . $line); 
        
        return (true);
    }

    public function smtp_ehlo($smtp) {
        /*
          A working EHLO command. Still, any received information is simply
          ignored.
        */ 
        fputs($smtp, "ehlo " . $_SERVER['SERVER_NAME'] . "\r\n"); 
        $this->sessionlog .= "Sent: ehlo " . $_SERVER['SERVER_NAME'];
        while (empty($line) || substr($line, 3, 1) == '-') {
            $line = fgets($smtp, 1024);
            $this->sessionlog .= "Rcvd: $line";

            if (substr($line, 0, 1) != '2')
                return new NoccException($html_smtp_error_unexpected . ' : ' . $line); 
        }
        return true;
    }

    public function smtp_auth($smtp) {
      global $conf;
      require_once './utils/crypt.php';
      switch ($_SESSION['smtp_auth']) {
          case 'LOGIN':
              fputs($smtp, "auth login\r\n"); 
              $this->sessionlog .= "Sent: auth login";
              $line = fgets($smtp, 1024);
              $this->sessionlog .= "Rcvd: $line";
              if (substr($line, 0, 1) != '3')
                  return new NoccException($html_smtp_error_unexpected . ' : ' . $line);
                  
              fputs($smtp, base64_encode($_SESSION['nocc_login']) . "\r\n"); 
              $this->sessionlog .= "Sent: encoded login";
              $line = fgets($smtp, 1024);
              $this->sessionlog .= "Rcvd: $line";
              if (substr($line, 0, 1) != '3')
                  return new NoccException($html_smtp_error_unexpected . ' : ' . $line);
                  
              fputs($smtp, base64_encode(decpass($_SESSION['nocc_passwd'], $conf->master_key)) . "\r\n"); 
              $this->sessionlog .= "Sent: encoded password";
              $line = fgets($smtp, 1024);
              $this->sessionlog .= "Rcvd: $line";
              if (substr($line, 0, 1) != '2')
                  return new NoccException($html_smtp_error_unexpected . ' : ' . $line);
              return (true);
              break;
          case 'TLS':
		fputs($smtp, "STARTTLS\r\n");
		$this->sessionlog .= "Sent: STARTTLS";
		$line = fgets($smtp, 1024);
		$this->sessionlog .= "Rcvd: $line";
		if (substr($line, 0, 1) != '2')
			return new NoccException($html_smtp_error_unexpected . ' : ' . $line);

		stream_socket_enable_crypto( $smtp,true,STREAM_CRYPTO_METHOD_SSLv23_CLIENT);

		fputs($smtp, "helo " . $_SERVER['SERVER_NAME'] . "\r\n"); 
		$this->sessionlog .= "Sent: helo " . $_SERVER['SERVER_NAME'];
		$line = fgets($smtp, 1024);
		$this->sessionlog .= "Rcvd: $line";
		if (substr($line, 0, 1) != '2')
			return new NoccException($html_smtp_error_unexpected . ' : ' . $line);

		fputs($smtp, "auth login\r\n"); 
		$this->sessionlog .= "Sent: auth login";
		$line = fgets($smtp, 1024);
		$this->sessionlog .= "Rcvd: $line";
		if (substr($line, 0, 1) != '3')
			return new NoccException($html_smtp_error_unexpected . ' : ' . $line);
		
		fputs($smtp, base64_encode($_SESSION['nocc_login']) . "\r\n");
		$this->sessionlog .= "Sent: encoded login";
		$line = fgets($smtp, 1024);
		$this->sessionlog .= "Rcvd: $line";
		if (substr($line, 0, 1) != '3')
			return new NoccException($html_smtp_error_unexpected . ' : ' . $line);

		fputs($smtp, base64_encode(decpass($_SESSION['nocc_passwd'], $conf->master_key)) . "\r\n");
		$this->sessionlog .= "Sent: encoded password";
		$line = fgets($smtp, 1024);
		$this->sessionlog .= "Rcvd: $line";
		if (substr($line, 0, 1) != '2')
			return new NoccException($html_smtp_error_unexpected . ' : ' . $line);

		return (true);
		break;
          case 'PLAIN':
              fputs($smtp, "auth plain " . base64_encode($_SESSION['nocc_login'] . chr(0) . $_SESSION['nocc_login'] . chr(0) . decpass($_SESSION['nocc_passwd'], $conf->master_key)) . "\r\n");
              $this->sessionlog .= "Sent: encoded auth";
              $line = fgets($smtp, 1024);
              $this->sessionlog .= "Rcvd: $line";
              if (substr($line, 0, 1) != '2')
              return new NoccException($html_smtp_error_unexpected . ' : ' . $line);
              return (true);
              break;
          case '':
              return (true);
      }
      return true;
    }

    public function smtp_mail_from($smtp) {
        fputs($smtp, "MAIL FROM:$this->from\r\n"); 
        $this->sessionlog .= "Sent: MAIL FROM:$this->from";
        $line = fgets($smtp, 1024);
        $this->sessionlog .= "Rcvd: $line";

        if (substr($line, 0, 1) <> '2')
            return new NoccException($html_smtp_error_unexpected . ' : ' . $line);

        return true;
    }

    public function smtp_rcpt_to($smtp) {
        // Modified by nicocha to use to, cc and bcc field
        while ($tmp = array_shift($this->to)) {
            if($tmp == '' || $tmp == '<>')
                continue;
            fputs($smtp, "RCPT TO:$tmp\r\n");
            $this->sessionlog .= "Sent: RCPT TO:$tmp";
            $line = fgets($smtp, 1024);
            $this->sessionlog .= "Rcvd: $line";

            if (substr($line, 0, 1) <> '2')
                return new NoccException($html_smtp_error_unexpected . ' : ' . $line);
        }
        while ($tmp = array_shift($this->cc)) {
            if($tmp == '' || $tmp == '<>')
                continue;
            fputs($smtp, "RCPT TO:$tmp\r\n");
            $this->sessionlog .= "Sent: RCPT TO:$tmp";
            $line = fgets($smtp, 1024);
            $this->sessionlog .= "Rcvd: $line";

            if (substr($line, 0, 1) <> '2')
                return new NoccException($html_smtp_error_unexpected . ' : ' . $line);
        }

        while ($tmp = array_shift($this->bcc)) {
            if($tmp == '' || $tmp == '<>')
                continue;
            fputs($smtp, "RCPT TO:$tmp\r\n");
            $this->sessionlog .= "Sent: RCPT TO:$tmp";
            $line = fgets($smtp, 1024);
            $this->sessionlog .= "Rcvd: $line";

            if (substr($line, 0, 1) <> '2')
                return new NoccException($html_smtp_error_unexpected . ' : ' . $line);
        }
        return true;
    }

    public function smtp_data($smtp) {
        fputs($smtp, "DATA\r\n"); 
        $this->sessionlog .= "Sent: DATA";
        $line = fgets($smtp, 1024);
        $this->sessionlog .= "Rcvd: $line";

        if (substr($line, 0, 1) != '3')
            return new NoccException($html_smtp_error_unexpected . ' : ' . $line); 
        
        fputs($smtp, "$this->data"); 
        fputs($smtp, "\r\n.\r\n"); 
        $line = fgets($smtp, 1024); 
        $this->sessionlog .= "Rcvd: $line";
        if (substr($line, 0, 1) !=  '2')
            return new NoccException($html_smtp_error_unexpected . ' : ' . $line); 

        return true;
    }

    public function smtp_quit($smtp) {
        fputs($smtp, "QUIT\r\n");
        $this->sessionlog .= "Sent: QUIT";
        $line = fgets($smtp, 1024);
        $this->sessionlog .= "Rcvd: $line";

        if (substr($line, 0, 1) !=  '2')
            return new NoccException($html_smtp_error_unexpected . ' : ' . $line); 

        return true;
    }

    public function send() {
        $smtp = $this->smtp_open();
        if(NoccException::isException($smtp))
            return $smtp;
        unset ($ev);
        $ev = $this->smtp_ehlo($smtp);
        if(NoccException::isException($ev))
            return $ev;
        unset ($ev);
        $ev = $this->smtp_auth($smtp);
        if(NoccException::isException($ev))
            return $ev;
        unset ($ev);
        $ev = $this->smtp_mail_from($smtp);
        if(NoccException::isException($ev))
            return $ev;
        unset ($ev);
        $ev = $this->smtp_rcpt_to($smtp);
        if(NoccException::isException($ev))
            return $ev;
        unset ($ev);
        $ev = $this->smtp_data($smtp);
        if(NoccException::isException($ev))
            return $ev;
        unset ($ev);
        $ev = $this->smtp_quit($smtp);
        if(NoccException::isException($ev))
            return $ev;
    }
}