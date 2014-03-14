<!-- start of $Id: footer.php 2255 2010-06-28 07:46:41Z gerundt $ -->
<?php
  if (!isset($conf->loaded))
    die('Hacking attempt');

if (!isset($theme)) //if the $theme variable NOT set...
    $theme = new NOCC_Theme($_SESSION['nocc_theme']);

$custom_footer = $theme->getCustomFooter();
if (file_exists($custom_footer)) {
    include $custom_footer;
}
else {
?>
    </div>
        <div id="footer">
            <a href="http://nocc.sourceforge.net" target="_blank">
                <img src="<?php echo $theme->getPath(); ?>/img/button.png" id="footerLogo" alt="Powered by NOCC" />
            </a>
        </div>
<?php
if (NOCC_DEBUG_LEVEL > 0) {
    define('NOCC_END_TIME', microtime(true));

    $time = NOCC_END_TIME - NOCC_START_TIME;
    $usage = memory_get_usage() / 1024;
    $peakUsage = memory_get_peak_usage() / 1024;

    printf('<p class="debug">Time: <strong>%.2f sec</strong> - Memory Usage: <strong>%.2f KB</strong> - Memory Peak Usage: <strong>%.2f KB</strong></p>', $time, $usage, $peakUsage);
}
?>
    </body>
</html>
<?php
}
?>
<!-- end of $Id: footer.php 2255 2010-06-28 07:46:41Z gerundt $ -->
