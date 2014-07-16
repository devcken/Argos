$(document).ready(function() {
	var viewer = new RfbViewer();

	viewer.init();
});

(function() {
	function RfbViewer() {
		this._client = null;
	}

	RfbViewer.prototype = {
		init: function () {
			this._canvas = $('<canvas></canvas>');

			this._canvas.bind('contextmenu', function(e) {return false;})

			this._canvas.attr('width', 0);
			this._canvas.attr('height', 0);

			$('body').append(this._canvas);

			this.showHostPanel();

			var RvInstance = this;

			$('#connect_btn').click(function () {
				var host = $('input[name="host"]').val();
				var port = null;

				if (host.indexOf(':') > 0) {
					var item = host.split(':');

					host = item[0];
					port = item[1];
				}

				RvInstance._client = new RfbClient({
					host: host,
					port: port
				});

				RvInstance._client.onConnected = function() { RvInstance.connected(); };

				RvInstance._client.onDisconnected = function() { RvInstance.disconnected(); };

				RvInstance._client.onServerInit = function(serverInit) {
					RvInstance._canvas.attr('width', serverInit.frameBufferWidth);
					RvInstance._canvas.attr('height', serverInit.frameBufferHeight);

					RvInstance.hideHostPanel();

					RvInstance._canvas.focus();
				}

				RvInstance._client.onFramebufferUpdate = function(x, y, width, height, rgbData) {
					var context = RvInstance._canvas[0].getContext('2d');

					var index = 0;
					var base64Image = '';

					while (index < rgbData.length) {
						var incremental = rgbData.length - index > 1000 ? 1000 : rgbData.length - index;

						var sub = rgbData.subarray(index, index + incremental);

						base64Image += String.fromCharCode.apply(String, sub);

						index += incremental;
					}

					var image = new Image(width, height);

					image.onload = function() {
						context.drawImage(image, x, y);
					};

					image.src = 'data:image/png;base64,' + base64Image;
				};
			});
		},
		connected: function () {
			console.log('connected!')

			var RvInstance = this;

			$(document).off('keydown').on('keydown', function(e) {
				RvInstance.handleKeyEvent(e);
			});

			$(document).off('keyup').on('keyup', function(e) {
				RvInstance.handleKeyEvent(e);
			});

			this._canvas.off('mousedown').on('mousedown', function(e) {
				RvInstance.handlePointEvent(e);
			});

			this._canvas.off('mousewheel').on('mousewheel', function(e) {
				RvInstance.handleWheelEvent();
			});

			this._canvas.off('mouseup').on('mouseup', function(e) {
				RvInstance.handlePointEvent(e);
			});

			this._canvas.off('mousemove').on('mousemove', function(e) {
				RvInstance.handlePointEvent(e);
			});
		},
		disconnected: function() {
			console.log('disconnected');

			$(document).off('keydown');
			$(document).off('keyup');

			this._canvas.off('mousedown');
			this._canvas.off('mouseup');
			this._canvas.off('mousemove');

			this._canvas[0].width = this._canvas[0].width;

			this.showHostPanel();
		},
		showHostPanel: function() {
			$('.hostpanel').animate({
				left: '10px'
			}, 1000, function () {
				$('input[name="host"]').focus();
			});
		},
		hideHostPanel: function() {
			$('.hostpanel').animate({
				left: '-298px'
			}, 1000, function () {

			});
		},
		handlePointEvent: function(e) {
			var buttonMask = 0;

			if (e.type == "mousemove") {
				this._client._messageHandler.pointerEvent(buttonMask, e.pageX, e.pageY);
			} else {
				switch (e.button) {
					case 0:
						buttonMask += 1;

						break;

					case 1:
						buttonMask += 2;

						break;

					case 2:
						buttonMask += 4;

						break;
				}

				this._client._messageHandler.pointerEvent(buttonMask, e.pageX, e.pageY);
			}
		},
		handleWheelEvent: function() {
			var delta = 0;

			if (event.wheelDelta) {
				delta = event.wheelDelta;
			} else if (event.detail) {
				delta = -event.detail;
			}

			var buttonMask = -1;

			if (delta <= 0) {
				buttonMask = 8;
			} else {
				buttonMask = 16;
			}

			this._client._messageHandler.pointerEvent(buttonMask, delta, 0);
		},
		handleKeyEvent: function(e) {
			console.log('keyevent:');
			console.log(e);
			this._client._messageHandler.keyEvent(e.type == "keydown", e.keyCode, e.ctrlKey, e.altKey, e.shiftKey);

			if (e.keyCode === 8) {
				e.preventDefault();
			}
		}
	}

	window.RfbViewer = RfbViewer;
})(window);