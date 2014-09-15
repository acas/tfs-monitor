tfsMonitor.directive('tmAudio', function () {
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

tfsMonitor.controller('build-monitor-controller', ['$http', '$scope', '$window', '$timeout', '$interval',
	function ($http, $scope, $window, $timeout, $interval) {
		$scope.buildMonitor = new function () {

			var options = {
				reverse: false,
				sortByField: 'definition',
				groupByField: 'project',
				collapseOptions: false
			}

			_.extend(options, JSON.parse($window.localStorage.getItem("tfs-monitor.buildMonitor.options")))

			var connected = true //start off connected so that the user doesn't see an error before the initial connection is attempted			


			var utilities = {
				intervals : [],
				loadData: function (data) {
					//the way this function handles keeping track of the time of running builds is a mess, I really don't like it. Look for a better way to do it.
					_.each(utilities.intervals, function (interval) {
						$interval.cancel(interval)
					})
					utilities.intervals = []
					$scope.$apply(function () {																		
						_.each(data, function (build) {
							if (build.inProgress) {
								build.runningTime = new Date() - acas.utility.parser.toDate(build.startTime)
								utilities.intervals.push($interval(
									function () {
										build.runningTime += 1000
									}, 1000)
								)
							}
						})
						api.groups = utilities.group(utilities.sort(data))
						api.serverError = false
					})
				},

				sort: function (data) {
					data = _.sortBy(data, function (build) { return build[options.sortByField] })
					if (options.reverse) {
						data.reverse()
					}
					return data
				},

				group: function (data) {
					var object = _.groupBy(data, function (build) { return build[options.groupByField] })
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

				setConnectionState: function (connected, connecting, serverError) {
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
									$scope.$on('$destroy', function () {
										hub.connection.stop()
									})
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



				}
			}

			var api = {
				options: options,
				connected: connected,

				connect: function () {
					utilities.connect(false)
				},


				sortBy: function (sortBy) {
					if (sortBy === options.sortByField) {
						options.reverse = !options.reverse
					}
					options.sortByField = sortBy
					var data = _.flatten(_.map(api.groups, function (x) { return x.builds }))
					api.groups = utilities.group(utilities.sort(data))
					$window.localStorage.setItem("tfs-monitor.buildMonitor.options", angular.toJson(options))

				},

				groupBy: function (groupBy) {
					options.groupByField = groupBy
					var data = _.flatten(_.map(api.groups, function (x) { return x.builds }))
					api.groups = utilities.group(utilities.sort(data))
					$window.localStorage.setItem("tfs-monitor.buildMonitor.options", angular.toJson(options))

				},

				toggleCollapseOptions: function () {
					options.collapseOptions = !options.collapseOptions
					$window.localStorage.setItem("tfs-monitor.buildMonitor.options", angular.toJson(options))
				},


				openBuild: function (build) {
					var buildNumber = build.buildUri.split('/')[5]
					$window.open(tfsMonitor.projectCollectionUrl + '/' + build.project + '/_build#buildUri=vstfs%3A%2F%2F%2FBuild%2FBuild%2F' + buildNumber + '&_a=summary')
				}
			}

			utilities.initialize()
			return api

		}








	}
])
