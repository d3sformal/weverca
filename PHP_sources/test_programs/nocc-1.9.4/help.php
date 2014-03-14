<?php
/**
 * Help
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
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: help.php 2373 2011-01-04 15:06:58Z gerundt $
 */

require_once './common.php';
$lang = $_SESSION['nocc_lang'];

$theme = new NOCC_Theme($_SESSION['nocc_theme']);
?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="<?php echo $lang ?>" lang="<?php echo $lang ?>">
<head>
  <title>NOCC - Webmail - <?php echo $html_help ?></title>
  <meta content="text/html; charset=UTF-8" http-equiv="Content-Type" />
  <link href="<?php echo $theme->getStylesheet(); ?>" rel="stylesheet" type="text/css" />
</head>
<body dir="<?php echo $lang_dir; ?>">

</body>
</html>
