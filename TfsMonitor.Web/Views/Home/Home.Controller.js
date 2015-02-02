tfsMonitor.controller('home-controller', ['$scope', '$state', '$http', function ($scope, $state, $http) {
	$scope.home = new function () {
		var utilities = {
			loadBingBackgrounds: function () {
				$http({
					method: 'GET',
					//the n= parameter can be changed to include images from prior days that could be rotated (maybe in the future)
					url: 'api/BingBackgrounds'
				})
				.success(function (data) {
					if (data.images && data.images.length) {
						api.backgroundImage = (tfsMonitor.isSsl ? 'https' : 'http') + '://www.bing.com' + data.images[0].url
					}
				})
			},
			initialize: function () {
				if (tfsMonitor.bingBackgrounds != null && Boolean(tfsMonitor.bingBackgrounds)) {
					utilities.loadBingBackgrounds()
				}
			}
		}

		var api = {
			backgroundImage: null
		}

		utilities.initialize()

		return api
	}
}])