/*
 * This file overwrites the default config from CKEditor.
 */

CKEDITOR.editorConfig = function( config )
{
    config.toolbar_NOCC = [
        ['FontName','FontSize'],
        ['TextColor','BGColor'],
        ['Cut','Copy','Paste'],
        ['Undo','Redo'],
        ['Source'],
        '/',
        ['Bold','Italic','Underline','StrikeThrough','-','Subscript','Superscript','-','RemoveFormat'],
        ['OrderedList','UnorderedList','-','Outdent','Indent'],
        ['JustifyLeft','JustifyCenter','JustifyRight','JustifyFull'],
        ['Link','Unlink','-','Rule','-','SpecialChar'],
        ['About'] // No comma for the last row.
    ];
    config.toolbar = 'NOCC';

    config.removeDialogTabs = 'link:advanced;link:target';
};