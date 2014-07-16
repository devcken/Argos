(function() {
	function MessageHandler(socket) {
		this._socket = socket;
		this._littleEndian = false;
	}

	MessageHandler.prototype = {
		onFramebufferUpdate: function(x, y, width, height, rgbData) {},

		handle: function(data) {
			var binary = new DataView(data);

			var messageType = binary.getUint8(0);

			switch (messageType) {
				// FramebufferUpdate
				case 0:
					this.updateFramebuffer(binary);

					break;

				// SetColourMapEntries
				case 1:
					var firstColour = binary.getUint16(2, this._littleEndian);
					var numberOfColour = binary.getUint16(4, this._littleEndian);
					var colours = new DataView(data, 6);

					this.setColourMapEntries(firstColour, numberOfColour, colours);

					break;

				// Bell
				case 2:
					this.bell();

					break;

				// ServerCutText
				case 3:
					var text = new DataView(data, 8);

					this.serverCutText(text);

					break;
			}
		},

		updateFramebuffer: function(binary) {
			var offset = 4;

			var x = binary.getUint16(offset, this._littleEndian); offset += 2;
			var y = binary.getUint16(offset, this._littleEndian); offset += 2;
			var width = binary.getUint16(offset, this._littleEndian); offset += 2;
			var height = binary.getUint16(offset, this._littleEndian); offset += 2;
			var encodingType = binary.getInt32(offset, this._littleEndian); offset += 4;

			var rgbData = new Uint8Array(binary.buffer, offset);

			this.onFramebufferUpdate(x, y, width, height, rgbData);
		},

		setColourMapEntries: function(firstColour, numberOfColours, colours) {

		},

		bell: function() {

		},

		serverCutText: function(text) {

		},

		setPixelFormat: function(pixelFormat) {
			var buffer = new ArrayBuffer(0);
			var binary = new DataView(buffer);

			var offset = 0;

			binary.setUint8(offset++, 0x00);                        // message-type
			binary.setUint8(offset++, 0x00);                        // padding
			binary.setUint8(offset++, 0x00);                        // padding
			binary.setUint8(offset++, 0x00);                        // padding
			binary.setUint8(offset++, pixelFormat.bitPerPixel);     // PIXEL_FORMAT.bit-per-pixel
			binary.setUint8(offset++, pixelFormat.depth);           // PIXEL_FORMAT.depth
			binary.setUint8(offset++, pixelFormat.bigEndianFlag);   // PIXEL_FORMAT.big-endian-flag
			binary.setUint8(offset++, pixelFormat.trueColourFlag);  // PIXEL_FORMAT.true-colour-flag
			binary.setUint16(offset+=2, pixelFormat.redMax);         // PIXEL_FORMAT.red-max
			binary.setUint16(offset+=2, pixelFormat.greenMax);      // PIXEL_FORMAT.green-max
			binary.setUint16(offset+=2, pixelFormat.blueMax);       // PIXEL_FORMAT.blue-max
			binary.setUint8(offset++, pixelFormat.redShift);       // PIXEL_FORMAT.red-shift
			binary.setUint8(offset++, pixelFormat.greenShift);      // PIXEL_FORMAT.green-shift
			binary.setUint8(offset++, pixelFormat.blueShift);       // PIXEL_FORMAT.blue-shift
			binary.setUint8(offset++, 0x00);                        // padding
			binary.setUint8(offset++, 0x00);                        // padding
			binary.setUint8(offset++, 0x00);                        // padding

			this._socket.send(binary.buffer);
		},

		setEncodings: function(encodings) {
			var buffer = new ArrayBuffer(0);
			var binary = new DataView(buffer);

			var offset = 0;

			binary.setUint8(offset++, 0x02);                // message-type
			binary.setUint8(offset++, 0x00);                // padding
			binary.setUint16(offset+=2, encodings.length);   // number-of-encodings

			for (var index in encodings) {
				var item = encodings[index];

				binary.setInt16(offset+=2, item);           // encoding-type
			}

			this._socket.send(binary.buffer);
		},

		framebufferUpdateRequest: function(entire, x, y, width, height) {
			var buffer = new ArrayBuffer(10);
			var binary = new DataView(buffer);

			var offset = 0;

			binary.setUint8(offset++, 0x03);                    // message-type
			binary.setUint8(offset++, entire ? 0x00 : 0x01);    // incremental
			binary.setUint16(offset, x);                      // x-position
			binary.setUint16(offset+=2, y);                     // y-position
			binary.setUint16(offset+=2, width);                 // width
			binary.setUint16(offset+=2, height);                // height

			this._socket.send(binary.buffer);
		},

		keyEvent: function(down, key, ctrl, alt, shift) {
			var buffer = new ArrayBuffer(8);
			var binary = new DataView(buffer);

			var offset = 0;

			if (key == 16 && shift) key = 21;

			binary.setUint8(offset, 0x04, this._littleEndian); offset++;                // message-type
			binary.setUint8(offset, down ? 0x01 : 0x00, this._littleEndian); offset++;  // down-flag
			binary.setUint16(offset, 0, this._littleEndian); offset += 2;              // padding
			binary.setUint32(offset, key, this._littleEndian); offset += 4;            // key

			this._socket.send(binary.buffer);
		},

		pointerEvent: function(buttonMask, x, y, delta) {
			var buffer = new ArrayBuffer(6);
			var binary = new DataView(buffer);

			var offset = 0;

			binary.setUint8(offset, 0x05, this._littleEndian); offset++;          // message-type
			binary.setUint8(offset, buttonMask, this._littleEndian); offset++;    // button-mask
			binary.setUint16(offset, x, this._littleEndian); offset += 2;         // x-position
			binary.setUint16(offset, y, this._littleEndian); offset += 2;         // y-position

			this._socket.send(binary.buffer);
		},

		clientCutText: function(text) {
			var buffer = new ArrayBuffer(0);
			var binary = new DataView(buffer);

			var offset = 0;

			binary.setUint8(offset++, 0x06);            // message-type
			binary.setUint8(offset++, 0x00);            // padding
			binary.setUint8(offset++, 0x00);            // padding
			binary.setUint8(offset++, 0x00);            // padding
			binary.setUint32(offset+=4, text.length);    // length
			//binary.setUint16(offset+=4, y);             // y-position

			this._socket.send(binary.buffer);
		}
	}

	window.MessageHandler = MessageHandler;
})(window);