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
					if (data.imageUrl) {
						api.backgroundImage = (tfsMonitor.isSsl ? 'https' : 'http') + '://www.bing.com' + data.imageUrl
					}
					if (data.video) {
						videos = {}
						_.each(data.video, function(v) {
							videos[v[0]] = {
								codec: v[1],
								url: v[2]
							}
						})

						if (videos.mp4hd) {
							api.backgroundVideo = videos.mp4hd.url
						}
						else if (videos.mp4) {
							api.backgroundVideo = videos.mp4.url
						}
						jQuery("#backgroundVideo").attr('src', api.backgroundVideo)
						jQuery("#backgroundVideo")[0].play()
					}
					if (data.copyright) {
						api.backgroundCopyright = data.copyright
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
			backgroundImage: null,
			backgroundVideo: null,
			backgroundCopyright: null
		}

		utilities.initialize()

		return api
	}
}])