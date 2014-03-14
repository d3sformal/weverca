<?php
/**
 * Language configuration file for NOCC
 *
 * Copyright 2001 Nicolas Chalanset <nicocha@free.fr>
 * Copyright 2001 Olivier Cahagne <cahagn_o@epita.fr>
 *
 * This file is part of NOCC. NOCC is free software under the terms of the
 * GNU General Public License. You should have received a copy of the license
 * along with NOCC.  If not, see <http://www.gnu.org/licenses/>.
 *
 * @package    NOCC
 * @subpackage Translations
 * @license    http://www.gnu.org/licenses/ GNU General Public License
 * @version    SVN: $Id: sv.php 2427 2011-04-04 10:02:42Z siebrand $
 */
/** Swedish (Svenska)
 * 
 * See the qqq 'language' for message documentation incl. usage of parameters
 * To improve a translation please visit http://translatewiki.net
 *
 * @ingroup Language
 * @file
 *
 * @author Gabbe.g
 * @author Hno
 * @author Per
 * @author Poxnar
 * @author Robert Niska <r.niska@redbox.d2g.nu>
 * @author Twobee
 */

$lang_locale = 'sv_SE.UTF-8';
$lang_dir = 'ltr';
$default_date_format = '%A %d %B %Y';
$no_locale_date_format = '%d-%m-%Y';
$default_time_format = '%I:%M %p';
$err_user_empty = 'Login fältet är tomt';
$err_passwd_empty = 'Lösenords fältet är tomt';
$alt_delete = 'Ta bort valt meddelande';
$alt_delete_one = 'Ta bort meddelandet';
$alt_new_msg = 'Nya Meddelanden';
$alt_reply = 'Svara';
$alt_reply_all = 'Svara alla';
$alt_forward = 'Vidarebefodra';
$alt_next = 'Nästa';
$alt_prev = 'Föregående';
$title_next_page = 'Nästa sida';
$title_prev_page = 'Föregående sida';
$title_next_msg = 'Nästa';
$title_prev_msg = 'Föregående';
$html_theme_label = 'Tema:';
$html_welcome = 'Välkommen till %1$s';
$html_login = 'Logga in';
$html_user_label = 'Användare:';
$html_passwd_label = 'Lösenord:';
$html_submit = 'Logga in';
$html_help = 'Hjälp';
$html_server_label = 'Server:';
$html_wrong = 'Användarnamnet eller lösenordet är fel';
$html_retry = 'Försök igen';
$html_remember = 'Kom ihåg inställningar';
$html_lang_label = 'Språk:';
$html_msgperpage_label = 'Meddelanden per sida:';
$html_preferences = 'Inställningar';
$html_full_name_label = 'Fullt namn';
$html_email_address_label = 'E-postadress';
$html_bccself = 'Hemlig kopia till mig själv';
$html_hide_addresses = 'Göm adresser';
$html_outlook_quoting = 'Outlook-liknande citering';
$html_reply_to = 'Svara till';
$html_use_signature = 'Använd signatur';
$html_signature = 'Signatur';
$html_signature_label = 'Signatur:';
$html_reply_leadin_label = 'Svarsinledning:';
$html_prefs_updated = 'Inställningar uppdaterade';
$html_manage_folders_link = 'Hantera IMAP-mappar';
$html_manage_filters_link = 'Hantera e-postfilter';
$html_use_graphical_smilies = 'Använd grafiska smilies';
$html_sent_folder_label = 'Kopiera skickat e-post till en dedikerad mapp:';
$html_trash_folder_label = 'Flytta borttagna e-postmeddelanden till en särskild mapp:';
$html_colored_quotes = 'Färgade citat';
$html_display_struct = 'Visa strukturerad text';
$html_send_html_mail = 'Skicka e-post i HTML-format';
$html_folders = 'Mappar';
$html_folders_create_failed = 'Mapp kunde inte skapas!';
$html_folders_sub_failed = 'Kunde inte prenumerera på mapp!';
$html_folders_unsub_failed = 'Kunde inte avsluta prenumerationen på mapp!';
$html_folders_rename_failed = 'Mappen kunde inte döpas om!';
$html_folders_updated = 'Mappar uppdaterade';
$html_folder_subscribe = 'Prenumerera på';
$html_folder_rename = 'Döp om';
$html_folder_create = 'Skapa en ny mapp med namnet';
$html_folder_remove = 'Avsluta prenumeration på';
$html_folder_delete = 'Radera';
$html_folder_to = 'till';
$html_filter_remove = 'Radera';
$html_filter_body = 'Meddelandetext';
$html_filter_subject = 'Ämne';
$html_filter_to = 'Till fältet';
$html_filter_cc = 'Kopia fältet';
$html_filter_from = 'Från fältet';
$html_filter_change_tip = 'För att ändra ett filter är det bara att skriva över den.';
$html_reapply_filters = 'Tillämpa alla filter';
$html_filter_contains = 'innehåller';
$html_filter_name = 'Filternamn';
$html_filter_action = 'Filteråtgärd';
$html_filter_moveto = 'Flytta till';
$html_select_one = '--Välj en--';
$html_and = 'Och';
$html_new_msg_in = 'Nya meddelanden i';
$html_or = 'eller';
$html_move = 'Flytta';
$html_copy = 'Kopiera';
$html_messages_to = 'valda meddelanden till';
$html_gotopage = 'Gå till sida';
$html_gotofolder = 'Gå till mapp';
$html_other_folders = 'Mapplista';
$html_page = 'Sida';
$html_of = 'av';
$html_view_header = 'Visa huvud';
$html_remove_header = 'Dölj huvud';
$html_inbox = 'Inkorg';
$html_new_msg = 'Skriv';
$html_reply = 'Svara';
$html_reply_short = 'Sv:';
$html_reply_all = 'Svara Alla';
$html_forward = 'Vidarebefodra';
$html_forward_short = 'Vb:';
$html_forward_info = 'Det vidarebefordrade meddelandet kommer att skickas som en bifogning till den här.';
$html_delete = 'Ta bort';
$html_new = 'Nytt';
$html_mark = 'Ta bort';
$html_att_label = 'Bifogning:';
$html_atts_label = 'Bilagor:';
$html_unknown = '[okänd]';
$html_part_x = 'Del %s';
$html_attach = 'Bifoga';
$html_attach_forget = 'Du måste bifoga din fil innan du skickar meddelandet !';
$html_attach_delete = 'Ta bort vald';
$html_attach_none = 'Du måste välja en fil att bifoga!';
$html_sort_by = 'Sortera efter';
$html_sort = 'Sortera';
$html_from = 'Från';
$html_from_label = 'Från:';
$html_subject = 'Ämne';
$html_subject_label = 'Ämne:';
$html_date = 'Datum';
$html_date_label = 'Datum:';
$html_sent_label = 'Skickat:';
$html_wrote = 'skrev';
$html_size = 'Storlek';
$html_totalsize = 'Total Storlek';
$html_kb = 'kB';
$html_mb = 'MB';
$html_gb = 'GB';
$html_bytes = 'byte';
$html_filename = 'Filnamn';
$html_to = 'Till';
$html_to_label = 'Till:';
$html_cc = 'Kopia';
$html_cc_label = 'Kopia:';
$html_bcc_label = 'Blind kopia:';
$html_nosubject = 'Inget ämne';
$html_send = 'Skicka';
$html_cancel = 'Avbryt';
$html_no_mail = 'Inget meddelande.';
$html_logout = 'Logga ut';
$html_msg = 'Meddelande';
$html_msgs = 'Meddelanden';
$html_configuration = 'Denna server är inte särskilt väl uppsatt!';
$html_priority = 'Prioritet';
$html_priority_label = 'Prioritet:';
$html_lowest = 'Lägsta';
$html_low = 'Låg';
$html_normal = 'Normal';
$html_high = 'Hög';
$html_highest = 'Högsta';
$html_flagged = 'Märkta';
$html_spam = 'Skräppost';
$html_spam_warning = 'Detta budskap har klassificerats som skräppost.';
$html_receipt = 'Begär ett returkvitto';
$html_select = 'Välj';
$html_select_all = 'Invertera urval';
$html_select_contacts = 'Välj kontakter';
$html_loading_image = 'Laddar bild';
$html_send_confirmed = 'Din e-post accepterades för leverans';
$html_no_sendaction = 'Ingen åtgärd specificerad. Försök att aktivera JavaScript.';
$html_error_occurred = 'Ett fel inträffade';
$html_prefs_file_error = 'Kan inte öppna inställningsfil för skrivning.';
$html_wrap = 'Antal tecken att radbryta utgående meddelanden till:';
$html_wrap_none = 'Ingen inpackning';
$html_usenet_separator = 'Usenet separator ("-- \n") före undertecknandet';
$html_mark_as = 'Markera som';
$html_read = 'läst';
$html_unread = 'oläst';
$html_encoding_label = 'Teckenkodning:';
$html_add = 'Lägg till';
$html_contacts = '%1$s Kontakter';
$html_modify = 'Modifiera';
$html_back = 'Bakåt';
$html_contact_add = 'Lägg till ny kontakt';
$html_contact_mod = 'Ändra en kontakt';
$html_contact_first = 'Förnamn';
$html_contact_last = 'Efternamn';
$html_contact_nick = 'Smeknamn';
$html_contact_mail = 'E-post';
$html_contact_list = 'Kontaktlista för %1$s';
$html_contact_del = 'från kontaktlistan';
$html_contact_count = '%1$d Kontakter';
$html_contact_err1 = 'Maximalt antal kontakter är "%1$d"';
$html_contact_err2 = 'Du kan inte lägga till en ny kontakt';
$html_contact_err3 = 'Du har inte behörighet till kontaktlista';
$html_contact_none = 'Inga kontakter kunde hittas.';
$html_del_msg = 'Radera valda meddelanden ?';
$html_down_mail = 'Ladda ner';
$original_msg = '-- Original Meddelande --';
$to_empty = 'Mottagar fältet kan inte vara tomt !';
$html_images_warning = 'För din säkerhet visas inte externa bilder.';
$html_images_display = 'Visa bilder';
$html_smtp_error_no_conn = 'Kan inte öppna SMTP-anslutning';
$html_smtp_error_unexpected = 'Oväntat SMTP svar:';
$lang_could_not_connect = 'Kunde inte ansluta till servern';
$lang_invalid_msg_num = 'Dåligt nummer på meddelandet';
$html_file_upload_attack = 'Möjlig filuppladdnings attack';
$html_invalid_email_address = 'Ogiltig e-postadress';
$html_invalid_msg_per_page = 'Ogiltigt antal meddelanden per sida';
$html_invalid_wrap_msg = 'Ogiltig bredd för meddelande radbrytning';
$html_seperate_msg_win = 'Meddelanden i separat fönster';
$html_err_file_contacts = 'Det går inte att öppna kontakter fil för skrivning.';
$html_session_file_error = 'Kan inte öppna session filen för skrivning.';
$html_login_not_allowed = 'Denna inloggning är inte tillåten för uppkoppling.';
$lang_err_send_delay = 'Du måste vänta mellan två e-brev (%1$d sekunder)';
$html_search = 'Sök';
