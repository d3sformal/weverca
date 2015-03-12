<?php
/**
 * Class for wrapping a imap_fetchheader() string
 *
 * Copyright 2009-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: nocc_header.php 2524 2011-12-15 15:11:08Z gerundt $
 */

/**
 * Wrapping a imap_fetchheader() string
 *
 * @package    NOCC
 */
class NOCC_Header {
    /**
     * imap_fetchheader() string
     * @var string
     * @access private
     */
    private $_header;
    /**
     * Priority
     * @var integer
     * @access private
     */
    private $_priority;
    /**
     * Content-Type
     * @var string
     * @access private
     */
    private $_contenttype;
    /**
     * SPAM flag
     * @var bool
     * @access private
     */
    private $_spamflag;
    /**
     * Status (UCB POP Server)
     * @var string
     * @access private
     */
    private $_status;

    /**
     * Initialize the wrapper
     * @param string $header imap_fetchheader() string
     */
    public function __construct($header) {
        $this->_header = $header;
        $this->_priority = 3;
        $this->_contenttype = '';
        $this->_spamflag = false;
        $this->_status = '';

        $header_lines = explode("\r\n", $header);
        foreach ($header_lines as $header_line) { //for all header lines...
            $header_field = explode(':', $header_line);
            switch (strtolower($header_field[0])) {
                case 'x-priority':
                case 'importance':
                case 'priority':
                    $this->_priority = $this->_parsePriority($header_field[1]);
                    break;
                case 'content-type':
                    $content_type = explode(';', $header_field[1]);
                    $this->_contenttype = trim($content_type[0]);
                    break;
                case 'x-spam-flag': //SpamAssassin (default)
                case 'x-virus-status': //ClamAV
                    $value = strtolower(trim($header_field[1]));
                    if ($value == 'yes') {
                        $this->_spamflag = true;
                    }
                    break;
                case 'x-kasspamfilter': //all-inkl.com
                    $this->_spamflag = true;
                    break;
                case 'status':
                    $this->_status = trim($header_field[1]);
                    break;
            }
        }
    }

    /**
     * Get the RFC2822 format header from the mail
     * @return string RFC2822 format header
     */
    public function getHeader() {
        return $this->_header;
    }

    /**
     * Get the priority from the mail
     * @return integer Priority
     */
    public function getPriority() {
        return $this->_priority;
    }

    /**
     * Get the (translated) priority text from the mail
     * @return string Priority text
     */
    public function getPriorityText() {
        global $html_highest, $html_high, $html_normal, $html_low, $html_lowest;

        switch ($this->_priority) {
            case 1: return $html_highest; break;
            case 2: return $html_high; break;
            case 3: return $html_normal; break;
            case 4: return $html_low; break;
            case 5: return $html_lowest; break;
            default: return '';
        }
    }

    /**
     * Get the Content-Type from the mail
     * @return string Content-Type
     */
    public function getContentType() {
        return $this->_contenttype;
    }

    /**
     * Has SPAM flag?
     * @return bool Has SPAM flag?
     */
    public function hasSpamFlag() {
        return $this->_spamflag;
    }

    /**
     * Get the status (UCB POP Server) from the mail
     * @return string Status (UCB POP Server)
     */
    public function getStatus() {
        return $this->_status;
    }

    /**
     * Normalise the different Priority headers into a uniform value,
     * namely that of the X-Priority header (1, 3, 5). Supports:
     * Priority, X-Priority, Importance.
     * X-MS-Mail-Priority is not parsed because it always coincides
     * with one of the other headers.
     *
     * @param string $sValue literal priority name
     * @return integer
     * @access private
     *
     * @copyright &copy; 2003-2007 The SquirrelMail Project Team
     * @license http://opensource.org/licenses/gpl-license.php GNU Public License
     */
    private function _parsePriority($sValue) {
        // don't use function call inside array_shift.
        $aValue = preg_split('/\s/', trim($sValue));
        $value = strtolower(array_shift($aValue));

        if ( is_numeric($value) ) {
            return $value;
        }
        if ( $value == 'urgent' || $value == 'high' ) {
            return 2;
        } elseif ( $value == 'non-urgent' || $value == 'low' ) {
            return 4;
        }
        // default is normal priority
        return 3;
    }
}
?>
