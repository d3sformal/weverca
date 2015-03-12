<?php
/**
 * Language configuration for NOCC
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
 * @subpackage Configuration
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: conf_lang.php 2495 2011-07-25 12:26:26Z gerundt $
 */

// ################### Language Array  ################### //
// If you add language files in 'lang/' folder, please list them here

class lang {
  var $filename="";
  var $label="";
}

//TODO: Move to "lang" class?
if (!isset($lang_dir)) { //if NO language direction defined...
  $lang_dir = 'ltr';
}

$i = 0;

//WEVERCA
//// English
$i++;
$lang_array[$i] = new lang();
$lang_array[$i]->filename = 'en';
$lang_array[$i]->label = 'English';

?>
