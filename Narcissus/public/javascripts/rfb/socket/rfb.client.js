(function() {
	function RfbClient(options) {
		if (!options.host) throw '';

		if (!options.port) options.port = RfbClient.DEFAULT_RFB_PORT;

		this._options = $.extend({}, options);
		this._handshakingPhase = RfbClient.HandshakingPhase.notYet;
		this._clientVersion = new ProtocolVersion(3, 7);
		this._securityType = RfbClient.SecurityTypes.vncAuthentication;

		this._serverInit = {
			frameBufferWidth: 0,
			frameBufferHeight: 0,
			pixelFormat: {
				bitPerPixel: 0,
				depth: 0,
				bigEndianFlag: 0,
				isBigEndian: true,
				trueColourFlag: 0,
				redMax: 0,
				greenMax: 0,
				blueMax: 0,
				redShift: 0,
				greenShift: 0,
				blueShift: 0
			},
			name: ''
		};

		var RcInstance = this;

		var hostString = 'ws://' + options.host + ':' + options.port + '/';

		this._socket = new WebSocket(hostString);

		this._socket.binaryType = 'arraybuffer';

		this._socket.onopen = function() { RcInstance.connected(); };
		this._socket.onmessage = function(event) { RcInstance.received(event); };
		this._socket.onclose = function() { RcInstance.disconnected(); };

		this._messageHandler = new MessageHandler(this._socket);

		this._messageHandler.onFramebufferUpdate = function(x, y, width, height, rgbData) {
			RcInstance.onFramebufferUpdate(x, y, width, height, rgbData);
		};
	}

	RfbClient.DEFAULT_RFB_PORT = 5900;

	RfbClient.HandshakingPhase = {
		notYet: 0,
		versionDecided: 1,
		securityTypeEnumeration: 2,
		securityTypeDecided: 3,
		authenticated: 4,
		initialized: 5,
		handshakingCompleted: 6
	}

	RfbClient.SecurityTypes = {
		invalid: 0,
		none: 1,
		vncAuthentication: 2
	}

	RfbClient.prototype = {
		onConnected: function() {},
		onDisconnected: function() {},
		onReceived: function(event) {},

		onServerInit: function(serverInit) {},
		onFramebufferUpdate: function(x, y, rgbData) {},

		connected: function() {
			this.onConnected();
		},
		disconnected: function() {
			this.onDisconnected();
		},
		received: function(event) {
			switch (this._handshakingPhase) {
				case RfbClient.HandshakingPhase.notYet:
					var serverVersion = ProtocolVersion.convert(event.data);

					if (console) {
						console.log(serverVersion.toString());
					}

					this._handshakingPhase = RfbClient.HandshakingPhase.versionDecided;

					this._socket.send(this._clientVersion.toBytes());

					break;

				case RfbClient.HandshakingPhase.versionDecided:
					this._handshakingPhase = RfbClient.HandshakingPhase.securityTypeDecided;

					this._socket.send(new Uint8Array([this._securityType]));

					break;

				case RfbClient.HandshakingPhase.securityTypeDecided:
					var challenge = CryptoJS.enc.u8array.parse(new Uint8Array(event.data));

					var keyHex = CryptoJS.enc.Utf8.parse('12345678');

					var challengeEncrypted = CryptoJS.DES.encrypt(challenge, keyHex, {
						mode: CryptoJS.mode.ECB,
						padding: CryptoJS.pad.NoPadding
					});

					challengeEncrypted = CryptoJS.enc.u8array.stringify(challengeEncrypted.ciphertext);

					this._handshakingPhase = RfbClient.HandshakingPhase.authenticated;

					this._socket.send(challengeEncrypted);

					break;

				case RfbClient.HandshakingPhase.authenticated:
					this._handshakingPhase = RfbClient.HandshakingPhase.initialized;

					var authenticated = new DataView(event.data).getUint32(0) == 0;

					if (authenticated) {
						this._socket.send(new Uint8Array([ 0x00 ]));
					} else {
						this._socket.close();
					}

					break;

				case RfbClient.HandshakingPhase.initialized:
					this._handshakingPhase = RfbClient.HandshakingPhase.handshakingCompleted;

					this.binaryToServerInit(event.data);

					console.log(this._serverInit);

					this.onServerInit(this._serverInit);

					this._messageHandler._littleEndian = this._serverInit.pixelFormat.bigEndianFlag < 1;

					//this._messageHandler.framebufferUpdateRequest(true, 0, 0, this._serverInit.frameBufferWidth, this._serverInit.frameBufferHeight);

					break;

				case RfbClient.HandshakingPhase.handshakingCompleted:
					this._messageHandler.handle(event.data);

					break;
			}
		},

		binaryToServerInit: function(binary) {
			var byteArray = new DataView(binary);

			// Read endian flag to determine endian type.
			this._serverInit.pixelFormat.bigEndianFlag = byteArray.getUint8(6);
			this._serverInit.pixelFormat.isBigEndian = this._serverInit.pixelFormat.bigEndianFlag > 0;

			this._serverInit.frameBufferWidth = byteArray.getUint16(0, !this._serverInit.pixelFormat.isBigEndian);
			this._serverInit.frameBufferHeight = byteArray.getUint16(2, !this._serverInit.pixelFormat.isBigEndian);
			this._serverInit.pixelFormat.bitPerPixel = byteArray.getUint8(4);
			this._serverInit.pixelFormat.depth = byteArray.getUint8(5);

			this._serverInit.pixelFormat.trueColourFlag = byteArray.getUint8(7);
			this._serverInit.pixelFormat.redMax = byteArray.getUint16(8, !this._serverInit.pixelFormat.isBigEndian);
			this._serverInit.pixelFormat.greenMax = byteArray.getUint16(10, !this._serverInit.pixelFormat.isBigEndian);
			this._serverInit.pixelFormat.blueMax = byteArray.getUint16(12, !this._serverInit.pixelFormat.isBigEndian);
			this._serverInit.pixelFormat.redShift = byteArray.getUint8(14);
			this._serverInit.pixelFormat.greenShift = byteArray.getUint8(15);
			this._serverInit.pixelFormat.blueShift = byteArray.getUint8(16);

			var nameLength = byteArray.getUint32(20, !this._serverInit.pixelFormat.isBigEndian);

			this._serverInit.name = String.fromCharCode.apply(null, new Uint16Array(binary.slice(24)));
		}
	}

	window.RfbClient = RfbClient;
})(window);

