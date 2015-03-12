<?php
/**
 * Charset configuration for NOCC
 *
 * Copyright 2001 Nicolas Chalanset <nicocha@free.fr>
 * Copyright 2001 Olivier Cahagne <cahagn_o@epita.fr>
 * Copyright 2005 Arnaud Boudou <goddess_skuld@users.sourceforge.net>
 * Copyright 2008-2011 Tim Gerundt <tim@gerundt.de>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @subpackage Configuration
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: conf_charset.php 2580 2013-08-19 21:57:33Z gerundt $
 */

/* Windows charsets as disabled as libiconv can't manage them */

class charset {
  var $charset = '';
  var $group = '';
  var $label = '';
}

$i = 0;

$charset_array[$i] = new charset();
$charset_array[$i]->charset = '';
$charset_array[$i]->group = '';
$charset_array[$i]->label = '----------';

// Arabic - ISO (ISO-8859-6)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-6';
$charset_array[$i]->group = 'Arabic';
$charset_array[$i]->label = 'Arabic (ISO-8859-6)';

// Arabic - Mac (MacArabic)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'MacArabic';
$charset_array[$i]->group = 'Arabic';
$charset_array[$i]->label = 'Arabic (MacArabic)';

// Arabic - Windows (WINDOWS-1256)
//$i++;
//$charset_array[$i] = new charset();
//$charset_array[$i]->charset = 'WINDOWS-1256';
//$charset_array[$i]->group = 'Arabic';
//$charset_array[$i]->label = 'Arabic (Windows-1256)';

// Armenian - ARMSCII (ARMSCII)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ARMSCII';
$charset_array[$i]->group = 'Armenian';
$charset_array[$i]->label = 'Armenian (ARMSCII)';

// Baltic - ISO (ISO-8859-13)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-13';
$charset_array[$i]->group = 'Baltic';
$charset_array[$i]->label = 'Baltic (ISO-8859-13)';

// Baltic - Windows (WINDOWS-1257)
//$i++;
//$charset_array[$i] = new charset();
//$charset_array[$i]->charset = 'WINDOWS-1257';
//$charset_array[$i]->group = 'Baltic';
//$charset_array[$i]->label = 'Baltic (Windows-1257)';

// Celtic - ISO (ISO-8859-14)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-14';
$charset_array[$i]->group = 'Celtic';
$charset_array[$i]->label = 'Celtic (ISO-8859-14)';

// Central European - ISO (ISO-8859-2)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-2';
$charset_array[$i]->group = 'Central European';
$charset_array[$i]->label = 'Central European (ISO-8859-2)';

// Chinese, simplified - GB (GB18030)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'GB18030';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, simplified (GB18030)';

// Chinese, simplified - GB (GB2312)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'GB2312';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, simplified (GB2312)';

// Chinese, simplified - GB (GBK)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'GBK';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, simplified (GBK)';

// Chinese, simplified - HZ (HZ)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'HZ';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, simplified (HZ)';

// Chinese, simplified - ISO (ISO-2022-CN)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-2022-CN';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, simplified (ISO-2022-CN)';

// Chinese, simplified - ISO (ISO-2022-CN-EXT)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-2022-CN-EXT';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, simplified (ISO-2022-CN-EXT)';

// Chinese, simplified - Unix (EUC-CN)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'EUC-CN';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, simplified (EUC-CN)';

// Chinese, traditional - BIG5 (BIG5)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'BIG5';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, traditional (BIG5)';

// Chinese, traditional - BIG5 (BIG5-HKSCS)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'BIG5-HKSCS';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, traditional (BIG5-HKSCS)';

// Chinese, traditional - Unix (EUC-TW)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'EUC-TW';
$charset_array[$i]->group = 'Chinese';
$charset_array[$i]->label = 'Chinese, traditional (EUC-TW)';

// Croatian - Mac (MacCroatian)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'MacCroatian';
$charset_array[$i]->group = 'Croatian';
$charset_array[$i]->label = 'Croatian (MacCroatian)';

// Cyrillic - ISO (ISO-8859-5)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-5';
$charset_array[$i]->group = 'Cyrillic';
$charset_array[$i]->label = 'Cyrillic (ISO-8859-5)';

// Cyrillic - ISO (ISO-IR-111)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-IR-111';
$charset_array[$i]->group = 'Cyrillic';
$charset_array[$i]->label = 'Cyrillic (ISO-IR-111)';

