mergeInto(LibraryManager.library, {

ReadKeyProxy: function (k) {
		var s=readKey(Pointer_stringify(k));
		if (s==null) {
			console.log(key +" not found!");
			return null;
			
		} else {	
    			var bufferSize = lengthBytesUTF8(s) + 1;
    			var buffer = _malloc(bufferSize);
    			stringToUTF8(s, buffer, bufferSize);
    			return buffer;
		}
},

ReadKey: function (k) {
	var key=Pointer_stringify(k);
	var e=document.getElementById(key);
	if (e===null) {
		console.log(key +" not found!");
		return null;
	} else {
		var s=e.textContent;
    		var bufferSize = lengthBytesUTF8(s) + 1;
    		var buffer = _malloc(bufferSize);
    		stringToUTF8(s, buffer, bufferSize);
    		return buffer;
	}
	
 },
ReadJSONBuilderFile: function (t) {
    		var type;
		switch(t) {
			case 1:
			type=jsonNet;
			break;
			case 2:
			type=jsonRoute;
			break;
			case 3:
			type=jsonPoly;
			break;
		}

		var bufferSize = lengthBytesUTF8(type) + 1;
    		var buffer = _malloc(bufferSize);
    		stringToUTF8(type, buffer, bufferSize);
    		return buffer;
},


UrlLogProxy: function (url,v) {
	urlLog(Pointer_stringify(url),Pointer_stringify(v));	
},

UrlLog: function (url,v) {

	var urls=Pointer_stringify(url);
	var XHR = new XMLHttpRequest();
	var FD  = new FormData();
	FD.append('value',Pointer_stringify(v));
	XHR.addEventListener('load', function(event) {
		var warnings = document.getElementById("warnings");
	//	warnings.innerHTML += "Logging to  "+urls+" OK: "+XHR.status +"<br>"+XHR.responseText;
	});
	XHR.addEventListener('error', function(event) {
		var warnings = document.getElementById("warnings");
		warnings.innerHTML += "ERROR saving to "+urls+": "+XHR.status;
	});

	// Set up our request
	XHR.open('POST',urls );
	//
	//     // Send our FormData object; HTTP headers are set automatically
	XHR.send(FD);


},


ExecuteJS: function (code) {
	var f= Function(Pointer_stringify(code));
	return f();
},

});