(function() {
	function ProtocolVersion(major, minor) {
		this._major = major;
		this._minor = minor;
	}

	ProtocolVersion.PROTOCOL_VERSION_BINARY_LENGTH = 12;
	ProtocolVersion.PROTOCOL_VERSION_PREFIX = new Uint8Array([ 0x52, 0x46, 0x42 ]);
	ProtocolVersion.PROTOCOL_VERSION_SUFFIX = 0x0a;

	ProtocolVersion.convert = function(versionBinary) {
		if (versionBinary.byteLength != ProtocolVersion.PROTOCOL_VERSION_BINARY_LENGTH) {
			throw 'Protocol version binary length must be 12.';
		}

		var prefix = new Uint8Array(versionBinary, 0, ProtocolVersion.PROTOCOL_VERSION_PREFIX.length);

		if (prefix > ProtocolVersion.PROTOCOL_VERSION_PREFIX ||
			prefix < ProtocolVersion.PROTOCOL_VERSION_PREFIX) {
			throw 'Protocol version prefix is not matched.';
		}

		var suffix = new Uint8Array(versionBinary, 11);

		if (suffix[0] != ProtocolVersion.PROTOCOL_VERSION_SUFFIX) {
			throw 'Protocol version binary must be ended by NewLine.';
		}

		var majorBinary = new Uint8Array(versionBinary, 4, 3);
		var minorBinary = new Uint8Array(versionBinary, 8, 3);

		var major = majorBinary[0] * 100 + majorBinary[1] * 10 + majorBinary[2];
		var minor = minorBinary[0] * 100 + minorBinary[1] * 10 + minorBinary[2];

		return new ProtocolVersion(major, minor);
	}

	ProtocolVersion.compare = function(a, b) {
		if (a._major > b._major) {
			return 1;
		} else if (a._major < b._major) {
			return -1;
		} else {
			if (a._minor == b._minor) {
				return 0;
			} else {
				return a._minor > b.minor ? 1 : -1;
			}
		}
	}

	ProtocolVersion.prototype = {
		toBytes: function() {
			var binary = new Uint8Array(ProtocolVersion.PROTOCOL_VERSION_BINARY_LENGTH);

			var offset = 0;

			binary.set(ProtocolVersion.PROTOCOL_VERSION_PREFIX, offset);
			binary.set(new Uint8Array([0x20]), offset += ProtocolVersion.PROTOCOL_VERSION_PREFIX.length);
			binary.set(new Uint8Array([this._major / 100, this._major % 100 / 10, this._major % 1000]), offset += 1);
			binary.set(new Uint8Array([0x2e]), offset += 3);
			binary.set(new Uint8Array([this._minor / 100, this._minor % 100 / 10, this._minor % 1000]), offset += 1);
			binary.set(new Uint8Array([0x0a]), offset += 3);

			return binary;
		},
		toString: function() {
			var versionBinary = this.toBytes();

			versionBinary[4] += 48;
			versionBinary[5] += 48;
			versionBinary[6] += 48;
			versionBinary[8] += 48;
			versionBinary[9] += 48;
			versionBinary[10] += 48;

			return String.fromCharCode.apply(String, versionBinary);
		}
	}

	window.ProtocolVersion = ProtocolVersion;
})(window);