// Cyrillic - Mac (MacCyrillic)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'MacCyrillic';
$charset_array[$i]->group = 'Cyrillic';
$charset_array[$i]->label = 'Cyrillic (MacCyrillic)';

// Cyrillic - Windows (WINDOWS-1251)
//$i++;
//$charset_array[$i] = new charset();
//$charset_array[$i]->charset = 'WINDOWS-1251';
//$charset_array[$i]->group = 'Cyrillic';
//$charset_array[$i]->label = 'Cyrillic (Windows-1251)';

// Eastern European - Windows (WINDOWS-1250)
//$i++;
//$charset_array[$i] = new charset();
//$charset_array[$i]->charset = 'WINDOWS-1250';
//$charset_array[$i]->group = 'Eastern European';
//$charset_array[$i]->label = 'Eastern European (Windows-1250)';

// Greek - ISO (ISO-8859-7)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-7';
$charset_array[$i]->group = 'Greek';
$charset_array[$i]->label = 'Greek (ISO-8859-7)';

// Greek - Mac (MacGreek)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'MacGreek';
$charset_array[$i]->group = 'Greek';
$charset_array[$i]->label = 'Greek (MacGreek)';

// Greek - Windows (WINDOWS-1253)
//$i++;
//$charset_array[$i] = new charset();
//$charset_array[$i]->charset = 'WINDOWS-1253';
//$charset_array[$i]->group = 'Greek';
//$charset_array[$i]->label = 'Greek (Windows-1253)';

// Hebrew - ISO (ISO-8859-8-I)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-8-I';
$charset_array[$i]->group = 'Hebrew';
$charset_array[$i]->label = 'Hebrew (ISO-8859-8-I)';

// Hebrew - Mac (MacHebrew)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'MacHebrew';
$charset_array[$i]->group = 'Hebrew';
$charset_array[$i]->label = 'Hebrew (MacHebrew)';

// Hebrew, visual - ISO (ISO-8859-8)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-8';
$charset_array[$i]->group = 'Hebrew';
$charset_array[$i]->label = 'Hebrew, visual (ISO-8859-8)';

// Hebrew - Windows (WINDOWS-1255)
//$i++;
//$charset_array[$i] = new charset();
//$charset_array[$i]->charset = 'WINDOWS-1255';
//$charset_array[$i]->group = 'Hebrew';
//$charset_array[$i]->label = 'Hebrew (Windows-1255)';

// Icelandic - Mac (MacIcelandic)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'MacIcelandic';
$charset_array[$i]->group = 'Icelandic';
$charset_array[$i]->label = 'Icelandic (MacIcelandic)';

// International - UTF-8 (UTF-8)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'UTF-8';
$charset_array[$i]->group = 'International';
$charset_array[$i]->label = 'International (UTF-8)';

// Japanese - ISO (ISO-2022-JP)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-2022-JP';
$charset_array[$i]->group = 'Japanese';
$charset_array[$i]->label = 'Japanese (ISO-2022-JP)';

// Japanese - ISO (ISO-2022-JP-1)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-2022-JP-1';
$charset_array[$i]->group = 'Japanese';
$charset_array[$i]->label = 'Japanese (ISO-2022-JP-1)';

// Japanese - ISO (ISO-2022-JP-2)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-2022-JP-2';
$charset_array[$i]->group = 'Japanese';
$charset_array[$i]->label = 'Japanese (ISO-2022-JP-2)';

// Japanese - ISO (ISO-2022-JP-3)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-2022-JP-3';
$charset_array[$i]->group = 'Japanese';
$charset_array[$i]->label = 'Japanese (ISO-2022-JP-3)';

// Japanese - Shift JIS (Shift_JIS)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'Shift_JIS';
$charset_array[$i]->group = 'Japanese';
$charset_array[$i]->label = 'Japanese (Shift_JIS)';

// Japanese - Unix (EUC-JP)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'EUC-JP';
$charset_array[$i]->group = 'Japanese';
$charset_array[$i]->label = 'Japanese (EUC-JP)';

// Korean - ISO (ISO-2022-KR)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-2022-KR';
$charset_array[$i]->group = 'Korean';
$charset_array[$i]->label = 'Korean (ISO-2022-KR)';

// Korean - JOHAB (JOHAB)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'JOHAB';
$charset_array[$i]->group = 'Korean';
$charset_array[$i]->label = 'Korean (JOHAB)';

// Korean - UHC (UHC)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'UHC';
$charset_array[$i]->group = 'Korean';
$charset_array[$i]->label = 'Korean (UHC)';

