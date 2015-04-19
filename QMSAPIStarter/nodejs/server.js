var express = require('express');
var app = express();
var bodyParser = require('body-parser')
var spawn = require('child_process').spawn;
 
app.use(bodyParser.urlencoded({ extended: false }))

app.get('/', function (req, res) {
  res.send('Hello World')
})

app.post('/getdata', function (req, res) {	
	var server = req.body.server;
	var files = req.body.files;
	
	var child = spawn('../bin/Debug/QlikviewToPebble.exe', ['-s', server, '-p', files]);
	child.stdout.on('data', function(chunk) {
		var output = chunk.toString();
		res.send(output);

	});
})
 
var server = app.listen(3000, function () {

  var host = server.address().address;
  var port = server.address().port;

  console.log('Server listening at http://%s:%s', host, port);
});