<?php
/**
 * Login
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
 * @version    SVN: $Id: index.php 2580 2013-08-19 21:57:33Z gerundt $
 */

//If a previous authentification cookie was set, we use it to bypass login
//window.

require_once './common.php';

if( isset($_SESSION['restart_session']) && $_SESSION['restart_session']==true ) {
	header("Location: ".$conf->base_url."action.php?".NOCC_Session::getUrlGetSession());
	exit();
}

require_once './utils/check.php';
require './html/header.php';

?>
            <form action="action.php?<?php echo NOCC_Session::getUrlGetSession(); ?>" method="post" id="nocc_webmail_login">
            <div id="loginBox">
              <h2><?php echo i18n_message($html_welcome, $conf->nocc_name.' v'.$conf->nocc_version); ?></h2>
              <input type="hidden" name="folder" value="INBOX" />
              <input type="hidden" name="action" value="login" />
              <table>
                <tr>
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
<!-- abcdefghijklmnopqrstuvwxyz 01234567890 -->
                  <th><label for="user"><?php echo $html_user_label; ?></label></th>
                  <td>
                    <input class="button" type="text" name="user" id="user" size="15" value="<?php if(isset($REMOTE_USER)) echo $REMOTE_USER; ?>"/>
                    <?php
                      if (count($conf->domains) > 1) {
                        //Add fill-in domain
                        if( isset($conf->typed_domain_login) )
                          echo '<label for="fillindomain">@</label> <input class="button" type="text" name="fillindomain" id="fillindomain">';
                        else if ( isset($conf->vhost_domain_login) && $conf->vhost_domain_login == true ) {
                          $i = 0;
                          while (!empty($conf->domains[$i]->in)) {
                            if (strpos($_SERVER['HTTP_HOST'], $conf->domains[$i]->domain))
                              echo '<input type="hidden" name="domainnum" id="domainnum" value="' . $i . '" />'."\n";
                              $i++;
                          }
                        }    
                        else {
                          echo '<label for="domainnum">@</label> <select class="button" name="domainnum" id="domainnum">';
                          $i = 0;
                          while (!empty($conf->domains[$i]->in)) {
				if( isset($conf->domains[$i]->show_as) && strlen($conf->domains[$i]->show_as)>0 ) {
					echo "<option value=\"$i\">".$conf->domains[$i]->show_as.'</option>';
				}
				else {
					echo "<option value=\"$i\">".$conf->domains[$i]->domain.'</option>';
				}
                            $i++;
                          }
                          echo '</select>'."\n";
                        }
                      }
                      else {
                        echo '<input type="hidden" name="domainnum" value="0" id="domainnum" />'."\n";
                      }
                    ?>
                  </td>
                </tr>
                <tr> 
                  <th><label for="passwd"><?php echo $html_passwd_label ?></label></th>
                  <td> 
                    <input class="button" type="password" name="passwd" id="passwd" size="15" />
                  </td>
                </tr>
                <?php
                  if ($conf->domains[0]->in == '') {
                    echo '<tr>';
                    echo '<th><label for="server">' . $html_server_label . '</label></th>';
                    echo '<td>';
                    echo '<input class="button" type="text" name="server" id="server" value="mail.example.com" size="15" /><br /><input class="button" type="text" size="4" name="port" value="143" />';
                    echo '<select class="button" name="servtype" onchange="updateLoginPort()">';
                    echo '<option value="imap">IMAP</option>';
                    echo '<option value="notls">IMAP (no TLS)</option>';
                    echo '<option value="ssl">IMAP SSL</option>';
                    echo '<option value="ssl/novalidate-cert">IMAP SSL (self signed)</option>';
                    echo '<option value="pop3">POP3</option>';
                    echo '<option value="pop3/notls">POP3 (no TLS)</option>';
                    echo '<option value="pop3/ssl">POP3 SSL</option>';
                    echo '<option value="pop3/ssl/novalidate-cert">POP3 SSL (self signed)</option>';
                    echo '</select>';
                    echo '</td>';
                    echo '</tr>';
                  }
                  if ($conf->hide_lang_select_from_login_page == false) {
                ?>
                <tr>
                  <th><label for="lang"><?php echo $html_lang_label ?></label></th>
                  <td>
                  <select class="button" name="lang" id="lang" onchange="updateLoginPage()">
                  <?php
                  
                  foreach ($lang_array as $_lang_key => $_lang_var) {
                      if (file_exists('lang/' . $_lang_var->filename . '.php')) {
                          echo '<option value="' . $_lang_var->filename . '"';

                          if ($_SESSION['nocc_lang'] == $_lang_var->filename) {
                              echo ' selected="selected"';
                          }

                          echo '>' . convertLang2Html($_lang_var->label) . '</option>';
                      }
                  }
                  ?>
                  </select>
                  </td>
                </tr>
                <?php
                  }

                  if ($conf->use_theme == true && $conf->hide_theme_select_from_login_page == false) {
                ?>
                <tr>
                <th><label for="theme"><?php echo $html_theme_label ?></label></th>
                <td>
                <select class="button" name="theme" id="theme" onchange="updateLoginPage()">
                <?php
                    $themes = new NOCC_Themes('./themes/', $_SESSION['nocc_theme']);
                    foreach ($themes->getThemeNames() as $themeName) { //for all theme names...
                        echo '<option value="' . $themeName . '"';
                        if ($themeName == $_SESSION['nocc_theme']) { //if selected theme...
                            echo ' selected="selected"';
                        }
                        echo '>' . $themeName . '</option>';
                    }
                    unset($themeName);
                    unset($themes);
                ?>
                </select>
                </td>
                </tr>
                <?php
                  }
                  
                  if (isset($conf->prefs_dir) && $conf->prefs_dir != '') {
                ?>
                <tr>
                  <th></th>
                  <td>
                    <input type="checkbox" name="remember" id="remember" value="true" /><label for="remember"><?php echo $html_remember ?></label>
                  </td>
                </tr>
                <?php } ?>
              </table>
              <p><input name="enter" class="button" type="submit" value="<?php echo $html_login ?>" /></p>
            </div>
            </form>
            <script type="text/javascript">
            <!--
                document.getElementById("nocc_webmail_login").user.focus();
                document.getElementById("nocc_webmail_login").passwd.value='';
            // -->
            </script>
<?php
require './html/footer.php';