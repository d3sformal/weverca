<?php
/**
 * Class with functions to modify a mail body
 *
 * Copyright 2010-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: nocc_body.php 2567 2013-08-06 10:44:40Z oheil $
 */

/**
 * Functions to modify a mail body
 *
 * @package    NOCC
 */
class NOCC_Body {
    /**
     * Prepare HTML links
     * @param string $body Mail body
     * @return string Mail body with prepared HMTL links
     * @static
     */
    public static function prepareHtmlLinks($body) {
        $body = preg_replace("|href=\"mailto:([a-zA-Z0-9\+\-=%&:_.~\?@]+[#a-zA-Z0-9\+]*)\"|i", "href=\"action.php?".NOCC_Session::getUrlGetSession()."&amp;action=write&amp;mail_to=$1\"", $body);
        $body = preg_replace("|href=mailto:([a-zA-Z0-9\+\-=%&:_.~\?@]+[#a-zA-Z0-9\+]*)|i", "href=\"action.php?".NOCC_Session::getUrlGetSession()."&amp;action=write&amp;mail_to=$1\"", $body);
        $body = preg_replace("|href=\"([a-zA-Z0-9\+\/\;\-=%&:_.~\?]+[#a-zA-Z0-9\+]*)\"|i", "href=\"$1\" target=\"_blank\"", $body);
        $body = preg_replace("|href=([a-zA-Z0-9\+\/\;\-=%&:_.~\?]+[#a-zA-Z0-9\+]*)|i", "href=\"$1\" target=\"_blank\"", $body);
        return $body;
    }

    /**
     * Prepare text links
     * @param string $body Mail body (prepared with htmlspecialchars())
     * @return string Mail body with prepared text links
     * @static
     */
    public static function prepareTextLinks($body) {
        $htmlEntities = array('&quot;', '&lt;', '&gt;');
        $noccEntities = array('«quot»', '«lt»', '«gt»');

        $body = str_replace($htmlEntities, $noccEntities, $body);
        $body = preg_replace("{(http|https|ftp)://([a-zA-Z0-9\+\/\;\-=%&:_.~\?]+[#a-zA-Z0-9\+:]*)}i", "<a href=\"$1://$2\" target=\"_blank\">$1://$2</a>", $body);
        $body = preg_replace("/([0-9a-zA-Z]([-_.]?[0-9a-zA-Z])*@[0-9a-zA-Z]([-.]?[0-9a-zA-Z])*\.[a-zA-Z]{2,})/", "<a href=\"action.php?".NOCC_Session::getUrlGetSession()."&amp;action=write&amp;mail_to=\\1\">\\1</a>", $body);
        $body = str_replace($noccEntities, $htmlEntities, $body);
        return $body;
    }

    /**
     * Add colored quotes
     * @param string $body Mail body (prepared with htmlspecialchars())
     * @return string Mail body with colored quotes
     * @static
     */
    public static function addColoredQuotes($body) {
        $body = preg_replace('/^(&gt; *&gt; *&gt; *&gt; *&gt;)(.*?)(\r?\n)/m', '<span class="quoteLevel5">\\1\\2</span>\\3', $body);
        $body = preg_replace('/^(&gt; *&gt; *&gt; *&gt;)(.*?)(\r?\n)/m', '<span class="quoteLevel4">\\1\\2</span>\\3', $body);
        $body = preg_replace('/^(&gt; *&gt; *&gt;)(.*?)(\r?\n)/m', '<span class="quoteLevel3">\\1\\2</span>\\3', $body);
        $body = preg_replace('/^(&gt; *&gt;)(.*?)(\r?\n)/m', '<span class="quoteLevel2">\\1\\2</span>\\3', $body);
        $body = preg_replace('/^(&gt;)(.*?)(\r?\n)/m', '<span class="quoteLevel1">\\1\\2</span>\\3', $body);
        return $body;
    }

    /**
     * Add structured text
     * @param string $body Mail body
     * @return string Mail body with structured text
     * @static
     */
    public static function addStructuredText($body) {
        $body = preg_replace('/(\s)\+\/-/', '\\1&plusmn;', $body); // +/-
        $body = preg_replace('/(\w|\))\^([0-9]+)/', '\\1<sup>\\2</sup>', $body); // 10^6, a^2, (a+b)^2
        $body = preg_replace('/(\s)(\*)([^\s\*]+[^\*\r\n]+)(\*)/', '\\1<strong>\\2\\3\\4</strong>', $body); // *strong*
        $body = preg_replace('/(\s)(\/)([^\s\/]+[^\/\r\n<>]+)(\/)/', '\\1<em>\\2\\3\\4</em>', $body); // /emphasis/
        $body = preg_replace('/(\s)(_)([^\s_]+[^_\r\n]+)(_)/', '\\1<span style="text-decoration:underline">\\2\\3\\4</span>', $body); // _underline_
        $body = preg_replace('/(\s)(\|)([^\s\|]+[^\|\r\n]+)(\|)/', '\\1<code>\\2\\3\\4</code>', $body); // |code|
        return $body;
    }
}
?>
