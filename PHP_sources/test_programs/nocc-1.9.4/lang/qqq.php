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
 * @version    SVN: $Id: qqq.php 2525 2011-12-19 00:37:22Z siebrand $
 */
/** Message documentation (Message documentation)
 * 
 * See the qqq 'language' for message documentation incl. usage of parameters
 * To improve a translation please visit http://translatewiki.net
 *
 * @ingroup Language
 * @file
 *
 * @author EugeneZelenko
 * @author Fryed-peach
 * @author Lloffiwr
 * @author McDutchie
 * @author Naudefj
 * @author Purodha
 * @author Siebrand
 */

$lang_locale = 'What language to use (Here, english US --> en_US). See [http://unicode.org/cldr/apps/survey CLDR] for more information';
$lang_dir = '"ltr" if your language uses left-to-right script, "rtl" if right-to-left. Default is ltr.';
$default_date_format = 'What format string should be passed to strftime() for messages sent on days other than today?';
$no_locale_date_format = 'If the locale is not implemented on the host, how we display the date. %-signs, and the letters following them, indicate parts of a date. So do not translate them, but adjust the whole date pattern to the format used in your language\'s environment. See [http://www.php.net/manual/en/function.strftime.php PHP manual]

* %Y = Four digit representation for the year
* %m = Two digit representation of the month
* %d = Two-digit day of the month (with leading zeros)';
$default_time_format = 'What format string should be passed to [http://www.php.net/manual/en/function.strftime.php PHP strftime()] for messages sent today? %-signs, and the letters following them, indicate parts of a time. So do not translate them, but adjust the entire pattern to the format used in your language\'s environment. 

* %I = Two digit representation of the hour in 12-hour format
* %M = Two digit representation of the minute
* %p = UPPER-CASE \'AM\' or \'PM\' based on the given time
* %H = Two digit representation of the hour in 24-hour format';
$alt_new_msg = '{{Identical|New messages}}';
$alt_forward = '{{Identical|Forward}}';
$alt_next = '{{Identical|Next}}

[[File:Nocc_paging.jpg‎|thumb|250px|right]]
Navigation link at top of page. See screenshot.';
$alt_prev = 'Navigation link at top of page. See [[Translating:NOCC#NOCC_screenshots|screenshot]].



{{Identical|Previous}}';
$title_next_page = '{{Identical|Next page}}';
$title_prev_page = '{{Identical|Previous page}}';
$html_theme_label = '{{Identical|Theme}}';
$html_login = 'Verb.{{Identical|Login}}';
$html_user_label = '{{Identical|User}}';
$html_passwd_label = '{{Identical|Password}}';
$html_submit = 'Submit button label used on the login page and the preferences page.';
$html_help = '{{Identical|Help}}';
$html_server_label = '{{Identical|Server}}';
$html_lang_label = '{{Identical|Language}}';
$html_msgperpage_label = 'Text before an input box in the user\'s \'preferences\'. The user chooses the number of messages to display together on each page of a list of messages.';
$html_preferences = '{{Identical|Preferences}}';
$html_bccself = 'This means: \'\'send a copy of the e-mail to my own address but hide this fact from all other receivers of the e-mail.\'\'
"Bcc" stands for "blind carbon copy" and is here being used as a verb.';
$html_signature = '{{Identical|Signature}}';
$html_signature_label = '{{Identical|Signature}}';
$html_display_struct = 'This is a prompt of a yes/no switch. If set to true (yes), NOCC will render few simple formatting options commonly used in plaintext ASCII e-mail via html markup, such as:
<span style="white-space:nowrap">"<code>+-</code>" → "&plusmn;"</span>, and
<span style="white-space:nowrap">"<code>xyz^2</code>" → "xyz<sup>2</sup>"</span>, and
<span style="white-space:nowrap">"<code>*strong*</code>" → "<strong>strong</strong>"</span>, and
<span style="white-space:nowrap">"<code>/emphasised/</code>" → "<em>emphasised</em>"</span>, and
<span style="white-space:nowrap">"<code>_underlined_</code>" → "<u>underlined</u>"</span>, and
<span style="white-space:nowrap">"<code>|code|</code>" → "<code>code</code>"</span>.';
$html_folders = 'Title of screen and tab. See [[Translating:NOCC#NOCC_screenshots|screenshot]].';
$html_folder_subscribe = 'Part of the interface for managing folders, see the [[Translating:NOCC#NOCC_screenshots|screenshot]].';
$html_folder_rename = 'Part of the interface for managing folders, see the [[Translating:NOCC#NOCC_screenshots|screenshot]]. It appears in the option to rename a folder: \'Rename [input box to select folder to be renamed] to [input box to write new name]\'.

{{Identical|Rename}}';
$html_folder_create = 'Part of the interface for managing folders, see the [[Translating:NOCC#NOCC_screenshots|screenshot]].';
$html_folder_remove = 'Part of the interface for managing folders, see the [[Translating:NOCC#NOCC_screenshots|screenshot]].';
$html_folder_delete = 'Part of the interface for managing folders, see the [[Translating:NOCC#NOCC_screenshots|screenshot]].

{{Identical|Delete}}';
$html_folder_to = 'Part of the interface for managing folders, see the [[Translating:NOCC#NOCC_screenshots|screenshot]]. It appears in the option to rename a folder: \'Rename [input box to select folder to be renamed] to [input box to write new name]\'.';
$html_filter_remove = '[[File:Nocc_filter.png|150px|right]]
Text of radio button. See screenshot.