// Korean - Unix (EUC-KR)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->group = 'Korean';
$charset_array[$i]->charset = 'EUC-KR';
$charset_array[$i]->label = 'Korean (EUC-KR)';

// Nordic - ISO (ISO-8859-10)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-10';
$charset_array[$i]->group = 'Nordic';
$charset_array[$i]->label = 'Nordic (ISO-8859-10)';

// North European - ISO (ISO-8859-4)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-4';
$charset_array[$i]->group = 'North European';
$charset_array[$i]->label = 'North European (ISO-8859-4)';

// Romanian - Mac (MacRomanian)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'MacRomanian';
$charset_array[$i]->group = 'Romanian';
$charset_array[$i]->label = 'Romanian (MacRomanian)';

// Russian - KOI-8 (KOI8-R)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'KOI8-R';
$charset_array[$i]->group = 'Russian';
$charset_array[$i]->label = 'Russian (KOI8-R)';

// South European - ISO (ISO-8859-3)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-3';
$charset_array[$i]->group = 'South European';
$charset_array[$i]->label = 'South European (ISO-8859-3)';

// South-Eastern European - ISO (ISO-8859-16)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-16';
$charset_array[$i]->group = 'South-Eastern European';
$charset_array[$i]->label = 'South-Eastern European (ISO-8859-16)';

// Thai - ISO (ISO-8859-11)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-11';
$charset_array[$i]->group = 'Thai';
$charset_array[$i]->label = 'Thai (ISO-8859-11)';

// Thai - TIS (TIS-620)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'TIS-620';
$charset_array[$i]->group = 'Thai';
$charset_array[$i]->label = 'Thai (TIS-620)';

// Turkish - ISO (ISO-8859-9)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-9';
$charset_array[$i]->group = 'Turkish';
$charset_array[$i]->label = 'Turkish (ISO-8859-9)';

// Turkish - Mac (MacTurkish)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'MacTurkish';
$charset_array[$i]->group = 'Turkish';
$charset_array[$i]->label = 'Turkish (MacTurkish)';

// Turkish - Windows (WINDOWS-1254)
//$i++;
//$charset_array[$i] = new charset();
//$charset_array[$i]->charset = 'WINDOWS-1254';
//$charset_array[$i]->group = 'Turkish';
//$charset_array[$i]->label = 'Turkish (Windows-1254)';

// Ukrainian - KOI8 (KOI8-U)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset ="KOI8-U";
$charset_array[$i]->group = 'Ukrainian';
$charset_array[$i]->label = 'Ukrainian (KOI8-U)';

// Ukrainian - Mac (MacUkrainian)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset ="MacUkrainian";
$charset_array[$i]->group = 'Ukrainian';
$charset_array[$i]->label = 'Ukrainian (MacUkrainian)';

// Vietnamese - TCVN (TCVN)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'TCVN';
$charset_array[$i]->group = 'Vietnamese';
$charset_array[$i]->label = 'Vietnamese (TCVN)';

// Vietnamese - VISCII (VISCII)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'VISCII';
$charset_array[$i]->group = 'Vietnamese';
$charset_array[$i]->label = 'Vietnamese (VISCII)';

// Vietnamese - Windows (WINDOWS-1258)
//$i++;
//$charset_array[$i] = new charset();
//$charset_array[$i]->charset = 'WINDOWS-1258';
//$charset_array[$i]->group = 'Vietnamese';
//$charset_array[$i]->label = 'Vietnamese (Windows-1258)';

// Western European - ISO Latin 1 (ISO-8859-1)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-1';
$charset_array[$i]->group = 'Western European';
$charset_array[$i]->label = 'Western European (ISO-8859-1)';

// Western European - ISO Latin 9 (ISO-8859-15)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'ISO-8859-15';
$charset_array[$i]->group = 'Western European';
$charset_array[$i]->label = 'Western European (ISO-8859-15)';

// Western European - Mac (MacRoman)
$i++;
$charset_array[$i] = new charset();
$charset_array[$i]->charset = 'MacRoman';
$charset_array[$i]->group = 'Western European';
$charset_array[$i]->label = 'Western European (MacRoman)';

// Western European - Windows (WINDOWS-1252)
//$i++;
//$charset_array[$i] = new charset();
//$charset_array[$i]->charset = 'WINDOWS-1252';
//$charset_array[$i]->group = 'Western European';
//$charset_array[$i]->label = 'Western European (Windows-1252)';
