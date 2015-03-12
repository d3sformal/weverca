<?php
/**
 * Class for wrapping a imap_headerinfo() object
 *
 * Copyright 2009-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: nocc_headerinfo.php 2373 2011-01-04 15:06:58Z gerundt $
 */

/**
 * Wrapping a imap_headerinfo() object
 * 
 * @package    NOCC
 */
class NOCC_HeaderInfo {
    /**
     * imap_headerinfo() object
     * @var object
     * @access private
     */
    private $_headerinfo;
    
    /**
     * Default charset
     * @var string
     * @access private
     */
    private $_defaultcharset;
    
    /**
     * Initialize the wrapper
     * @param object $headerinfo imap_headerinfo() object
     * @param string $defaultcharset Default charset
     * @todo Throw exception, if no vaild header info? 
     */
    public function __construct($headerinfo, $defaultcharset = 'ISO-8859-1') {
        $this->_headerinfo = $headerinfo;
        $this->_defaultcharset = $defaultcharset;
    }
    
    /**
     * Get the default charset
     * @return string Default charset
     */
    public function getDefaultCharset() {
        return $this->_defaultcharset;
    }
    
    /**
     * Set the default charset
     * @param string $defaultcharset Default charset
     */
    public function setDefaultCharset($defaultcharset) {
        $this->_defaultcharset = $defaultcharset;
    }
    
    /**
     * Get the message id from the header info
     * @return string Message id
     */
    public function getMessageId() {
        if (isset($this->_headerinfo->message_id)) {
            return $this->_headerinfo->message_id;
        }
        return '';
    }
    
    /**
     * Get the subject from the header info8
     * @return string Subject
     */
    public function getSubject() {
        if (isset($this->_headerinfo->subject)) {
            return $this->_decodeMimeHeader($this->_headerinfo->subject, $this->_defaultcharset);
        }
        return '';
    }
    
    /**
     * Get the "From" address from the header info
     * @return string "From" address
     */
    public function getFromAddress() {
        if (isset($this->_headerinfo->fromaddress)) {
            return $this->_decodeMimeHeader($this->_headerinfo->fromaddress, $this->_defaultcharset);
        }
        return '';
    }
    
    /**
     * Get the "To" address from the header info
     * @return string "To" address
     */
    public function getToAddress() {
        if (isset($this->_headerinfo->toaddress)) {
            return $this->_decodeMimeHeader($this->_headerinfo->toaddress, $this->_defaultcharset);
        }
        return '';
    }
    
    /**
     * Get the "Cc" address from the header info
     * @return string "Cc" address
     */
    public function getCcAddress() {
        if (isset($this->_headerinfo->ccaddress)) {
            return $this->_decodeMimeHeader($this->_headerinfo->ccaddress, $this->_defaultcharset);
        }
        return '';
    }
    
    /**
     * Get the "Reply-To" address from the header info
     * @return string "Reply-To" address
     */
    public function getReplyToAddress() {
        if (isset($this->_headerinfo->reply_toaddress)) {
            return $this->_decodeMimeHeader($this->_headerinfo->reply_toaddress, $this->_defaultcharset);
        }
        return '';
    }
    
    /**
     * Get the date (in Unix time) from the header info
     * @return int Date in Unix time
     */
    public function getTimestamp() {
        if (isset($this->_headerinfo->udate)) {
            return $this->_headerinfo->udate;
        }
        return 0;
    }
    
    /**
     * Is the mail unread?
     * @return boolean Is unread?
     */
    public function isUnread() {
        if (($this->_headerinfo->Unseen == 'U') || ($this->_headerinfo->Recent == 'N')) {
            return true;
        }
        return false;
    }
    
    /**
     * Is the mail flagged?
     * @return boolean Is flagged?
     */
    public function isFlagged() {
        if ($this->_headerinfo->Flagged == 'F') {
            return true;
        }
        return false;
    }
    
    /**
     * Decode MIME header
     * @param string $mimeheader Encoded MIME header
     * @param string $defaultcharset Default charset
     * @return string Decoded MIME header
     * @access private
     */
    private function _decodeMimeHeader($mimeheader, $defaultcharset) {
        $decodedheader = '';
        if (isset($mimeheader)) {
            $mimeheader = str_replace('x-unknown', $defaultcharset, $mimeheader);
            
            $decoded = nocc_imap::mime_header_decode($mimeheader);
            for ($i = 0; $i < count($decoded); $i++) { //for all elements...
                $decodedheader .= $decoded[$i]->text;
            }
        }
        return $decodedheader;
    }
}
?>