{{Identical|Delete}}';
$html_filter_body = '[[File:Nocc_filter.png|150px|right]]
Option in "Select one" drop-down box. See screenshot.';
$html_filter_subject = '[[File:Nocc_filter.png|150px|right]]
Option in "Select one" drop-down box. See screenshot.';
$html_filter_to = '[[File:Nocc_filter.png|150px|right]]
Option in "Select one" drop-down box. See screenshot.';
$html_filter_cc = '[[File:Nocc_filter.png|150px|right]]
Option in "Select one" drop-down box. See screenshot.';
$html_filter_from = '[[File:Nocc_filter.png|150px|right]]
Option in "Select one" drop-down box. See screenshot.';
$html_filter_contains = 'Used in the e-mail filter settings form. See [[:File:Nocc filter.png|screenshot]] for usage.';
$html_filter_name = '[[File:Nocc_filter.png|150px|right]]
Field label. See screenshot.';
$html_filter_action = '[[File:Nocc_filter.png|150px|right]]
Label in front of radio buttons. See screenshot.';
$html_filter_moveto = '[[File:Nocc_filter.png|150px|right]]
Text of radio button - before drop-down list. See screenshot.';
$html_select_one = '[[File:Nocc_filter.png|150px|right]]
In drop-down box. See screenshot.';
$html_and = '{{Identical|And}}

The word is used as \'And\' for three filter fields, each on a row of its own. See [[:File:Nocc filter.png|screenshot of the NOCC Filter preferences]].';
$html_or = '{{Identical|Or}}';
$html_move = '{{Identical|Move}}';
$html_copy = '{{Identical|Copy}}';
$html_page = '{{Identical|Page}}';
$html_of = 'Appears at the top of a mailbox, to navigate between the pages of a mailbox. \'of\' is part of \'Page x of y\', where n is a page number in an input box and y is the total number of pages in the mailbox. See the [[Translating:NOCC#NOCC_screenshots|screenshot]].';
$html_reply = '{{Identical|Reply}}';
$html_reply_short = 'Appears at the beginning of the subject box when replying to a message. It is short for reply. See the [[Translating:NOCC#NOCC_screenshots|screenshot]].
{{Identical|Re}}';
$html_forward = '{{Identical|Forward}}';
$html_delete = '{{Identical|Delete}}';
$html_new = '{{Identical|New}}';
$html_mark = '{{Identical|Delete}}';
$html_att_label = '{{Identical|Attachments}}';
$html_atts_label = '{{Identical|Attachments}}';
$html_unknown = '{{Identical|Unknown}}';
$html_part_x = 'This message has to do with e-mails with attachments that are split up because the attachment cannot be sent in one e-mail. %s is the part number.';
$html_attach = '{{Identical|Attach}}';
$html_attach_delete = 'This is a button label which appears under the list of attached files when writing an e-mail. When this button is clicked all selected file attachments are removed from the e-mail.';
$html_sort_by = '{{Identical|Sort by}}';
$html_from = '{{Identical|From}}';
$html_from_label = '{{Identical|From}}';
$html_subject = '{{Identical|Subject}}';
$html_date = '{{Identical|Date}}';
$html_date_label = '{{Identical|Date}}';
$html_wrote = 'The text \'{email sender} wrote\' appears on the reply, before the original message.';
$html_size = '{{Identical|Size}}';
$html_filename = 'Column header on the screen for writing an e-mail, in a table of files attached to the e-mail. See [http://nocc.sourceforge.net/screenshots/nocc15_write.png screenshot].

{{Identical|Filename}}';
$html_to_label = '{{Identical|To}}';
$html_send = '{{Identical|Send}}';
$html_cancel = '{{Identical|Cancel}}';
$html_logout = '{{Identical|Logout}}';
$html_priority = '{{Identical|Priority}}';
$html_priority_label = '{{Identical|Priority}}';
$html_lowest = 'Option in the drop-down box for selecting the priority for tagging an e-mail before sending it. The five options are lowest, low, normal, high and highest.';
$html_low = 'Option in the drop-down box for selecting the priority for tagging an e-mail before sending it. The five options are lowest, low, normal, high and highest.';
$html_normal = 'Option in the drop-down box for selecting the priority for tagging an e-mail before sending it. The five options are lowest, low, normal, high and highest.';
$html_high = 'Option in the drop-down box for selecting the priority for tagging an e-mail before sending it. The five options are lowest, low, normal, high and highest.';
$html_highest = 'Option in the drop-down box for selecting the priority for tagging an e-mail before sending it. The five options are lowest, low, normal, high and highest.';
$html_flagged = '"Flagged" in this context means that the e-mail has been marked (possibly for follow-up or something else)';
$html_spam = '{{Identical|Spam}}';
$html_select = '{{Identical|Select}}';
$html_add = '{{Identical|Add}}';
$html_contacts = '"%1$s" is replaced by a config string. "NOCC" is the default value.';
$html_modify = '{{Identical|Modify}}';
$html_back = '{{Identical|Back}}';
$html_contact_mod = 'This means the same as \'amend the details of a contact\'.';
$html_contact_list = '%1$s is the username';
$html_down_mail = '{{Identical|Download}}';
$html_session_file_error = 'Probably a system configuration error message. Probably refers to a PHP session which uses a file for each session. Therefore for languages which have to use an article, "a" or "the", before "file", "a" is probably best. See [[Thread:Translating talk:NOCC/Html session file error/reply (3)|discussion]].';
$html_search = '{{Identical|Search}}';
