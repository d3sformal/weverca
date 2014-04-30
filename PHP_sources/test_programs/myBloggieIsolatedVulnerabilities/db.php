<?php

class sql_db
{

    //
    // Constructor
    //
    function sql_db($sqlserver, $sqluser, $sqlpassword, $database, $persistency = true)
    {
    }

    //
    // Other base methods
    //
    function sql_close()
    {
    }

    //
    // Base query method
    //
    function sql_query($query = "", $transaction = FALSE)
    {
    }

    //
    // Other query methods
    //
    function sql_numrows($query_id = 0)
    { return 1;
    }
    function sql_affectedrows()
    {
    }
    function sql_numfields($query_id = 0)
    {
    }
    function sql_fieldname($offset, $query_id = 0)
    {
    }
    function sql_fieldtype($offset, $query_id = 0)
    {
    }
    function sql_fetchrow($query_id = 0)
    {
	    return array();
    }
    function sql_fetchrowset($query_id = 0)
    {
    }
    function sql_fetchfield($field, $rownum = -1, $query_id = 0)
    {
    }
    function sql_rowseek($rownum, $query_id = 0){
    }
    function sql_nextid(){
    }
    function sql_freeresult($query_id = 0){
    }
    function sql_error($query_id = 0)
    {
	    return array('message' => 0);
    }

} // class sql_db

$db = new sql_db($dbhost, $dbuser, $dbpasswd, $dbname, false);
?>
