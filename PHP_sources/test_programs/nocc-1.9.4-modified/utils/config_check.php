<?php

require_once './classes/nocc_mailaddress.php';

// This function allows you to customise the default e-mail address
function get_default_from_address() {
    if (!NOCC_Session::existsUserPrefs())
        return '';

    $user_prefs = NOCC_Session::getUserPrefs();

    $mailAddress = $user_prefs->getMailAddress();

    if (!$mailAddress->hasAddress()) {
        if (isset($_SESSION['nocc_login_mailaddress'])) {
            $mailAddress->setAddress($_SESSION['nocc_login_mailaddress']);
        }
    }
    
    return (string)$mailAddress;
}

// Detect base url
if (!isset($conf->base_url) || $conf->base_url == '') {
  $path_info = pathinfo($_SERVER['SCRIPT_NAME']);
  if (substr($path_info['dirname'], -1, 1) == '/')
    $dir_name = $path_info['dirname'];
  else
    $dir_name = $path_info['dirname'].'/';
  //Prevent a buggy behavior from PHP under Windows
  if ($path_info['dirname'] == '\\') $dir_name = '/';

  $conf->base_url = 'http';
  if (isset($_SERVER['HTTPS']) && $_SERVER['HTTPS'] == 'on')
    $conf->base_url .=  's';
  $conf->base_url .= '://'.$_SERVER['HTTP_HOST'].$dir_name;
}
