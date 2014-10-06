<?php
// Security
$htmlsafe                     = "yes";               // disable & enable html posting
$commenthtmlsafe              = "yes";              // Disable all HTML Tags for comment section prevent exploit
$searchstriptagsenable        = "yes";              // Strip all HTML Tags before search to prevent exploit
$searchhtmlsafe               = "yes";              // Disable all HTML Tags before search to prevent exploit

$html_entities_match         = array('#&(?!(\#[0-9]+;))#', '#<#', '#>#');
$html_entities_replace       = array('&amp;', '&lt;', '&gt;');
?>
