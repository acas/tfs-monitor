tfsMonitor.controller('work-monitor-controller', ['$http', '$scope', '$window', '$timeout', '$interval', 'tmMonitor',
	function ($http, $scope, $window, $timeout, $interval, tmMonitor) {
		$scope.workMonitor = new function () {
			var utilities = {
				loadData: function (data) {
					$scope.$apply(function () {
						var grouped = _.groupBy(_.sortBy(data, 'state'), 'assignee')
						var processed = []
						for (var group in grouped) {
							processed.push({ name: group, data: grouped[group]})
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

				monitor: tmMonitor('workMonitorHub'),
				connect: function () {
					this.monitor.connect()
				}
			}

			var api = {
				groupCollapse: {},
				serverError: utilities.monitor.serverError,
				connecting: utilities.monitor.connecting,
				connected: utilities.monitor.connected,

				connect: function () {
					utilities.connect(false)
				},



				openWorkItem: function (workItem) {					
					$window.open(tfsMonitor.projectCollectionUrl + '/' + workItem.project + '/_workitems#id=' + workItem.workItemID + '&_a=edit' )
				}
			}

			utilities.initialize()
			return api
		}
	}
])
