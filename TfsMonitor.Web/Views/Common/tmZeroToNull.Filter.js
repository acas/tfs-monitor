//converts zeros (and undefined) to null - useful if we're going to display numbers on the screen but we don't want zeros to show
tfsMonitor.filter('tmZeroToNull', function () {
	return function (value) {
		if (!value) {
			return null
		}
		return value
	}
})