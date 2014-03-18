/******************************************************************************
 *********** Zdrojový kód javascript pro stránky buildingforlife.cz ***********
 ****************  Pavel Baštecký anebril(a)seznam.cz (c) 2012  ***************
 ******************************************************************************/
 
 //Odpočet přesměrování
function RefreshCuntdown(num)
{
  document.getElementById('refresh_number').innerHTML = num;
  if(num > 0)
  {
    var fce = "RefreshCuntdown("+ (num-1) +")";
    setTimeout(fce,1000);
  }
}

//Zobrazí skrytý příspěvek v diskusi
function ShowComment(id)
{   
  var text = document.getElementById('text_' + id)
  var title = document.getElementById('title_' + id);
  var error = document.getElementById('error_' + id);
  
  if (text) text.style.display = 'block';
  if (title) title.style.display = 'inline';
  if (error) error.style.display = 'none';

  return false;
}

var galeryData =
{
  images:null,
  length:0
}


/****************************** Obsluha galerie *******************************/
 
function ShowGalery(index)
{
  if (galeryData.images != null)
  {
    var source = new Array();
    for(i = 0; i < galeryData.length; i++)
      source[i] = galeryData.images[i];

  
    $.fancybox(source ,
      {
    		'overlayShow'	: true,
    		'transitionIn'	: 'elastic',
    		'transitionOut'	: 'elastic',
        'autoScale' : true,
        'centerOnScroll' : true,
        'type' : 'image',
        'padding' : 0,
        'index' : index
      }
    );
    return false;
  }
  else return true;
}

function SetImageArray(images)
{
  galeryData.images = images;
  galeryData.length = images.length;
}
 

/***************************** Formuláře stránek ******************************/
  
/*** Testy formulářových prvků ***/
  
  //Zobrazí chybové hlášení u prvku na pozici ID 
  function ThrowFormError(id, error, length)
  {
    var txt = document.getElementById('col' + id)
    var color = document.getElementById('txt' + id);
    
    if (length > -1) document.getElementById('num' + id).innerHTML = length;
    
    txt.title = error;
    document.getElementById('err_' + id).innerHTML = error;
    
    if(error)
    {
      if (length > -1) color.style.color = "red";
      txt.style.borderColor = "red";
    }
    else
    {
      if (length > -1) color.style.color = "black";
      txt.style.borderColor = "black";
    }
  }
  
  //Vyvolání testu obsahu formuláře - odchyt události onkeyup a onblur
  function Typing(id)
  {
    TestCollum(id, false);
  }
  
  //Otestování celého formuláře před odesláním
  function FormCheck()
  {
    var e = true;
    for (var i = 1; i < scripts.length; i++)
      e = TestCollum(i, true) && e;
    
    if (e) document.getElementById('form_error').style.display = "none";
    else
    {
      document.getElementById('form_error').style.display = "";
    }
    return e;
  }
  
  //Realizace testu formulářového prvku
  function TestCollum(id, showError)
  {  
    if (id > scripts.length) return false;
    
    var txt = document.getElementById('col' + id);
    var stat = document.getElementById('c' + id);
    var lenNumber = document.getElementById('num' + id);
    var errBox = document.getElementById('err_' + id);
    
    var scr = scripts[id], min = minims[id], max = maxims[id];
    
    error = '';
    switch (scr)  //Výběr testovací metody
    {
      case 'txt': error = LenghtTest(txt.value, min, max, true); break;
      case 'not': error = LenghtTest(txt.value, min, max, false); break;
      case 'num': error = NumberTest(txt.value, min, max); break;
      case 'spam': error = SpamTest(txt.value, min, max); break;
      case 'psck':error = PasswordRepeate(txt.value, id - 1); break;
      case 'mail':error = MailTest(txt.value, min > 0);
    }

    if (stat != null) stat.className = error ? "form_item error" : "form_item";
    if (txt != null)
    {
      txt.title = error;
      if (lenNumber != null) lenNumber.innerHTML = txt.value.length;
    }

    if (showError && errBox != null)
    {
      errBox.innerHTML = error;
      errBox.style.display = error ? "" : "none";
    }
    
    return error == "";
  }

  //Test opakování hesla - pole pro heslo a opakované heslo musí sousedit
  function PasswordRepeate(repeate, PassId)
  {
    return repeate == document.getElementById('col' + PassId).value ? '' :
                        "Heslo a opakované heslo se musí shodovat";
  }
  
  //Test délky vstupu
  function LenghtTest(value, min, max, duly)
  {
    var length =  value.length;
    error = "";
    

    if(!length)
    {
      if (duly && min) error = "Toto pole je povinné.";
    }
    else if(length < min)
      error = "Toto pole musí obsahovat nejméně "
            + min + " znak" + (min > 4 ? "ů" : min > 1 ? "y" : "") + ", zadal(a) jste "
            + length + " znak" + (length > 4 ? "ů" : length > 1 ? "y" : "") + "!";

    else if(length > max)
      error = "Toto pole musí obsahovat nejvíce "
            + max + " znak" + (max > 4 ? "ů" : max > 1 ? "y" : "") + ", zadal(a) jste "
            + length + " znak" + (length > 4 ? "ů" : length > 1 ? "y" : "") + "!";

    return error;
  }
  
  //Test číselnosti a mezí
  function NumberTest(number, min, max)
  {
    if(!number.length) number = 0;

    error = "";
    
    if (!/(^[1-9]{1}[0-9]*$)|0/.test(number) || number < min || number > max)
      error = "Do tohoto pole smí být zadáno číslo od "+ min +" do "+ max +"!";
    
    return error;
  }
  
  //Test kontroly proti spamu
  function SpamTest(number, min, max)
  {
    if(!number.length) number = 0;

    error = "";
    
    if (number != min + max)
      error = "Antispamová kontrola - zadejte prosím součet čísel "+ min +" a "+ max +"!";
    
    return error;
  }
  
  //Test e-mailu
  function MailTest(text, duly)
  {
    var error = "";
    if (text.length == 0)
    {
      if (duly) error = "Toto pole jepovinné";
    }
    else if(!/.+@.+\..+/.test(text) || / /.test(text)) error = "Zadejte platnou e-mailovou adresu";
    return error;
  }


/***************************** Formuláře stránek ******************************/
/*********************************** Konec ************************************/


