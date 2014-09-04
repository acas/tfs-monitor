app.directive('tmAudio', function () {
	return {
		link: function (scope, element, attributes) {
			var parts = attributes.tmAudio.split('.')
			for (var i = 0; i < parts.length - 1; i++) {
				scope = scope[parts[i]]
			}
			scope[parts[i]] = function () {
				element[0].play()
			}
		}
	}
})


app.controller('BuildMonitorCtrl', ['$http', '$scope', '$window', '$timeout',
	function ($http, $scope, $window, $timeout) {
		$scope.buildMonitor = new function () {

			var reverse = false
			var sortByField = 'definition'
			var groupByField = 'project'
			var connected = true //start off connected so that the user doesn't see an error before the initial connection is attempted			

			var utilities = {
				loadData: function (data) {
					$scope.$apply(function () {
						api.groups = utilities.group(utilities.sort(data))
						api.serverError = false
					})
				},

				sort: function (data) {
					data = _.sortBy(data, function (build) { return build[sortByField] })
					if (reverse) {
						data.reverse()
					}
					return data
				},

				group: function (data) {
					var object = _.groupBy(data, function (build) { return build[groupByField] })
					var result = []
					for (var prop in object) {
						result.push({ name: prop, builds: object[prop] })
					}
					result = _.sortBy(result, 'name')
					return result
				},

				initialize: function () {
					utilities.connect(true)
				},

				setConnectionState: function(connected, connecting, serverError){
					$timeout(function () {
						$scope.$apply(function () {
							api.serverError = serverError
							api.connecting = connecting
							api.connected = connected
						})
					})
				},

				connect: function (firstTry) {
					jQuery(function () {


						var hub = jQuery.connection.buildMonitorHub
						hub.client.sendData = function (data) {
							utilities.loadData(data)
						}

						hub.client.notifyError = function () {
							//if we're getting a server error, we must be connected
							utilities.setConnectionState(true, false, true)
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
								.fail(function () {
									//set connected = false
									//clear server error, we don't even have a connection so server error is irrelevant now
									utilities.setConnectionState(false, false, false)
								})

						}
						catch (ex) {

						}
						jQuery.connection.hub.disconnected(function () {
							utilities.setConnectionState(false, false, false)
						})
					})



				}
			}

			var api = {


				reverse: reverse,
				sortByField: sortByField,
				groupByField: groupByField,
				connected: connected,

				connect: function () {
					utilities.connect(false)
				},


				sortBy: function (sortBy) {
					if (sortBy === sortByField) {
						reverse = !reverse
					}
					sortByField = sortBy
					var data = _.flatten(_.map(api.groups, function (x) { return x.builds }))
					api.groups = utilities.group(utilities.sort(data))
				},

				groupBy: function (groupBy) {
					groupByField = groupBy
					var data = _.flatten(_.map(api.groups, function (x) { return x.builds }))
					api.groups = utilities.group(utilities.sort(data))
				},




				openBuild: function (build) {
					var buildNumber = build.buildUri.split('/')[5]
					$window.open('https://tfs.americancapital.com/tfs/ACASProjects/' + build.project + '/_build#buildUri=vstfs%3A%2F%2F%2FBuild%2FBuild%2F' + buildNumber + '&_a=summary')
				}
			}

			utilities.initialize()
			return api

		}








	}
])
