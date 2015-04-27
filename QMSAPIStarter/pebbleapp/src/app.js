var UI = require('ui');
var ajax = require('ajax');
var Vector2 = require('vector2');

var webserver = decodeURIComponent(localStorage.getItem('webserver') ? localStorage.getItem('webserver') : 'webserver'); 

var qvserver = localStorage.getItem('qvserver') ? localStorage.getItem('qvserver') : 'qvserver';
var files = localStorage.getItem('files') ? localStorage.getItem('files') : 'files';

// Show splash screen while waiting for data
var splashWindow = new UI.Window();

// Text element to inform user
var text = new UI.Text({
  position: new Vector2(0, 0),
  size: new Vector2(144, 168),
  text:'Downloading data...',
  font:'GOTHIC_28_BOLD',
  color:'black',
  textOverflow:'wrap',
  textAlign:'center',
  backgroundColor:'white'
});

// Add to splashWindow and show
splashWindow.add(text);
splashWindow.show();

// Make request to the nodejs app //, webserver: test
ajax(
  {
    url: webserver + '/getdata',
    method: 'post',
    data: {server: qvserver, files: files, webserver: webserver},
    crossDomain: true    
  },
  function(data) {
    try {
      var items = [];
      data = JSON.parse(data);

      var areas = data.data;
      for(var i = 0; i < areas.length; i++) {
        items.push({ 
          title:areas[i].name //,subtitle:time        
        });            
      }

      var resultsMenu = new UI.Menu({
        sections: [{
          title: 'Areas',
          items: items
        }]
      });
      resultsMenu.on('select', function(e) {
        var innerData = data.data[e.itemIndex].areadata;
        var title = data.data[e.itemIndex].name;

        var details = [];
        for(var d = 0; d < innerData.length; d++) {      
          details.push({
            title: innerData[d].category,
            subtitle: innerData[d].value
          });      
        }

        var detailsMenu = new UI.Menu({
          sections: [{
            title: title,
            items: details}]
        });    
        detailsMenu.show();
      });    

      // Show the Menu, hide the splash
      resultsMenu.show();
      splashWindow.hide();
    } catch (err) {
      var text = new UI.Text({
        position: new Vector2(0, 0),
        size: new Vector2(144, 168),
        text:'Error parsing the data',
        font:'GOTHIC_28_BOLD',
        color:'black',
        textOverflow:'wrap',
        textAlign:'center',
        backgroundColor:'white'
      });

      // Add to splashWindow and show
      splashWindow.add(text);
      splashWindow.show();          
    }
  },
  function(error) {
    var text = new UI.Text({
      position: new Vector2(0, 0),
      size: new Vector2(144, 168),
      text:'Download failed :(',
      font:'GOTHIC_28_BOLD',
      color:'black',
      textOverflow:'wrap',
      textAlign:'center',
      backgroundColor:'white'
    });

    // Add to splashWindow and show
    splashWindow.add(text);
    splashWindow.show();
  });

Pebble.addEventListener('showConfiguration', function(e) {
  console.log("Showing configuration");
  Pebble.openURL('https://googledrive.com/host/0BxjGsOE_3VoOU2RPQ3BjTlBfX0E');
});

Pebble.addEventListener('webviewclosed', function(e) {
  var options = JSON.parse(decodeURIComponent(e.response));
  qvserver = encodeURIComponent(options.qvserver);
  webserver = encodeURIComponent(options.webserver);
  files = encodeURIComponent(options.files);

  if(qvserver == 'undefined') {
    qvserver = 'http://localhost:4799/QMS/Service';
  }

  localStorage.setItem('qvserver', qvserver);
  localStorage.setItem('webserver', webserver);
  localStorage.setItem('files', files);

  //console.log("Configuration window returned: ", JSON.stringify(options));
});

//Send a string to Pebble
var dict = { 
  QVSERVER : qvserver, 
  WEBSERVER: webserver,
  FILES: files
};

Pebble.sendAppMessage(dict, function(e) {
  console.log("Send successful.");
}, function(e) {
  console.log("Send failed!");
});


