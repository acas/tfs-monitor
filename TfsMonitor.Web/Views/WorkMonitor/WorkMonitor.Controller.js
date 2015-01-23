tfsMonitor.controller('work-monitor-controller', ['$http', '$scope', '$window', '$timeout', '$interval', 'tmMonitor',
	function ($http, $scope, $window, $timeout, $interval, tmMonitor) {
		var options = {
			reverse: false,
			groupByField: 'project',
			sortByField: 'state',
			collapseOptions: false,
			autoScroll: false
		}

		$scope.workMonitor = new function () {
			var utilities = {
				autoScrollInterval: null,

				loadData: function (data) {
					$scope.$apply(function () {
						var grouped = utilities.group(utilities.sort(data))
						var processed = utilities.process(grouped)
						api.groups = processed
					})
				},

				initialize: function () {
					_.extend(options, JSON.parse($window.localStorage.getItem("tfs-monitor.workMonitor.options")))
					utilities.monitor.setLoadDataFn(utilities.loadData)
					utilities.connect(true)
					$scope.$on('$destroy', function () {
						utilities.monitor.stop()
					})
					if (options.autoScroll) {
						api.startAutoScroll()
					}
				},


				sort: function (data) {
					data = _.sortBy(data, function (item) { return item[options.sortByField] })
					if (options.reverse) {
						data.reverse()
					}
					return data
				},

				group: function (data) {
					var object = _.groupBy(data, function (item) { return item[options.groupByField] })
					var result = []
					for (var prop in object) {
						var name = prop
						if (options.groupByField === 'assignee' & !prop) {
							name = 'Unassigned'
						}
						result.push({ name: name, group: object[prop] })
					}
					result = _.sortBy(result, 'name')
					return result
				},

				process: function (data) {
					var processed = []
					for (var group in data) {
						processed.push({
							name: data[group].name,
							data: data[group].group,
							count: data[group].group.length,
							workRemaining: _.reduce(data[group].group, function (memo, item) { return memo += (item.workRemaining.development || 0) + (item.workRemaining.testing || 0) }, 0)
						})
					}
					return processed
				},

				monitor: tmMonitor('workMonitorHub', $scope),
				connect: function (firstTry) {
					this.monitor.connect(firstTry)
				}
			}

			var api = {
				options: options,
				groupCollapse: {},

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
					var data = _.flatten(_.map(api.groups, function (x) { return x.data }))
					api.groups = utilities.process(utilities.group(utilities.sort(data)))
					$window.localStorage.setItem("tfs-monitor.workMonitor.options", angular.toJson(options))

				},

				groupBy: function (groupBy) {
					options.groupByField = groupBy
					var data = _.flatten(_.map(api.groups, function (x) { return x.data }))
					api.groups = utilities.process(utilities.group(utilities.sort(data)))
					$window.localStorage.setItem("tfs-monitor.workMonitor.options", angular.toJson(options))

				},

				toggleCollapseOptions: function () {
					options.collapseOptions = !options.collapseOptions
					$window.localStorage.setItem("tfs-monitor.workMonitor.options", angular.toJson(options))
				},

				selectGroup: function (name) {
					this.selectedGroup = name
					this.groupCollapse[name] = false
				},

				openWorkItem: function (workItem) {
					$window.open(tfsMonitor.projectCollectionUrl + '/' + workItem.project + '/_workitems#id=' + workItem.workItemID + '&_a=edit')
				},

				startAutoScroll: function () {
					options.autoScroll = true
					$window.localStorage.setItem("tfs-monitor.workMonitor.options", angular.toJson(options))
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
					$window.localStorage.setItem("tfs-monitor.workMonitor.options", angular.toJson(options))
					$interval.cancel(utilities.autoScrollInteval)
				}
			}

			utilities.initialize()
			return api
		}
	}
])
