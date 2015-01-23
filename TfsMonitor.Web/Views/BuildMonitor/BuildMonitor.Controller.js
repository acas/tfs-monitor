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

tfsMonitor.controller('build-monitor-controller', ['$http', '$scope', '$window', '$timeout', '$interval', 'tmMonitor',
	function ($http, $scope, $window, $timeout, $interval, tmMonitor) {
		$scope.buildMonitor = new function () {

			var options = {
				reverse: false,
				sortByField: 'definition',
				groupByField: 'project',
				collapseOptions: false
			}
			
			_.extend(options, JSON.parse($window.localStorage.getItem("tfs-monitor.buildMonitor.options")))
			
			var utilities = {
				intervals: [],
				loadData: function (data) {
					//the way this function handles keeping track of the time of running builds is a mess, I really don't like it. Look for a better way to do it.
					_.each(utilities.intervals, function (interval) {
						$interval.cancel(interval)
					})
					utilities.intervals = []
					$scope.$apply(function () {
						utilities.playSounds(data)
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
						utilities.playSounds(data)
					})
				},
				previousData: null,

				playSounds: function (data) {
					var playStartSound = function () {
						//api.startSound()
					}
					var playSucceededSound = function () {
						//api.succeededSound()
					}
					var playFailedSound = function () {
						api.failedSound()
					}
					var previousData = utilities.previousData
					if (previousData && data.length === previousData.length) {
						for (var i in data) { //the server must send back the build definitions in the same order each time. 
							//this will break down if there's a new build definition in data but the length of the two arrays are the same, but that's rare so whatever
							if (data[i].inProgress && !previousData[i].inProgress) {
								playStartSound()
							} else if (!data[i].inProgress && previousData[i].inProgress) {
								if (data[i].succeeded) {
									playSucceededSound()
								}
								else if (data[i].failed) {
									playFailedSound()
								}

							}
						}
					}
					utilities.previousData = data
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
						result.push({
							name: prop, builds: object[prop],
							failureCount: _.filter(object[prop], function (x) { return x.status === 'Failed' }).length,
							inProgressCount: _.filter(object[prop], function (x) { return x.status === 'InProgress' }).length							
						})
					}
					result = _.sortBy(result, 'name')
					return result
				},

				initialize: function () {
					utilities.monitor.setLoadDataFn(utilities.loadData)
					utilities.connect(true)
					$scope.$on('$destroy', function () {
						utilities.monitor.stop()
					})
					if (options.autoScroll) {
						api.startAutoScroll()
					}
				},
			
				monitor: tmMonitor('buildMonitorHub', $scope),
				connect: function (firstTry) {
					this.monitor.connect(firstTry)
				}
			}

			var api = {
				options: options,
				
				serverError: function () { return utilities.monitor.serverError },
				connecting: function () { return utilities.monitor.connecting },
				connected: function () { return utilities.monitor.connected },

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
				},
				selectGroup: function (name) {
					this.selectedGroup = name					
				},
				startAutoScroll: function () {
					options.autoScroll = true
					$window.localStorage.setItem("tfs-monitor.buildMonitor.options", angular.toJson(options))
					var i = 0
					var scroll = function () {
						var groups = _.pluck(api.groups, 'name')
						var group = groups[i % groups.length]
						api.selectGroup(group)
						i++
					}
					//start the scroll on a multiple of six seconds. The objective is to sync up multiple 
					//browser instances running monitors to do the scroll at roughly the same time
					var now = new Date()
					var milliseconds = now.getSeconds() * 1000 + now.getMilliseconds()
					var wait = 6000 - (milliseconds % 6000)
					$timeout(function () {
						scroll() //initial scroll
						//then set up the interval every six seconds
						utilities.autoScrollInteval = $interval(scroll, 6000)
					}, wait)
				},

				stopAutoScroll: function () {
					options.autoScroll = false
					$window.localStorage.setItem("tfs-monitor.buildMonitor.options", angular.toJson(options))
					$interval.cancel(utilities.autoScrollInteval)
				}
			}

			utilities.initialize()
			return api

		}








	}
])
