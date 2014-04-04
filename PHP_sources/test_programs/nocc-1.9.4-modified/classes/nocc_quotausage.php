<?php
/**
 * Class for wrapping a imap_get_quotaroot() array
 *
 * Copyright 2009-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: nocc_quotausage.php 2580 2013-08-19 21:57:33Z gerundt $
 */

/**
 * Wrapping a imap_get_quotaroot() array
 *
 * @package    NOCC
 */
class NOCC_QuotaUsage {
    /**
     * imap_get_quotaroot() array
     * @var array
     * @access private
     */
    private $_quotausage;
    
    /**
     * Initialize the wrapper
     * @param array $quotausage imap_get_quotaroot() array
     */
    public function __construct($quotausage) {
        $this->_quotausage = $quotausage;
    }
    
    /**
     * Get the current size of the mailbox in KB
     * @return int Current mailbox size in KB
     */
    public function getStorageUsage() {
        if (isset($this->_quotausage['STORAGE']['usage'])) {
            return $this->_quotausage['STORAGE']['usage'];
        }
        return 0;
    }
    
    /**
     * Get the current formatted size of the mailbox in KB/MB/GB
     * @return int Current formatted mailbox size in KB/MB/GB
     */
    public function getFormattedStorageUsage() {
        return $this->getFormattedSize($this->getStorageUsage());
    }
    
    /**
     * Get the maximum size of the mailbox in KB
     * @return int Maximum mailbox size in KB
     */
    public function getStorageLimit() {
        if (isset($this->_quotausage['STORAGE']['limit'])) {
            return $this->_quotausage['STORAGE']['limit'];
        }
        return 0;
    }
    
    /**
     * Get the maximum formatted size of the mailbox in KB/MB/GB
     * @return int Maximum formatted mailbox size in KB/MB/GB
     */
    public function getFormattedStorageLimit() {
        return $this->getFormattedSize($this->getStorageLimit());
    }
    
    /**
     * Get the current number of messages in the mailbox
     * @return int Current number of mailbox messages
     */
    public function getMessageUsage() {
        if (isset($this->_quotausage['MESSAGE']['usage'])) {
            return $this->_quotausage['MESSAGE']['usage'];
        }
        return 0;
    }
    
    /**
     * Get the maximum number of messages in the mailbox
     * @return int Maximum number of mailbox messages
     */
    public function getMessageLimit() {
        if (isset($this->_quotausage['MESSAGE']['limit'])) {
            return $this->_quotausage['MESSAGE']['limit'];
        }
        return 0;
    }
    
    /**
     * Is the quota usage supported?
     * @return bool Is supported?
     */
    public function isSupported() {
        return is_array($this->_quotausage);
    }
    
    /**
     * Get a formatted size from a kilobyte number in KB/MB/GB
     * @param int $kilobytes Kilobyte number
     * @return string Formatted size in KB/MB/GB
     */
    public function getFormattedSize($kilobytes) {
        // load translated abbreviations
        global $html_kb, $html_mb, $html_gb;

        if ($kilobytes >= 1048576) { //If gigabytes...
            $size = round($kilobytes / 1024 / 1024);
            return $size . ' ' . $html_gb;
        }
        elseif (($kilobytes < 1048576) && ($kilobytes >= 1024)) { //If megabytes...
            $size = round($kilobytes / 1024);
            return $size . ' ' . $html_mb;
        }
        else { //If kilobytes...
            return $kilobytes . ' ' . $html_kb;
        }
    }
}