tfsMonitor.controller('work-monitor-controller', ['$http', '$scope', '$window', '$timeout', '$interval', 'tmMonitor',
	function ($http, $scope, $window, $timeout, $interval, tmMonitor) {
		$scope.workMonitor = new function () {
			var utilities = {
				loadData: function (data) {
					$scope.$apply(function () {
						var grouped = _.groupBy(_.sortBy(data, 'state'), 'project')
						var processed = []

						for (var group in grouped) {
							processed.push({
								name: group,
								data: grouped[group],
								count: grouped[group].length,
								workRemaining: _.reduce(grouped[group], function (memo, item) { return memo += (item.workRemaining.development || 0) + (item.workRemaining.testing || 0) }, 0)
							})
						}
						api.groups = processed
					})
				},

				initialize: function () {
					utilities.monitor.setLoadDataFn(utilities.loadData)
					utilities.connect(true)
					$scope.$on('$destroy', function () {
						utilities.monitor.stop()
					})
				},

				monitor: tmMonitor('workMonitorHub', $scope),
				connect: function (firstTry) {
					this.monitor.connect(firstTry)
				}
			}

			var api = {
				settings: {groupByName: 'Projects'},
				groupCollapse: {},
				serverError: function () { return utilities.monitor.serverError },
				connecting: function () { return utilities.monitor.connecting },
				connected: function () { return utilities.monitor.connected },				

				connect: function () {
					utilities.connect(false)
				},



				openWorkItem: function (workItem) {
					$window.open(tfsMonitor.projectCollectionUrl + '/' + workItem.project + '/_workitems#id=' + workItem.workItemID + '&_a=edit')
				}
			}

			utilities.initialize()
			return api
		}
	}
])
