tfsMonitor.controller('work-monitor-controller', ['$http', '$scope', '$window', '$timeout', '$interval', 'tmMonitor',
	function ($http, $scope, $window, $timeout, $interval, tmMonitor) {
		$scope.workMonitor = new function () {
			var utilities = {
				loadData: function (data) {
					$scope.$apply(function () {
						api.workItems = data						
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

				serverError: utilities.monitor.serverError,
				connecting: utilities.monitor.connecting,
				connected: utilities.monitor.connected,

				connect: function () {
					utilities.connect(false)
				}
			}

			utilities.initialize()
			return api
		}
	}
])
