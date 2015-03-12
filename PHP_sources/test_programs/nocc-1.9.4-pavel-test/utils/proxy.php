<?php
/**
 * Sends HTTP headers to forbid transparent proxies, HTTP/1.x proxies to
 * to cache answers from the server running NOCC.
 *
 * This is quite aggressive, we could have set Cache-control to private
 * to forbid only proxy to cache answers but this would allow browser
 * to be able to cache answers; given that some people use NOCC from
 * a public computer, Cache-control is set to no-cache to prevent any
 * caching. This might lower NOCC speed but it's hard to be both secure
 * and cache-friendly when dealing with dynamic content.
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
 * @version    SVN: $Id: proxy.php 2580 2013-08-19 21:57:33Z gerundt $
 */

header("Pragma: no-cache");
header("Cache-control: no-cache");