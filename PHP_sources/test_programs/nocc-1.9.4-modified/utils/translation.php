<?php
/**
 * Adds additional string formatting for better i18n support
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
 * @subpackage Utilities
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id$
 */


  function i18n_message($func_str_translation = null, $func_str_insert = null, $func_convert = 1) {
  
    if ((!is_null($func_str_translation)) && (!is_null($func_str_insert))) {

      if (is_array($func_str_insert)) {
        $func_output = vsprintf($func_str_translation, $func_str_insert);
      } else {
        $func_output = sprintf($func_str_translation, $func_str_insert);
      }
      
      if ($func_convert == 1) {
        $func_output = convertLang2Html($func_output);
      }
      
      return $func_output;
    }

    return false;
  }