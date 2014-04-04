<?php
/**
 * File for viewing the images
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
 * @version    SVN: $Id: get_img.php 2580 2013-08-19 21:57:33Z gerundt $
 */

require_once './common.php';

try {
    $pop = new nocc_imap();

    $mail = $_REQUEST['mail'];
    $num = $_REQUEST['num'];
    $transfer = $_REQUEST['transfer'];
    $mime = $_REQUEST['mime'];

    $img = $pop->fetchbody($mail, $num);
    $img = nocc_imap::decode(removeUnicodeBOM($img), $transfer);

    $pop->close();

    header('Content-type: image/'.$mime);
    echo $img;
}
catch (Exception $ex) {
    //TODO: Show error without NoccException!
    $ev = new NoccException($ex->getMessage());
    require './html/header.php';
    require './html/error.php';
    require './html/footer.php';
    return;
}