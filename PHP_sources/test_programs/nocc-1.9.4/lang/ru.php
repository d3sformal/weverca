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
 * @version    SVN: $Id: ru.php 2531 2012-03-01 15:52:20Z siebrand $
 */
/** Russian (Русский)
 * 
 * See the qqq 'language' for message documentation incl. usage of parameters
 * To improve a translation please visit http://translatewiki.net
 *
 * @ingroup Language
 * @file
 *
 * @author Anton Jakimov <t0xa@ls2.lv>
 * @author EugeneZelenko
 * @author Eugrus
 * @author Lockal
 * @author Lvova
 * @author Sergey Frolovithev <serg@spylog.ru>
 * @author Sk
 * @author Александр Сигачёв
 * @author გიორგიმელა
 */

$lang_locale = 'ru_RU.UTF-8';
$default_date_format = '%d.%m.%Y';
$no_locale_date_format = '%d.%m.%Y';
$default_time_format = '%H:%M';
$err_user_empty = 'Не введен логин';
$err_passwd_empty = 'Не введен пароль';
$alt_delete = 'Удалить выбранные сообщения';
$alt_delete_one = 'Удалить сообщение';
$alt_new_msg = 'Новые сообщения';
$alt_reply = 'Ответить автору';
$alt_reply_all = 'Ответить всем';
$alt_forward = 'Переслать';
$alt_next = 'Дальше';
$alt_prev = 'Назад';
$title_next_page = 'Следующая страница';
$title_prev_page = 'Предыдущая страница';
$title_next_msg = 'Следующее';
$title_prev_msg = 'Предыдущее';
$html_theme_label = 'Оформление:';
$html_welcome = 'Добро пожаловать в %1$s';
$html_login = 'Войти';
$html_user_label = 'Пользователь:';
$html_passwd_label = 'Пароль:';
$html_submit = 'Войти';
$html_help = 'Помощь';
$html_server_label = 'Сервер:';
$html_wrong = 'Логин или пароль не верны';
$html_retry = 'Повторить';
$html_remember = 'Запомнить настройки';
$html_lang_label = 'Язык:';
$html_msgperpage_label = 'Писем на странице:';
$html_preferences = 'Настройки';
$html_full_name_label = 'Полное имя:';
$html_email_address_label = 'Адрес электронной почты:';
$html_bccself = 'Копию себе';
$html_hide_addresses = 'Спрятать адрес';
$html_outlook_quoting = 'Цитирование в стиле Outlook';
$html_reply_to = 'Ответить';
$html_reply_to_label = 'Ответить на:';
$html_use_signature = 'Использовать подпись';
$html_signature = 'Подпись';
$html_signature_label = 'Подпись:';
$html_reply_leadin_label = 'Вводный блок ответа:';
$html_prefs_updated = 'Настройки сохранены';
$html_manage_folders_link = 'Управление папками IMAP';
$html_manage_filters_link = 'Управление почтовыми фильтрами';
$html_use_graphical_smilies = 'Использовать графические смайлики';
$html_sent_folder_label = 'Копировать отправленные письма в специальную папку:';
$html_trash_folder_label = 'Перемещать удалённые письма в специальную папку:';
$html_colored_quotes = 'Цветные цитаты';
$html_display_struct = 'Отобразить текст структуры';
$html_send_html_mail = 'Отправить письма в HTML';
$html_folders = 'Папки';
$html_folders_create_failed = 'Папка не может быть создана!';
$html_folders_sub_failed = 'Невозможно подписаться на папку!';
$html_folders_unsub_failed = 'Невозможно отписаться от папки!';
$html_folders_rename_failed = 'Папка не может быть переименована!';
$html_folders_updated = 'Папки сохранены';
$html_folder_subscribe = 'Подписаться на';
$html_folder_rename = 'Переименовать';
$html_folder_create = 'Создать новую папку с именем';
$html_folder_remove = 'Отписаться от';
$html_folder_delete = 'Удалить';
$html_folder_to = 'до';
$html_filter_remove = 'Удалить';
$html_filter_body = 'Тело письма';
$html_filter_subject = 'Тема';
$html_filter_to = 'Поле Кому';
$html_filter_cc = 'Поле Копия';
$html_filter_from = 'Поле От';
$html_filter_change_tip = 'Для замены, просто перезапешите фильтр.';
$html_reapply_filters = 'Переприменить все фильтры';
$html_filter_contains = 'содержит';
$html_filter_name = 'Название фильтра';
$html_filter_action = 'Действие фильтра';
$html_filter_moveto = 'Переместить в';
$html_select_one = '--выберите одно--';
$html_and = 'и';
$html_new_msg_in = 'Новые сообщения в';
$html_or = 'или';
$html_move = 'Переместить';
$html_copy = 'Копировать';
$html_messages_to = 'выбранные письма в';
$html_gotopage = 'Перейти к странице';
$html_gotofolder = 'Перейти в папку';
$html_other_folders = 'Список папок';
$html_page = 'Страница';
$html_of = 'из';
$html_view_header = 'Просмотреть заголовок письма';
$html_remove_header = 'Убрать заголовок письма';
$html_inbox = 'Входящие';
$html_new_msg = 'Написать';
$html_reply = 'Ответить';
$html_reply_short = 'Кому';
$html_reply_all = 'Ответить всем';
$html_forward = 'Переслать';
$html_forward_short = 'Отец';
$html_forward_info = 'Пересылаемое сообщение будет отправлено как приложение к этому письму.';
$html_delete = 'Удалить';
$html_new = 'Новое';
$html_mark = 'Удалить';
$html_att_label = 'Прикрепленный файл:';
$html_atts_label = 'Прикрепленные файлы:';
$html_unknown = '[неизвестно]';
$html_part_x = 'Часть %s';
$html_attach = 'Прикрепить файл';
$html_attach_forget = 'Вы должны прикрепить файл до отправки сообщения!';
$html_attach_delete = 'Удалить выбранные';
$html_attach_none = 'Вы можете выбрать файл для присоединения!';
$html_sort_by = 'Сортировать по';
$html_sort = 'Сортировать';
$html_from = 'От';
$html_from_label = 'От:';
$html_subject = 'Тема';
$html_subject_label = 'Тема:';
$html_date = 'Время';
$html_date_label = 'Дата:';
$html_sent_label = 'Отправлено:';
$html_wrote = 'Написал(а)';
$html_size = 'Размер';
$html_totalsize = 'Общий размер';
$html_kb = 'КБ';
$html_mb = 'МБ';
$html_gb = 'ГБ';
$html_bytes = 'Б';
$html_filename = 'Имя файла';
$html_to = 'Кому';
$html_to_label = 'Кому:';
$html_cc = 'Копия';
$html_cc_label = 'Копия:';
$html_bcc_label = 'Rss';
$html_nosubject = 'Нет темы';
$html_send = 'Отправить';
$html_cancel = 'Отменить';
$html_no_mail = 'Сообщений нет';
$html_logout = 'Выйти';
$html_msg = 'Сообщение';
$html_msgs = 'Сообщений';
$html_configuration = 'Этот сервер некоррекно установлен!';
$html_priority = 'Приоритет';
$html_priority_label = 'Приоритет:';
$html_lowest = 'Дальше';
$html_low = 'Низкий';
$html_normal = 'Нормальный';
$html_high = 'Высокий';
$html_highest = 'Выше';
$html_flagged = 'Отмеченные';
$html_spam = 'Спам';
$html_spam_warning = 'Это сообщение было классифицировано как спам.';
$html_receipt = 'Запросить уведомление о прочтении';
$html_select = 'Выбрать';
$html_select_all = 'Инвертировать выбор';
$html_select_contacts = 'Выбор контактов';
$html_loading_image = 'Загрузка изображения';
$html_send_confirmed = 'Ваша почта принята к отправлению';
$html_no_sendaction = 'Не указано действие. Попробуйте включить JavaScript.';
$html_error_occurred = 'Произошла ошибка';
$html_prefs_file_error = 'Файл настроек не может быть открыт для записи.';
$html_wrap = 'Число символов вызывающих перенос на новую строку в исходящих письмах:';
$html_wrap_none = 'Нет';
$html_usenet_separator = 'Разделитель в стиле Usenet ("-- \n") перед подписью';
$html_mark_as = 'Пометить как';
$html_read = 'прочитанное';
$html_unread = 'непрочитанное';
$html_encoding_label = 'Кодировка символов:';
$html_add = 'Добавить';
$html_contacts = 'Контакты %1$s';
$html_modify = 'Изменить';
$html_back = 'Назад';
$html_contact_add = 'Добавить контакт';
$html_contact_mod = 'Изменить контакт';
$html_contact_first = 'Имя';
$html_contact_last = 'Фамилия';
$html_contact_nick = 'Псевдоним';
$html_contact_mail = 'Адрес электронной почты';
$html_contact_list = 'Список контактов %1$s';
$html_contact_del = 'из контактного листа';
$html_contact_count = '%1$d контактов';
$html_contact_err1 = 'Максимальным числом контактов является %1$d';
$html_contact_err2 = 'Вы не можете добавить новый контакт';
$html_contact_err3 = 'У вас нет прав доступа к контактному листу';
$html_contact_none = 'Контакты не выбраны';
$html_del_msg = 'Стереть выбранные письма?';
$html_down_mail = 'Загрузить';
$original_msg = '-- Исходное письмо --';
$to_empty = 'Поле \'Кому\' не должно быть пустым !';
$html_images_warning = 'Для вашей безопасности удаленные картинки не показаны';
$html_images_display = 'Отобразить картинки';
$html_smtp_error_no_conn = 'Не возможно установить SMTP соединение';
$html_smtp_error_unexpected = 'Неожиданный ответ SMTP:';
$lang_could_not_connect = 'Невозможно подключиться к серверу';
$lang_invalid_msg_num = 'Плохой номер изображения';
$html_file_upload_attack = 'Возможная аттака с помощью загрузки файла';
$html_invalid_email_address = 'Неверный адрес электронной почты';
$html_invalid_msg_per_page = 'Неверное количество писем на странице';
$html_invalid_wrap_msg = 'Неверная ширина строки строки (в символах)';
$html_seperate_msg_win = 'Сообщение в отдельном окне';
$html_err_file_contacts = 'Файл контактов не может быть открыт для записи.';
$html_session_file_error = 'Открыть сессию для письма';
$html_login_not_allowed = 'Логин принят для соединения';
$lang_err_send_delay = 'Вы должны отправить два письма (%1d секунд)';
$html_search = 'Поиск';
