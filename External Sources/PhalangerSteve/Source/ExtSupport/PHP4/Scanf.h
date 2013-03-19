//
// ExtSupport.PHP4 - substitute for php4ts.dll
//
// Scanf.h
// - slightly modified scanf.h, originally PHP source file
//

#pragma once

#include "stdafx.h"
#include "ExtSupport.h"

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
   | Author: Clayton Collie <clcollie@mindspring.com>                     |
   +----------------------------------------------------------------------+
*/

/* $Id: Scanf.h,v 1.1.2.2 2006/04/15 20:19:12 prosl0am Exp $ */

#ifndef  SCANF_H
#define  SCANF_H


#define SCAN_MAX_ARGS   0xFF    /* Maximum number of variable which can be      */
                                /* passed to (f|s)scanf. This is an artifical   */
                                /* upper limit to keep resources in check and   */
                                /* minimize the possibility of exploits         */

#define SCAN_MAX_FSCANF_BUFSIZE		512  /* Max input buffer allocated for fscanf */
#define SCAN_SUCCESS			SUCCESS	
#define SCAN_ERROR_EOF			-1	/* indicates premature termination of scan 	*/
									/* can be caused by bad parameters or format*/
									/* string.									*/
#define SCAN_ERROR_INVALID_FORMAT		(SCAN_ERROR_EOF - 1)
#define SCAN_ERROR_VAR_PASSED_BYVAL		(SCAN_ERROR_INVALID_FORMAT - 1)
#define SCAN_ERROR_WRONG_PARAM_COUNT	(SCAN_ERROR_VAR_PASSED_BYVAL - 1)
#define SCAN_ERROR_INTERNAL             (SCAN_ERROR_WRONG_PARAM_COUNT - 1)


#ifdef __cplusplus
extern "C"
{
#endif

/*  
 * The following are here solely for the benefit of the scanf type functions
 * e.g. fscanf
 */
ZEND_API int ValidateFormat(char *format, int numVars, int *totalVars);
ZEND_API int php_sscanf_internal(char *string,char *format,int argCount,zval ***args,
				int varStart, pval **return_value TSRMLS_DC);

#ifdef __cplusplus
}
#endif

#endif /* SCANF_H */
