/***************************************************************************************************
*************************  Zdrojový kód JAVASCRIPTU pro stránky Pestrá Škola  **********************
****************************************************************************************************
Pro stránky Pestrá škola a školka Bobinice napsal Pavel Baštecký (c) září 2010 - anebril<a>seznam.cz
***************************************************************************************************/
 
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

//Vyslání AJAX požadvku

var ajaxFailureAdres = window.location;
function server_request(http_request, reply, param, adr, method, post)
{
  if(http_request)
  {
    if (http_request.readyState == 4)
    {
      if (http_request.status == 200) reply(http_request.responseText, param); 
      else if(http_request.status == 0) alert(http_request.responseText);
      else if(ajaxFailureAdres) window.location = ajaxFailureAdres;
      else alert('a');
    }
  }
  else 
  {
    if (window.XMLHttpRequest) // Mozilla, Safari,...
      http_request = new XMLHttpRequest();
    
    else if (window.ActiveXObject) //IE
    { 
      try { http_request = new ActiveXObject("Msxml2.XMLHTTP"); }
      catch (e)
      {
        try  {http_request = new ActiveXObject("Microsoft.XMLHTTP"); }
        catch (e) {}
      }
    }
    if (!http_request){return true;}
    adr = adr +"&r=" + Math.ceil(Math.random()*100)
    http_request.onreadystatechange = function() {server_request(http_request,reply,param,adr,method,post);}
    http_request.open(method, adr, true);
    if(method=='post')http_request.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    http_request.send(post);
  }
  return false;
}


function Ajax(adres)
{
  var target = document.getElementById('ajax_target');
  if (target == null) return true;
  
  ajaxFailureAdres = "index.php?" + adres;
  return server_request(false, PrintAjaxData, 0, "ajax.php?" + adres, 'get', null);
}

function PrintAjaxData(data, param)
{
  var target = document.getElementById('ajax_target');
  target.innerHTML = data;
}

//Navrati dvourozmerne pole s hodnotami oscrollovani prohlizece [scrollLeft, scrollTop]
function getScrollXY() 
{
  var scrOfX = 0, scrOfY = 0;
  if( typeof( window.pageYOffset ) == 'number' )
  {
    //Netscape compliant
    scrOfY = window.pageYOffset;
    scrOfX = window.pageXOffset;
  } 
  else if( document.body && ( document.body.scrollLeft || document.body.scrollTop ) )
  {
    //DOM compliant
    scrOfY = document.body.scrollTop;
    scrOfX = document.body.scrollLeft;
  } 
  else if( document.documentElement && ( document.documentElement.scrollLeft || document.documentElement.scrollTop ) )
  {
    //IE6 standards compliant mode
    scrOfY = document.documentElement.scrollTop;
    scrOfX = document.documentElement.scrollLeft;
  }
  return [ scrOfX, scrOfY ];
}

//Navrati dvourozmerne pole s velikostmi okna prohlizece [width, height]
function getWindowSizeXY()
{
  var winW = 630, winH = 460;
  if (document.body && document.body.offsetWidth)
  {
    winW = document.body.offsetWidth;
    winH = document.body.offsetHeight;
  }
  if (document.compatMode=='CSS1Compat' && document.documentElement && document.documentElement.offsetWidth ) 
  {
    winW = document.documentElement.offsetWidth;
    winH = document.documentElement.offsetHeight;
  }
  if (window.innerWidth && window.innerHeight)
  {
    winW = window.innerWidth;
    winH = window.innerHeight;
  }
  
  return [ winW, winH ];
}

//Vypocita a nastavi pozici objektu do stredu obrazovky
function CountObjectposition(wnd)
{
  var size = getWindowSizeXY();
  var scroll = getScrollXY();
    
  var w = (size[0] - wnd.offsetWidth ) / 2 + scroll[0];
  var h = (size[1] - wnd.offsetHeight) / 2 + scroll[1];
  if (w < 0) w = 0;
  if (h < 0) h = 0;
  
  wnd.style.top = h +"px";
  wnd.style.left = w +"px";
}



/***************************** Formuláře stránek ******************************/

/*** Vyvolání formuláře ***/

