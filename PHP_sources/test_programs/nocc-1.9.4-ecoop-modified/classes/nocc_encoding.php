<?php
/**
 * Class for wrapping the encoding from a imap_fetchstructure() object
 *
 * Copyright 2010-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: nocc_encoding.php 2373 2011-01-04 15:06:58Z gerundt $
 */

/**
 * Wrapping the encoding from a imap_fetchstructure() object
 * 
 * @package    NOCC
 * @todo: Add is7Bit() function.
 * @todo: Add is8Bit() function.
 * @todo: Add isBinary() function.
 * @todo: Add isBase64() function.
 * @todo: Add isQuotedPrintable() function.
 * @todo: Add isOther() function.
 */
class NOCC_Encoding {
    /**
     * Encoding
     * @var integer
     * @access private
     */
    private $_encoding;

    /**
     * Initialize the wrapper
     * @param integer $encoding Encoding
     */
    public function __construct($encoding = null) {
        $this->_encoding = -1;
        if (is_int($encoding)) { //if valid type...
            $this->_encoding = $encoding;
        }
    }

    /**
     * ...
     * @return string Encoding text
     */
    public function __toString() {
        switch($this->_encoding) {
            case 0: return '7BIT';
            case 1: return '8BIT';
            case 2: return 'BINARY';
            case 3: return 'BASE64';
            case 4: return 'QUOTED-PRINTABLE';
            case 5: return 'OTHER';
        }
        return '';
    }
}
?>
