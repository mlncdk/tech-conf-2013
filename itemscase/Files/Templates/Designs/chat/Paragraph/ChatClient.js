var ChatClient = function($scope) {
	var debug = function() {
		if ((typeof(console) != 'undefined') && (typeof(console.debug) == 'function')) {
			// ;;; console.debug.apply(console, arguments);
		}
	}

	var update = function(f, apply) {
		if (apply) {
			$scope.$apply(f);
		} else {
			f();
		}
	},

	getSession = function(sessionId) {
		var i, session;
		for (i = 0; session = $scope.sessions[i]; i++) {
			if (session.id == sessionId) {
				return session;
			}
		}
	},

	setSessionActive = function(session, active) {
		session.isActive = !!active;
		$scope.numberOfActiveSessions = 0;
		for (i = 0; session = $scope.sessions[i]; i++) {
			if (session.isActive) {
				$scope.numberOfActiveSessions++;
			}
		}
	}

	startSession = function(values, apply) {
		;;; debug('startSession', values);

		var sessionId = values.sessionId ? values.sessionId : null,
		session = getSession(sessionId);

		if (session) {
			update(function() {
				if (values.name) {
					session.name = values.name;
				}
				setSessionActive(session, true);
			}, apply);
		} else {
			update(function() {
				session = {
					id: sessionId || 'session-'+$scope.sessions.length,
					name: values.name || sessionId,
					messages: [],
					newMessageText: ''
				};
				$scope.sessions.push(session);
				setSessionActive(session, true);
			}, apply);
		}
		return session;
	},

	endSession = function(sessionId, apply) {
		var session = getSession(sessionId);
		;;; debug('endSession', sessionId, session);
		if (session) {
			update(function() {
				setSessionActive(session, false);
			}, apply);
		}
	},

	deleteSession = function(sessionId, apply) {
		var i, session;
		for (i = 0; session = $scope.sessions[i]; i++) {
			if (session.id == sessionId) {
				update(function() {
					;;; debug('deleteSession', i);
					;;; debug($scope.sessions);
					$scope.sessions.splice(i, 1);
					$scope.sessions = $scope.sessions;
					;;; debug($scope.sessions);
				}, apply);
			}
		}
	},

	addMessage = function(sessionId, sender, messageText, apply) {
		;;; debug('addMessage', arguments);
		var session = getSession(sessionId);
		;;; debug('addMessage', session);
		if (!session) {
			session = startSession({ sessionId: sessionId, name: sessionId }, apply);
		}
		if (session) {
			update(function() {
				var i, message;
				session.messages.push({
 					created_at: new Date(),
 					sender: sender,
 					message: messageText,
					isUnread: true,
					session: session
				});
				session.numberOfUnreadMessages = 0;
				for (i = 0; message = session.messages[i]; i++) {
					if (message.isUnread) {
						session.numberOfUnreadMessages++;
					}
				}
				setSessionActive(session, true);
			}, apply);
		}
	},

	saveHistory = function (names) {
		setTimeout(function () {
			$.get('/Default.aspx?Chat.EndSessions='+(names ? encodeURIComponent(names.join()) : ''));
		}, 50);
	},

	reconnectSession = function(f) {
		$.connection.hub.stop();
		$.connection.hub.start().done(f ? f : function() {});
	},

	setSessionName = function(name) {
		if (name) {
			hub.state.name = Chat.isManager ? 'Manager' : name;
			hub.state.manager = !!Chat.isManager;
			$.connection.hub.qs = 'manager='+hub.state.manager+'&name='+encodeURIComponent(hub.state.name);
			reconnectSession();
		}
	};

	$scope.readMessage = function(message) {
		var session = message.session;
		message.isUnread = false;
		session.numberOfUnreadMessages = 0;
		for (i = 0; message = session.messages[i]; i++) {
			if (message.isUnread) {
				session.numberOfUnreadMessages++;
			}
		}
	}

	$scope.newMessage = function(session) {
		;;; debug('newMessage', session.id, session.newMessageText);
		addMessage(session.id, $scope.isManager ? 'manager' : 'client', session.newMessageText);
		hub.server.sendMessage(session.id, session.newMessageText);
		session.newMessageText = '';

	}

	$scope.startChat = function() {
		$scope.name = $scope.username;
		startSession({ sessionId: $scope.name, name: $scope.name });
	}

	$scope.$watch('name', function(value) {
		if (value) {
			setSessionName(value);
		}
	});

	var hub = $.connection.chatHub;

	hub.state.name = Chat.isManager ? 'Manager' : '';
	hub.state.manager = !!Chat.isManager;
	$.connection.hub.qs = 'manager='+hub.state.manager+'&name='+encodeURIComponent(hub.state.name);

	hub.client.onMessageReceived = function(name, message) {
		;;; debug('onMessageReceived', arguments, name ? name : $scope.name, message);

		addMessage(name ? name : $scope.name, !name ? 'manager' : 'client', message, true);
	}

	hub.client.onManagerConnected = function() {
		;;; debug('onManagerConnected', arguments);

		update(function() {
			$scope.managerIsConnected = true;
		}, true);

		if (!$scope.isManager) {
			// Resend all messages to manager
			if (false)
			reconnectSession(function() {
				var i, message,
				session = getSession($scope.name);
				if (session) {
					for (i = 0; message = session.messages[i]; i++) {
						hub.server.sendMessage(session.id, message.message);
					}
				}
			});
		}
	}

	hub.client.onManagerDisconnected = function() {
		;;; debug('onManagerDisconnected', arguments);

		saveHistory();

		update(function() {
			$scope.managerIsConnected = false;
		}, true);
	}

	hub.client.onClientConnected = function(name) {
		;;; debug('onClientConnected', arguments);

		session = startSession({ sessionId: name, name: name }, true);
	}

	hub.client.onClientDisconnected = function(name) {
		;;; debug('onClientDisconnected', arguments);

		saveHistory([ name ]);
		endSession(name, true);
	}

	$.connection.hub.start().done(function() {
		;;; debug('start done');
		update(function() {
			if (Chat && Chat.isManager) {
				$scope.isManager = true;
				$scope.name = 'manager';
			}

			/*
			sessions = [ {
				id: '...',
				name: '...',
				isActive: true|false,
				messages: [ {
 					created_at: new Date(),
 					sender: sender,
 					message: message,
					isUnread: true
				},
				... ],
				newMessageText: ''
			},
			... ]
			*/
			$scope.sessions = [];
			$scope.numberOfActiveSessions = 0;
		}, true);
 	});
};
