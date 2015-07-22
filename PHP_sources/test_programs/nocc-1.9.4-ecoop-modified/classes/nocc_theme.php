<?php
/**
 * Class for wrapping a theme
 *
 * Copyright 2009-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: nocc_theme.php 2580 2013-08-19 21:57:33Z gerundt $
 */

/**
 * Wrapping details from a theme
 *
 * @package    NOCC
 */
class NOCC_Theme {
    /**
     * Name
     * @var string
     * @access private
     */
    private $_name;
    
    /**
     * Path
     * @var string
     * @access private
     */
    private $_path;
    
    /**
     * Real path
     * @var string
     * @access private
     */
    private $_realpath;
    
    /**
     * Exists?
     * @var bool
     * @access private
     */
    private $_exists;
    
    /**
     * Initialize the theme wrapper
     * @param string $name Theme name
     */
    public function __construct($name) {
        $this->_name = '';
        $this->_path = '';
        $this->_realpath = '';
        $this->_exists = false;
        
        $name = strip_tags($name);
        $name = str_replace('..', '', $name);
        $name = str_replace('/', '', $name);
        if (!empty($name)) { //if the name exists...
            $this->_name = $name;
            $path = 'themes/' . $name;
            $realpath = realpath($path);
            if (!empty($realpath)) { //if the real path exists...
                $this->_path = $path;
                $this->_realpath = $realpath;
                $this->_exists = true;
            }
        }
    }
    
    /**
     * Get the name from the theme
     * @return string Name
     */
    public function getName() {
        return $this->_name;
    }
    
    /**
     * Get the path from the theme
     * @return string Path
     */
    public function getPath() {
        return $this->_path;
    }
    
    /**
     * Get the real path from the theme
     * @return string Real path
     */
    public function getRealPath() {
        return $this->_realpath;
    }
    
    /**
     * Exists the theme?
     * @return bool Exists?
     */
    public function exists() {
        return $this->_exists;
    }
    
    /**
     * Get the stylesheet from the theme
     * @return string Stylesheet
     */
    public function getStylesheet() {
        if ($this->_exists) { //if exists...
            return $this->_path . '/style.css';
        }
        return '';
    }
    
    /**
     * Get the print stylesheet from the theme
     * @return string Print stylesheet
     */
    public function getPrintStylesheet() {
        if ($this->_exists) { //if exists...
            return $this->_path . '/print.css';
        }
        return '';
    }
    
    /**
     * Get the favicon from the theme
     * @return string Favicon
     */
    public function getFavicon() {
        if (file_exists($this->_realpath . '/favicon.ico')) //if theme favicon exists...
            return $this->_path . '/favicon.ico';
        else //if NO theme favicon exists...
            return 'favicon.ico';
    }
    
    /**
     * Get the custom header from the theme
     * @return string Custom header
     */
    public function getCustomHeader() {
        if ($this->_exists) { //if exists...
            return $this->_realpath . '/header.php';
        }
        return '';
    }
    
    /**
     * Get the custom footer from the theme
     * @return string Custom footer
     */
    public function getCustomFooter() {
        if ($this->_exists) { //if exists...
            return $this->_realpath . '/footer.php';
        }
        return '';
    }
    
    /**
     * Replace text smilies with graphical smilies
     * @param string $body String with text smilies
     * @return string String with graphical smilies (HTML)
     */
    public function replaceTextSmilies($body) {
        $smiliespath = $this->_path . '/img/smilies';
        
        $body = preg_replace('|\;\-?\)|', '<img src="' . $smiliespath . '/wink.png" alt="wink"/>', $body); // ;-) ;)
        $body = preg_replace('|\;\-?D|', '<img src="' . $smiliespath . '/grin.png" alt="grin"/>', $body); // ;-D ;D
        $body = preg_replace('|:\'\(?|', '<img src="' . $smiliespath . '/cry.png" alt="cry"/>', $body); // :'( :'
        $body = preg_replace('|:\-?X|i', '<img src="' . $smiliespath . '/confused.png" alt="confused"/>', $body); // :-x :X
        $body = preg_replace('|:\-?\[\)|', '<img src="' . $smiliespath . '/embarassed.png" alt="embarassed"/>', $body); // :-[) :[)
        $body = preg_replace('|:\-?\*|', '<img src="' . $smiliespath . '/love.png" alt="love"/>', $body); // :-* :*
        $body = preg_replace('|:\-?P|i', '<img src="' . $smiliespath . '/tongue.png" alt="tongue"/>', $body); // :-p :P
        $body = preg_replace('|:\-?\)|', '<img src="' . $smiliespath . '/happy.png" alt="happy"/>', $body); // :-) :)
        $body = preg_replace('|:-\?\(|', '<img src="' . $smiliespath . '/unhappy.png" alt="unhappy"/>', $body); // :-( :(
        $body = preg_replace('|:\-O|i', '<img src="' . $smiliespath . '/surprised.png" alt="surprised"/>', $body); // :-o :-O
        $body = preg_replace('|8\-?\)|', '<img src="' . $smiliespath . '/cool.png" alt="cool"/>', $body); // 8-) 8)
        
        return $body;
    }
}