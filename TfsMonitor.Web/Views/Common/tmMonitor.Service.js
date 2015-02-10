tfsMonitor.factory('tmMonitor', ['$timeout', function ($timeout) {
	return function (hubName, scope, monitorFunctions) {
		return new function () {
			
			var utilities = {
				monitorFunctions: monitorFunctions,
				scope: scope,
				setConnectionState: function (connected, connecting, serverError) {
					$timeout(function () {
						scope.$apply(function(){
							api.serverError = serverError
							api.connecting = connecting
							api.connected = connected						
						})
					})
				}
			}

			var api = {
				connect: function (firstTry) {
					jQuery(function () {
						var hub = jQuery.connection[hubName]
						utilities.hub = hub
						for (var func in utilities.monitorFunctions) {
							hub.client[func] = utilities.monitorFunctions[func]
						}
						
						hub.client.notifyError = function (ex) {
							//if we're getting a server error, we must be connected
							utilities.setConnectionState(true, false, true)
							console.error(ex)
						}

						try {
							if (!firstTry) {
								//set it to connecting, leave server error false until it fails again after a successful connection
								utilities.setConnectionState(false, true, false)
							}
							jQuery.connection.hub
								.start()
								.done(function () {
									//clear server error, set it to connected. if the server error occurs it'll come back
									utilities.setConnectionState(true, false, false)									
								})
								.fail(function (ex) {
									//set connected = false
									//clear server error, we don't even have a connection so server error is irrelevant now
									utilities.setConnectionState(false, false, false)
									console.error(ex)
								})

						}
						catch (ex) {
							console.error(ex)
						}
						jQuery.connection.hub.disconnected(function () {
							utilities.setConnectionState(false, false, false)
						})
					})
				},
				stop: function () {
					utilities.hub.connection.stop()											
				},

				serverError: false,
				connecting: false,
				connected: true, //start off connected so that the user doesn't see an error before the initial connection is attempted	
				setMonitorFunctions: function(monitorFunctions){
					utilities.monitorFunctions = monitorFunctions
				}
			}
			return api
		}
		



	}

}])