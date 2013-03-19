//
// ExtSupport.PHP4 - substitute for php4ts.dll
//
// Spprintf.h
// - slightly modified spprintf.h, originally PHP source file
//

#pragma once

#include "stdafx.h"
#include "ExtSupport.h"
#include "Snprintf.h"

#include <stdarg.h>

/*
   +----------------------------------------------------------------------+
   | PHP Version 4                                                        |
   +----------------------------------------------------------------------+
   | Copyright (c) 1997-2003 The PHP Group                                |
   +----------------------------------------------------------------------+
   | This source file is subject to version 2.02 of the PHP license,      |
   | that is bundled with this package in the file LICENSE, and is        |
   | available at through the world-wide-web at                           |
   | http://www.php.net/license/2_02.txt.                                 |
   | If you did not receive a copy of the PHP license and are unable to   |
   | obtain it through the world-wide-web, please send a note to          |
   | license@php.net so we can mail you a copy immediately.               |
   +----------------------------------------------------------------------+
   | Author: Marcus Boerger <helly@php.net>                               |
   +----------------------------------------------------------------------+
 */

/* 

The pbuf parameter of all spprintf version receives a pointer to the allocated
buffer. This buffer must be freed manually after usage using efree() function.
The buffer will allways be terminated by a zero character. When pbuf is NULL
the function can be used to calculate the required size of the buffer but for
that purpose snprintf is faster. When both pbuf and the return value are 0
than you are out of memory.

There is also snprintf: See difference explained in snprintf.h

*/

#ifdef __cplusplus
extern "C"
{
#endif

ZEND_API int spprintf( char **pbuf, size_t max_len, const char *format, ...);
ZEND_API int vspprintf(char **pbuf, size_t max_len, const char *format, va_list ap);

#ifdef __cplusplus
}
#endif

#define zend_sprintf sprintf