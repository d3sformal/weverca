<?php
/**
 * Class for wrapping a attached file
 *
 * Copyright 2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: nocc_attachedfile.php 2552 2012-05-28 13:23:16Z gerundt $
 */

/**
 * Wrapping a attached file
 *
 * @package    NOCC
 */
class NOCC_AttachedFile {
    /**
     * Temp file path
     * @var string
     */
    private $tmpFile = '';
    /**
     * File name
     * @var string 
     */
    private $name = '';
    /**
     * Bytes
     * @var integer
     */
    private $bytes = 0;
    /**
     * MIME type
     * @var string 
     */
    private $mimeType = '';
    
    /**
     * ...
     * @param string $tmpFile Temp file path
     * @param string $name File name
     * @param integer $bytes File size in bytes
     * @param string $mimeType MIME type
     */
    public function __construct($tmpFile, $name, $bytes, $mimeType) {
        $this->tmpFile = $tmpFile;
        $this->name = $name;
        $this->bytes = $bytes;
        $this->mimeType = $mimeType;
        if (empty($mimeType)) {
            $attachedFile->mimeType = trim(`file -b $tmpFile`);
        }
    }
    
    /**
     * Get the temp file path from the attached file
     * @return string Temp file path
     */
    public function getTmpFile() {
        return $this->tmpFile;
    }
    
    /**
     * Get the name from the attached file
     * @return string File name
     */
    public function getName() {
        return $this->name;
    }
    
    /**
     * Get the number of bytes from the attached file
     * @return integer Number of bytes
     */
    public function getBytes() {
        return $this->bytes;
    }
    
    /**
     * Get the size from the attached file in kilobyte
     * @return integer Size in kilobyte
     */
    public function getSize() {
        if ($this->bytes > 1024) { //if more then 1024 bytes...
            return ceil($this->bytes / 1024);
        }
        return 1;
    }
    
    /**
     * Get the MIME type from the attached file
     * @return type MIME type
     */
    public function getMimeType() {
        return $this->mimeType;
    }
    
    /**
     * ...
     * @return bool Exists?
     */
    public function exists() {
        return file_exists($this->tmpFile);
    }
    
    /**
     * ...
     * @return string Content
     */
    public function getContent() {
        if ($this->exists()) {
            $fp = fopen($this->tmpFile, 'rb');
            //TODO: Check if the file size is 0!
            $content = fread($fp, $this->bytes);
            fclose($fp);
            
            return $content;
        }
        return '';
    }
    
    /**
     * ...
     */
    public function delete() {
        @unlink($this->tmpFile);
    }
}
?>