//Následující funkce zobrazí plovoucí okno s formulářem (nemusí se znovu načíst celá stránka)

  //Datová reprezentace okna formuláře
  var form =
  {
  	wnd:null, box:null,    //plovoucí okno formuláře; IFRAME formuláře
  	wndId:'',              //ID okna formuláře
  	adress:''              //adresa stránky v IFRAME
  }
  //Iniciace datové třídy
  function FormInit(wnd)
  {
    form.wndId = wnd;
    form.wnd = document.getElementById(wnd);
    form.box = document.getElementById("box_"+ wnd);
  }
  //Iniciace datové třídy z IFRAME
  function FormInitIframe(wnd)
  {
    form.wndId = wnd;
    form.wnd = window.parent.document.getElementById(wnd);
    form.box = window.parent.document.getElementById("box_"+ wnd);
  }
  
  //Zobrazí plovoucí okno s formulářem
  function ShowForm(adress)
  {
    //if (form.adress != adress)
    {
      form.box.src = 'form.php?'+ adress + "&wnd=" + form.wndId + "&rnd=" + Math.random() ;
      form.adress = adress;
    }
  
    form.wnd.style.display = "block";
    CountObjectposition(form.wnd);
    return false;
  }
  
  //Realizuje přesměrování hlavního okna stránek z elementu IFRAME
  function MakeRefresh(adress, mode)
  {
    switch (mode)
    {
      case 1: window.location = adress; break;
      case 2: window.parent.location = adress; break;
      default: window.parent.location = window.parent.location; break;
    }
  }
  //Změna rozměrů okna formláře - voláno z elementu IFRAME
  function RefreshIframeSize()
  {
    if (window.parent && window.parent != window)
    {
      var div = document.getElementById('form_div');
      form.box.height = div.offsetHeight ;
      
      window.parent.CountObjectposition(form.wnd);
    }
  }


