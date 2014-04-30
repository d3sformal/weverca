<?php
/*
 * Fake definition of templates (just for verification purposes).
 */
class Template {
	
    function Template($root = ".")
    {
    }
	
    /**
     * Destroys this template object. Should be called when you're done with it, in order
     * to clear out the template data so you can load/parse a new template set.
     */
    function destroy()
    {
    }

    /**
     * Sets the template root directory for this Template object.
     */
    function set_rootdir($dir)
    {
        return true;
    }

    /**
     * Sets the template filenames for handles. $filename_array
     * should be a hash of handle => filename pairs.
     */
    function set_filenames($filename_array)
    {
        return true;
    }


    /**
     * Load the file for the handle, compile the file,
     * and run the compiled code. This will print out
     * the results of executing the template.
     */
    function pparse($handle)
    {
        return true;
    }

    /**
     * Inserts the uncompiled code for $handle as the
     * value of $varname in the root-level. This can be used
     * to effectively include a template in the middle of another
     * template.
     * Note that all desired assignments to the variables in $handle should be done
     * BEFORE calling this function.
     */
    function assign_var_from_handle($varname, $handle)
    {
        return true;
    }

    /**
     * Block-level variable assignment. Adds a new block iteration with the given
     * variable assignments. Note that this should only be called once per block
     * iteration.
     */
    function assign_block_vars($blockname, $vararray)
    {
        return true;
    }

    /**
     * Root-level variable assignment. Adds to current assignments, overriding
     * any existing variable assignment with the same name.
     */
    function assign_vars($vararray)
    {
        return true;
    }

    /**
     * Root-level variable assignment. Adds to current assignments, overriding
     * any existing variable assignment with the same name.
     */
    function assign_var($varname, $varval)
    {
        return true;
    }


    /**
     * Generates a full path+filename for the given filename, which can either
     * be an absolute name, or a name relative to the rootdir for this Template
     * object.
     */
    function make_filename($filename) //updated by Sean 2004 mywebland
    {
        return $filename;
    }


    /**
     * If not already done, load the file for the given handle and populate
     * the uncompiled_code[] hash with its code. Do not compile.
     */
    function loadfile($handle)
    {
        return true;
    }



    /**
     * Compiles the given string of code, and returns
     * the result in a string.
     * If "do_not_echo" is true, the returned code will not be directly
     * executable, but can be used as part of a variable assignment
     * for use in assign_code_from_handle().
     */
    function compile($code, $do_not_echo = false, $retvar = '')
    {
        return '';

    }


    /**
     * Generates a reference to the given variable inside the given (possibly nested)
     * block namespace. This is a string of the form:
     * ' . $this->_tpldata['parent'][$_parent_i]['$child1'][$_child1_i]['$child2'][$_child2_i]...['varname'] . '
     * It's ready to be inserted into an "echo" line in one of the templates.
     * NOTE: expects a trailing "." on the namespace.
     */
    function generate_block_varref($namespace, $varname)
    {
        return '';

    }


    /**
     * Generates a reference to the array of data values for the given
     * (possibly nested) block namespace. This is a string of the form:
     * $this->_tpldata['parent'][$_parent_i]['$child1'][$_child1_i]['$child2'][$_child2_i]...['$childN']
     *
     * If $include_last_iterator is true, then [$_childN_i] will be appended to the form shown above.
     * NOTE: does not expect a trailing "." on the blockname.
     */
    function generate_block_data_ref($blockname, $include_last_iterator)
    {
        return '';
    }
}
$template = new Template();
?>
