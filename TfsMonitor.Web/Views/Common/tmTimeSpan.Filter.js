app.filter('tmTimeSpan', function () {
	return function (value) {
		if (!value) {
			return ''
		}
		var parts = value.split(':')
		var hours = parts[0]
		var minutes = parts[1]
		var seconds = parseInt(parts[2]).toString()
		if (hours !== '00') {
			minutes = ('0' + minutes).substr(minutes.length - 1, 2)
			hours = parseInt(hours) + ':'
		}
		else {
			hours = ''
			minutes = parseInt(minutes)
		}
		return hours + minutes + ':' + ('0' + seconds).substr(seconds.length - 1, 2)
	}
})