/*** Vkládání editačních značek do formuláře ***/

  //Vkládání smajlíků a formátovacích prvků na místo kurzoru
  //Zdroj: http://programujte.com/index.php?akce=diskuze&kam=vlakno&tema=8035-vkladanie-smajliku-do-textu
  //http://www.alexking.org - LGPL
  //Provedl jsem jen drobné změny a korekce, původní kód zustal zachován
  function insertAtCursor(id, prefix, postfix, newline)
  {
    myField = document.getElementById(id);
    var re = new RegExp("^(.*\\S)(\\s*)$");
    myField.focus();
    if (newline && myField.value)prefix = "\n" + prefix;

    if (document.selection) //IE support
    {
      sel = document.selection.createRange();
      var selection = sel.text;
      var wasEmpty = (selection == "");
      var space = "";
      
      if (!wasEmpty)
      {
        var matches = selection.match(re);
        if (matches)
        {
          selection = RegExp.$1;
          space = RegExp.$2;
        }
      }
  
      sel.text = prefix+selection+postfix+space;
      sel.collapse(false);
  
      if (wasEmpty){sel.moveEnd('character',-(postfix.length))}
      sel.select();
    }
    else //MOZILLA/NETSCAPE support
    {
      if (myField.selectionStart || myField.selectionStart == '0')
      {
        var startPos = myField.selectionStart;
        var endPos = myField.selectionEnd;
        var selection = myField.value.substring(startPos, endPos);
        var wasEmpty = (startPos == endPos);
        var space = "";
        
        if (!wasEmpty)
        {
          var matches = selection.match(re);
          if (matches) {
            selection = RegExp.$1;
            space = RegExp.$2;
          }
        }
        myField.value = myField.value.substring(0, startPos)
                      + prefix+selection+postfix+space
                      + myField.value.substring(endPos, myField.value.length);
  
        var newPosition;
        if (wasEmpty){newPosition = startPos+prefix.length;}
        else{newPosition = startPos+prefix.length+selection.length+postfix.length+space.length;}
        myField.setSelectionRange(newPosition, newPosition);
      }
      else //Insert at the end
      {
        myField.value += prefix+postfix;
        myField.setSelectionRange(startPos+prefix.length, startPos+prefix.length);
      }
    }
  }
  
  //Pomocí hlášky načte od uživatele celé číslo
  function LoadNumber(msg)
  {
    var number = prompt(msg, 1) *1;
    if (!number || number < 1)return 0;
    return Math.floor(number);
  }
  
  //Podle uživatelského zadání vloží do formulářového prvku tabulku s přesným počtem sloupců a řádek
  function InsertTable(id)
  {
    var cols = LoadNumber("Zadejte počet sloupců tabulky");
    if (!cols) return;
    var rows = LoadNumber("Zadejte počet řádků tabulky");
    if (!rows) return;
    
    var pre = "[TABULKA]\n[RADEK]\n[BUNKA]";
    var pos = "[/BUNKA]";
    for (x = 1; x < cols; x++) pos += "\n[BUNKA][/BUNKA]";
    pos += "\n[/RADEK]";
    
    for (y = 1; y < rows; y++)
    {
      pos += "\n[RADEK]";
      for (x = 0; x < cols; x++) pos += "\n[BUNKA][/BUNKA]";
      pos += "\n[/RADEK]";
    }
    
    pos += "\n[/TABULKA]";
    
    insertAtCursor(id,pre,pos,1);
  }
  
  //Vloží jeden řádek tabulky o počtu sloupců dle zadání
  function InsertRow(id)
  {
    var cols = LoadNumber("Zadejte počet sloupců tabulky");
    if (!cols) return;
    
    var pre = "\n[RADEK]\n[BUNKA]";
    var pos = "[/BUNKA]";
    for (x = 1; x < cols; x++) pos += "\n[BUNKA][/BUNKA]";
    pos += "\n[/RADEK]";
    
    insertAtCursor(id,pre,pos,1);
  }
  
  //Vloží číslovaný seznam s vybraným počtem položek
  function InsertNumList(id)
  {
    var lines = LoadNumber("Zadejte počet položek číslovaného seznamu");
    if (!lines) return;
    
    var pre = "\n[CISLOVANY]\n[POLOZKA]";
    var pos = "[/POLOZKA]";
    for (x = 1; x < lines; x++) pos += "\n[POLOZKA][/POLOZKA]";
    pos += "\n[/CISLOVANY]";
    
    insertAtCursor(id,pre,pos,1);
  }
  
  //Vloží seznam s vybraným počtem položek
  function InsertList(id)
  {
    var lines = LoadNumber("Zadejte počet položek seznamu");
    if (!lines) return;
    
    var pre = "\n[SEZNAM]\n[POLOZKA]";
    var pos = "[/POLOZKA]";
    for (x = 1; x < lines; x++) pos += "\n[POLOZKA][/POLOZKA]";
    pos += "\n[/SEZNAM]";
    
    insertAtCursor(id,pre,pos,1);
  }
  
  //Vloží odkaz se zadanou adresou a popiskem
  function InsertLink(id)
  {
    var link = prompt("Zadejte cílovou adresu odkazu");
    if (!link) return;
    var text = prompt("Zadejte popisek odkazu", link);
    if (!text) text = link;
    
    insertAtCursor(id,'[ODKAZ='+ link +']'+ text +'[/ODKAZ]','',0);
  }
  
  
  
  
  var textId = 0;
  function InsertImage(id, gid, aid, str)
  {
    if (id != 0) textId = id;
    var box = document.getElementById('insert_image');
    
    if (textId == 0) return false;
    if (box == null) { insertAtCursor(textId,'[IMG]','[/IMG]',0); return false; }
    
    ajaxFailureAdres = '';
    
    server_request(false, AjaxImage, 0, "insert_image.php?p="+ gid +"&i="+ aid +"&s="+ str, 'get', null);
  }
  
  function InsertImageTag(image)
  {
    if (textId == 0) return false;
    
    insertAtCursor(textId,'[IMG]'+ image +'[/IMG]','',0);
    var box = document.getElementById('insert_image');
    box.style.display = 'none';
    RefreshIframeSize();
  }
  
  function AjaxImage(data, param)
  {
    var box = document.getElementById('insert_image');
    box.style.display = 'block';
    box.innerHTML = data;
    RefreshIframeSize();
  }

  
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
    
    if (e) document.getElementById('e0').style.display = "none";
    else
    {
      document.getElementById('e0').style.display = "";
      RefreshIframeSize();
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
      case 'psck':error = PasswordRepeate(txt.value, id - 1); break;
      case 'mail':error = MailTest(txt.value, min > 0);
    }

    if (stat != null) stat.className = error ? "error" : "";
    if (txt != null)
    {
      txt.title = error;
      if (lenNumber != null) lenNumber.innerHTML = txt.value.length;
    }

    if (showError && errBox != null)
    {
      errBox.innerHTML = error;
      document.getElementById('e' + id).style.display = error ? "" : "none";
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
      error = "Toto pole musí obsahovat nejméně "
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



/******************************* Drag and Drop ********************************/
//System drag and drop inspirovaný příklady ze stránky Petra Mlicha
//http://www.volny.cz/peter.mlich/Pr/efekty/drag/ppmoving2.htm
//Promenna 'dad' je vyuzivana i funkcemi pro zmenu velikosti alba
//Nasledujici oddil drag and drop je univerzalne pouzitelny pro vsechna okna na strankach
//System pro zmenu velikosti neni univerzalni, jeho metody jsou svazany z prvky galerie

//Parametry aktivniho (prenaseneho) okna
var dad =
{
	obj:null,                      //Prenasene okno
	tempX:0, tempY:0,              //Relativni pozice mysi vzhledem k prenasenemu oknu
	coorX:0, coorY:0,              //Pocatecni pozice leveho horniho rohu okna
	bottX:0, bottY:0,              //Pocatecni pozice praveho dolniho rohu okna
	mode:'', vMode:'', hMode:''    //Aktivni mod (drop/resize) a parametry
}
var html = null, body = null;    //Tag html, body

//Skryti prvku na strance podle id
function WndClose(id)
{
  document.getElementById(id).style.display = "none";
  return false;
}

//Reset systemu pri uvolneni mysi
function onMouseUp(e)
{
  dad.obj = null;
  dad.mode = '';
  document.onmousemove = new function(){return true;}
}

//Navraci vektor pozice mysi
function GetCursorPosition(e)
{
  var pt = {xPos:0, yPos:0};
  
  if (document.all)
  {
  	pt.xPos = event.clientX + (Boolean(body.scrollLeft) ? body.scrollLeft : 0);
  	pt.yPos = event.clientY + (Boolean(body.scrollTop)  ? body.scrollTop  : 0);
  }
  else if (document.layers || document.getElementsByTagName)
  {
  	pt.xPos = e.pageX;
  	pt.yPos = e.pageY;
  }
  return pt;
}

//Premisti objekt na novou pozici
function SetNewPosition(obj, xPos, yPos)
{
  if (document.layers)
  {
  	obj.pageX = xPos;
  	obj.pageY = yPos;
  }
  else if (document.all || obj.getElementsByTagName)
  {
  	obj.style.left = xPos + "px";
  	obj.style.top  = yPos + "px";
  }
}

//Inicializace systemu
function DragAndDropInit()
{
  html = document.getElementsByTagName('html')[0];
  body = document.getElementsByTagName('body')[0];

  if (document.layers) document.captureEvents(Event.MOUSEMOVE | Event.MOUSEUP);
  document.onmouseup = onMouseUp;
}

//Spusti operaci 'Drag and Drop' na oknu dle id
function StartDragAndDrop(id, e)
{
	dad.obj = document.getElementById(id);
	dad.mode = "drop";
	document.onmousemove = DragAndDropMouseMove;
	
	//Zaznamenani relativni pozice kurzoru
  var pt = GetCursorPosition(e);
	if (document.layers)
	{
		dad.tempX = pt.xPos;
		dad.tempY = pt.yPos;
  }
	else if (document.all || dad.obj.getElementsByTagName)
	{
		dad.tempX = pt.xPos - dad.obj.offsetLeft;
		dad.tempY = pt.yPos - dad.obj.offsetTop;
  }
	return false;
}

//Presun okna pohybem mysi
//Nova pozice je rozdil vektoru pozice kurzoru na strance a relativne vudci oknu
function DragAndDropMouseMove(e)
{
  if (dad.mode != 'drop') return true;
  else	
  {
    var pt = GetCursorPosition(e);
    SetNewPosition(dad.obj, pt.xPos - dad.tempX, pt.yPos - dad.tempY);
  	return false;
  }
}

/******************************* Drag and Drop ********************************/
/*********************************** Konec ************************************/


/******************************* Funkce galerie *******************************/
//Funkce pro zobrazovani nahledu obrazku a zmenu jejich velikosti
//Okno s obrazkem vyuziva 'Drag and Drop' a zmena velikosti je umoznena tazenim za okraj - cast Resize
//Funkce pro zmenu velikosti vyuzivaji nektere metody 'Drag and Drop'

//Parametry okna galerie
var alb = 
{
  //Objekty: okno; popisek; div obrazku; obrazek; odkaz na otevreni nahledu
  wnd:null, text:null, img:null, image:null, href:null,
  images:null,                      //Pole vektoru obrazku {src:'cesta', width:#, height:#}
  name:'', imagesCt:0, accImage:1,  //Jmeno galerie; pocet obrazku; aktualni obrazek
  minWidth:0, minHeight:0,          //Minimalni rozmer okna nahledu
  borderWidth:0, borderHeight:0     //Rozmer odsazeni vnitrniho okna vzhledem k hlavnimu
}

//Inicializace alba
function AlbumInit(name, wnd, images, minWidth,minHeight,borderWidth,borderHeight)
{
  alb.wnd = document.getElementById(wnd);
  alb.text = document.getElementById(wnd +"_text");
  alb.img = document.getElementById(wnd +"_img");
  alb.image = document.getElementById(wnd +"_image");
  alb.href = document.getElementById(wnd +"_href");
  alb.images = images;
  alb.name = name;
  alb.imagesCt = images.length - 1;
  alb.minWidth = minWidth;
  alb.minHeight = minHeight;
  alb.borderWidth = borderWidth;
  alb.borderHeight = borderHeight;
}
  
//Zobrazeni obrazku na pozici pos
// 0: nasledujici obrazek; -1: predchozi obrazek;
//Pokud je okno skryto, dojde k zobrazeni
function ShowImage(pos)
{
  if (pos == 0) pos = alb.accImage + 1;
  else if (pos == -1) pos = alb.accImage - 1;

  if (pos < 1) alb.accImage = alb.imagesCt;
  else if (pos > alb.imagesCt) alb.accImage = 1;
  else alb.accImage = pos;
  
  var t = alb.name + " | "+ alb.accImage + ". obrázek z "+ alb.imagesCt;
  var i = alb.images[alb.accImage];
  alb.text.innerHTML = t;
  alb.href.href = i.src;
  alb.image.src = i.src;
  alb.image.alt = t;
  
  if (alb.wnd.style.display != "block")
  {
    alb.wnd.style.display = "block";
    ResizeAlbum(800, 500);
  }
  else ResizeImage();
  return false;
}

//Otevre nove okno prohlizece s obrazkem
function ShowImageWindow()
{
  var i = alb.images[alb.accImage];
  var w = i.width + 30;
  var h = i.height + 30;
  var wnd = window.open(i.src, 'wnd', 'width='+ w +', height='+ h +',resizable, scrollbars');
  if (!wnd) return true;
  wnd.focus();
  return false;
}

//Zmeni velikost okna alba
function ResizeAlbum(width, height)
{
  //zmena velikosti okna a divu obrazku
  if (height < alb.minHeight) { height = alb.minHeight; }
  alb.img.style.height = (height - alb.borderHeight) + "px";
  alb.wnd.style.height = height + "px";

  if (width < alb.minWidth) { width = alb.minWidth; }
  alb.img.style.width = (width - alb.borderWidth) + "px";
  alb.wnd.style.width = width + "px";
  
  //Vystredeni okna
  CountObjectposition(alb.wnd);
  
  ResizeImage();
  return false;
}

//Zmena velikosti obrazku podle velikosti divu
function ResizeImage()
{
  var h = alb.img.offsetHeight, w = alb.img.offsetWidth;
  var i = alb.images[alb.accImage];
  
  //Nova velikost
  var width = i.width, height = i.height;
  if (width > w) {height = (height / width) * w; width = w; }
  if (height > h) {width = (width / height) * h; height = h; }
  alb.image.width = Math.round(width);
  alb.image.height = Math.round(height);
  
  //Vycentrovani
  alb.image.style.top = (Math.round((h - height) / 2)) + "px";
  //alb.image.style.left = (Math.round((w - width) / 2)) + "px";
}

//Zacatek zmeny velikosti tazenim
function StartAlbumResize(e, dirrection)
{
  dad.mode = dirrection + "_res";
	dad.mode = "resize";
  
  //Nacteni parametru
  switch(dirrection)
  {
    case 't':  dad.vMode = 't'; dad.hMode = '';   break;
    case 'tl': dad.vMode = 't'; dad.hMode = 'l';  break;
    case 'tr': dad.vMode = 't'; dad.hMode = 'r';  break;
    case 'b':  dad.vMode = 'b'; dad.hMode = '';   break;
    case 'bl': dad.vMode = 'b'; dad.hMode = 'l';  break;
    case 'br': dad.vMode = 'b'; dad.hMode = 'r';  break;
    case 'l':  dad.vMode = ''; dad.hMode = 'l';   break;
    case 'r':  dad.vMode = ''; dad.hMode = 'r';   break;
  }
	document.onmousemove = AlbumResizeMouseMove;
	
  var pt = GetCursorPosition(e);
  
  var w = alb.wnd.offsetWidth, h = alb.wnd.offsetHeight;
  //V mozzile je nutno odecist velikost ramecku
  if (!document.all && alb.wnd.getElementsByTagName)
  {
    w -= 4; h -= 4;
  }
  
  //Zaznamenani pozice mysi v oknu a koordinatu okna
	if (document.layers)
	{
		dad.tempX = pt.xPos;
		dad.tempY = pt.yPos;
  	dad.bottX = alb.wnd.offsetWidth;
  	dad.bottY = alb.wnd.offsetHeight;
  	dad.coordX = -alb.wnd.offsetWidth;
  	dad.coordY = -alb.wnd.offsetHeight;
  }
	else if (document.all || alb.wnd.getElementsByTagName)
	{
		dad.tempX = pt.xPos - alb.wnd.offsetLeft;
		dad.tempY = pt.yPos - alb.wnd.offsetTop;
    dad.bottX = alb.wnd.offsetLeft + w;
  	dad.bottY = alb.wnd.offsetTop + h;
  	dad.coordX = alb.wnd.offsetLeft - w;
  	dad.coordY = alb.wnd.offsetTop - h;
  }
	
	return false;
}

//Zmena velikosti tazenim
function AlbumResizeMouseMove(e)
{
  if (dad.mode != 'resize') return true;
  else	
  {
    //Promenne pro vektory pozice okna a jeho velikosti
    var heig = 0, widt = 0, xPos = 0, yPos = 0;
    var pt = GetCursorPosition(e);
  	
    //Nova pozice leveho horniho rohu okna v pripade tazeni
    pt.xPos -= dad.tempX; pt.yPos -= dad.tempY;
    
    //Vypocet hodnot
    switch (dad.vMode)
    {
      case 't': heig = dad.bottY - pt.yPos;   yPos = pt.yPos;             break;
      case 'b': heig = pt.yPos - dad.coordY;  yPos = alb.wnd.offsetTop;   break;
      default: heig = -1; yPos = alb.wnd.offsetTop;   break;
    }
    switch (dad.hMode)
    {
      case 'l': widt = dad.bottX - pt.xPos;   xPos = pt.xPos;             break;
      case 'r': widt = pt.xPos - dad.coordX;  xPos = alb.wnd.offsetLeft;  break;
      default: widt = -1; xPos = alb.wnd.offsetLeft;  break;
    }
    
    //Zmena velikosti okna a divu obrazku, hodnota -1 znamena, ze nedojde ke zmene
    if (heig != -1)
    {
      if (heig < alb.minHeight)
      {
        heig = alb.minHeight;
        //Vypocet mezni polohy okna (jinak dochazelo k pokakovani z duvodu rychlejsi mysi)
        yPos = dad.vMode == 't' ? dad.bottY - alb.minHeight : alb.wnd.offsetTop;
      }
      alb.img.style.height = (heig - alb.borderHeight) + "px";
      alb.wnd.style.height = heig + "px";
    }
    if (widt != -1)
    {
      if (widt < alb.minWidth)
      {
        widt = alb.minWidth;
        xPos = dad.hMode == 'l' ? dad.bottX - alb.minWidth : alb.wnd.offsetLeft;
      }
      alb.img.style.width = (widt - alb.borderWidth) + "px";
      alb.wnd.style.width = widt + "px";
    }
    
    //Presun okna (pokud se tahlo za horni, nebo levy okraj) a vycentrovani obrazku
    SetNewPosition(alb.wnd, xPos, yPos);
    ResizeImage();
  	
  	return false;
  }
}

/******************************* Funkce galerie *******************************/
/*********************************** Konec ************************************/
