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
 * @version    SVN: $Id: th.php 2396 2011-02-14 16:47:29Z siebrand $
 */
/** Thai (ไทย)
 * 
 * See the qqq 'language' for message documentation incl. usage of parameters
 * To improve a translation please visit http://translatewiki.net
 *
 * @ingroup Language
 * @file
 *
 * @author Horus
 * @author Octahedron80
 * @author Passawuth
 * @author Robert Niska <r.niska@redbox.d2g.nu>
 * @author Woraponboonkerd
 */

$lang_locale = 'th_TH.UTF-8';
$default_date_format = '%d-%m-%Y';
$no_locale_date_format = '%d-%m-%Y';
$default_time_format = '%I:%M %p';
$err_user_empty = 'ชื่อผู้ใช้ไม่ได้กรอก';
$err_passwd_empty = 'รหัสผ่านไม่ได้กรอก';
$alt_delete = 'ลบจดหมายที่เลือก';
$alt_delete_one = 'ลบจดหมาย';
$alt_new_msg = 'จดหมายใหม่';
$alt_reply = 'ตอบกลับไปยังผู้ส่ง';
$alt_reply_all = 'ตอบกลับทั้งหมด';
$alt_forward = 'ส่งต่อ';
$alt_next = 'ถัดไป';
$alt_prev = 'ก่อนหน้า';
$title_next_page = 'หน้าถัดไป';
$title_prev_page = 'หน้าก่อนหน้า';
$title_next_msg = 'จดหมายถัดไป';
$title_prev_msg = 'จดหมายก่อนหน้า';
$html_theme_label = 'รูปแบบ:';
$html_welcome = 'ยินดีต้อนรับสู่ %1$s';
$html_login = 'ลงชื่อเข้าใช้';
$html_user_label = 'ชื่อผู้ใช้:';
$html_passwd_label = 'รหัสผ่าน:';
$html_submit = 'ส่งข้อมูล';
$html_help = 'วิธีใช้';
$html_server_label = 'เซิร์ฟเวอร์';
$html_wrong = 'ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง';
$html_retry = 'ลองใหม่';
$html_remember = 'จดจำการตั้งค่า';
$html_lang_label = 'ภาษา:';
$html_msgperpage_label = 'จำนวนจดหมายต่อหน้า:';
$html_preferences = 'การตั้งค่า';
$html_full_name_label = 'ชื่อเต็ม:';
$html_email_address_label = 'ที่อยู่อีเมล:';
$html_bccself = 'สำเนาลับถึงตนเอง';
$html_hide_addresses = 'ซ่อนที่อยู่';
$html_outlook_quoting = 'รูปแบบ Outlook';
$html_reply_to = 'ตอบกลับถึง';
$html_use_signature = 'ใช้ลายเซ็น';
$html_signature = 'ลายเซ็น';
$html_signature_label = 'ลายเซ็น:';
$html_reply_leadin_label = 'ตอบกลับ Leadin:';
$html_prefs_updated = 'การตั้งค่าได้ปรับปรุงแล้ว';
$html_manage_folders_link = 'จัดการโฟลเดอร์ IMAP';
$html_manage_filters_link = 'จัดการตัวกรองอีเมล์';
$html_use_graphical_smilies = 'ใช้หน้ายิ้มแบบกราฟิก';
$html_sent_folder_label = 'คัดลอกอีเมลที่ส่งแล้วไปยังโฟลเดอร์ที่จัดไว้:';
$html_trash_folder_label = 'ย้ายอีเมลที่ลบแล้วไปยังโฟลเดอร์ที่จัดไว้:';
$html_colored_quotes = 'ใส่สีข้อความอ้างอิง';
$html_display_struct = 'แสดงข้อความที่เป็นโครงสร้าง';
$html_send_html_mail = 'ส่งจดหมายในรูปแบบ HTML';
$html_folders = 'โฟลเดอร์';
$html_folders_create_failed = 'ไม่สามารถสร้างโฟลเดอร์!';
$html_folders_sub_failed = 'ไม่สามารถสมัครสมาชิกไปยังโฟลเดอร์!';
$html_folders_unsub_failed = 'ไม่สามารถยกเลิกสมาชิกจากโฟลเดอร์!';
$html_folders_rename_failed = 'ไม่สามารถเปลี่ยนชื่อโฟลเดอร์!';
$html_folders_updated = 'โฟลเดอร์ปรับปรุงแล้ว';
$html_folder_subscribe = 'สมัครสมาชิกไปยัง';
$html_folder_rename = 'เปลี่ยนชื่อ';
$html_folder_create = 'สร้างโฟลเดอร์ใหม่ชื่อ';
$html_folder_remove = 'ยกเลิกสมาชิกจาก';
$html_folder_delete = 'ลบ';
$html_folder_to = 'ถึง';
$html_filter_remove = 'ลบ';
$html_filter_body = 'เนื้อความจดหมาย';
$html_filter_subject = 'หัวเรื่องจดหมาย';
$html_filter_to = 'ช่องถึง';
$html_filter_cc = 'ช่องสำเนา';
$html_filter_from = 'ช่องจาก';
$html_filter_change_tip = 'เพื่อที่จะเปลี่ยนตัวกรอง เพียงเขียนทับลงไป';
$html_reapply_filters = 'ใช้ตัวกรองทั้งหมดอีกครั้ง';
$html_filter_contains = 'มี';
$html_filter_name = 'ชื่อตัวกรอง';
$html_filter_action = 'การดำเนินการกรอง';
$html_filter_moveto = 'ย้ายไปยัง';
$html_select_one = '--เลือกอย่างหนึ่ง--';
$html_and = 'และ';
$html_new_msg_in = 'มีจดหมายใหม่ใน';
$html_or = 'หรือ';
$html_move = 'ย้าย';
$html_copy = 'คัดลอก';
$html_messages_to = 'เลือกจดหมายไปยัง';
$html_gotopage = 'ไปที่หน้า';
$html_gotofolder = 'ไปที่โฟลเดอร์';
$html_other_folders = 'รายชื่อโฟลเดอร์';
$html_page = 'หน้า';
$html_of = 'ของ';
$html_view_header = 'แสดงรายละเอียด';
$html_remove_header = 'ซ่อนรายละเอียด';
$html_inbox = 'กล่องจดหมาย';
$html_new_msg = 'เขียนจดหมาย';
$html_reply = 'ตอบกลับ';
$html_reply_short = 'Re:';
$html_reply_all = 'ตอบกลับทุกคน';
$html_forward = 'ส่งต่อ';
$html_forward_short = 'Fwd:';
$html_forward_info = 'จดหมายส่งต่อจะถูกส่งเป็นไฟล์แนบในจดหมายนี้';
$html_delete = 'ลบ';
$html_new = 'ใหม่';
$html_mark = 'ลบ';
$html_att_label = 'ไฟล์แนบ:';
$html_atts_label = 'ไฟล์แนบ:';
$html_unknown = '[ไม่รู้จัก]';
$html_part_x = 'ส่วน %s';
$html_attach = 'แนบ';
$html_attach_forget = 'คุณต้องแนบไฟล์ก่อนที่จะส่งจดหมาย !';
$html_attach_delete = 'ลบไฟล์แนบที่เลือก';
$html_attach_none = 'คุณต้องเลือกไฟล์เพื่อที่จะแนบ !';
$html_sort_by = 'จัดเรียงโดย';
$html_sort = 'เรียงลำดับ';
$html_from = 'จาก';
$html_from_label = 'จาก:';
$html_subject = 'หัวเรื่อง';
$html_subject_label = 'เรื่อง:';
$html_date = 'วัน';
$html_date_label = 'วันที่:';
$html_sent_label = 'ส่ง:';
$html_wrote = 'เขียนเมื่อ';
$html_size = 'ขนาด';
$html_totalsize = 'ขนาดทั้งหมด';
$html_kb = 'kB';
$html_mb = 'MB';
$html_gb = 'GB';
$html_bytes = 'ไบต์';
$html_filename = 'ชื่อไฟล์';
$html_to = 'ถึง';
$html_to_label = 'ถึง:';
$html_cc = 'สำเนา';
$html_cc_label = 'สำเนา:';
$html_bcc_label = 'สำเนาซ่อน:';
$html_nosubject = 'ไม่มีหัวเรื่อง';
$html_send = 'ส่ง';
$html_cancel = 'ยกเลิก';
$html_no_mail = 'ไม่มีข้อความ';
$html_logout = 'ล็อกเอาต์';
$html_msg = 'ข้อความ';
$html_msgs = 'หลายข้อความ';
$html_configuration = 'เซิรฟเวอร์นี้ยังตั้งค่าไม่สมบูรณ์ !';
$html_priority = 'ความสำคัญ:';
$html_priority_label = 'ความสำคัญ:';
$html_lowest = 'ต่ำสุด';
$html_low = 'ต่ำ';
$html_normal = 'ปกติ';
$html_high = 'สูง';
$html_highest = 'สูงสุด';
$html_flagged = 'ติดธง';
$html_spam = 'อีเมลขยะ';
$html_spam_warning = 'เราเข้าใจว่าจดหมายนี้เป็นอีเมลขยะ';
$html_receipt = 'ร้องขอการรับส่งคืน';
$html_select = 'เลือก';
$html_select_all = 'กลับการเลือก';
$html_select_contacts = 'เลือกรายชื่อผู้ติดต่อ';
$html_loading_image = 'กำลังโหลดภาพ';
$html_send_confirmed = 'จดหมายท่านได้ถูกส่งไปแล้ว';
$html_no_sendaction = 'ไม่มีการกระทำใดๆ ให้ลองเปิดการใช้ JavaScript';
$html_error_occurred = 'เกิดข้อผิดพลาดขึ้น';
$html_prefs_file_error = 'ไม่สามารถตั้งค่าได้';
$html_wrap = 'จำนวนของอักขระเพื่อตัดคำในข้อความที่ส่งออก:';
$html_wrap_none = 'ไม่มีการตัดคำ';
$html_usenet_separator = 'ตัวแบ่ง Usenet ("-- \n") ก่อนลายเซ็น';
$html_mark_as = 'ทำเครื่องหมายเป็น';
$html_read = 'อ่านแล้ว';
$html_unread = 'ยังไม่ได้อ่าน';
$html_encoding_label = 'รหัสอักขระ:';
$html_add = 'เพิ่ม';
$html_contacts = 'ผู้ติดต่อ %1$s รายการ';
$html_modify = 'แก้ไข';
$html_back = 'ย้อนกลับ';
$html_contact_add = 'เพิ่มผู้ติดต่อใหม่';
$html_contact_mod = 'แก้ไขผู้ติดต่อ';
$html_contact_first = 'ชื่อจริง';
$html_contact_last = 'นามสกุล';
$html_contact_nick = 'ชื่อเล่น';
$html_contact_mail = 'ที่อยู่อีเมล';
$html_contact_list = 'รายชื่อผู้ติดต่อของ %1$s';
$html_contact_del = 'จากรายชื่อผู้ติดต่อ';
$html_contact_count = 'ผู้ติดต่อ %1$s รายการ';
$html_contact_err1 = 'จำนวนผู้ติดต่อมากที่สุดคือ "%1$d"';
$html_contact_err2 = 'คุณไม่สามารถเพิ่มผู้ติดต่อใหม่';
$html_contact_err3 = 'คุณไม่มีสิทธิ์เข้าถึงรายชื่อผู้ติดต่อ';
$html_contact_none = 'ไม่พบรายชื่อผู้ติดต่อ';
$html_del_msg = 'ลบจดหมายที่เลือกหรือไม่ ?';
$html_down_mail = 'ดาวน์โหลด';
$original_msg = '-- ข้อความต้นฉบับ --';
$to_empty = 'ช่อง \'ถึง\' ห้ามเว้นว่างไว้ !';
$html_images_warning = 'เพื่อความปลอดภัยของคุณ รูปภาพจากระยะไกลจะไม่ถูกแสดง';
$html_images_display = 'แสดงรูปภาพ';
$html_smtp_error_no_conn = 'ไม่สามารถติดต่อได้';
$html_smtp_error_unexpected = 'ไม่มีการตอบสนอง:';
$lang_could_not_connect = 'ไม่สามารถเชื่อมต่อเซิร์ฟเวอร์';
$lang_invalid_msg_num = 'หมายเลขข้อความไม่ถูกต้อง';
$html_file_upload_attack = 'การโจมตีการอัปโหลดไฟล์ที่อาจเกิดขึ้น';
$html_invalid_email_address = 'ที่อยู่อีเมลไม่ถูกต้อง';
$html_invalid_msg_per_page = 'จำนวนข้อความต่อหนึ่งหน้าไม่ถูกต้อง';
$html_invalid_wrap_msg = 'ความกว้างของการตัดคำในข้อความไม่ถูกต้อง';
$html_seperate_msg_win = 'จดหมายในหน้าต่างแยก';
$html_err_file_contacts = 'ไม่สามารถเปิดไฟล์รายชื่อผู้ติดต่อได้';
$html_session_file_error = 'ไม่สามารถเปิดไฟล์เซสชันสำหรับการเขียน';
$html_login_not_allowed = 'ชื่อเข้าใช้นี้ไม่ได้รับอนุญาตให้เชื่อมต่อได้';
$lang_err_send_delay = 'คุณต้องรอสักครู่หลังจากส่งอีเมลก่อนหน้า (%1$d วินาที)';
$html_search = 'สืบค้น';
