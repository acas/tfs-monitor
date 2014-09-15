tfsMonitor.filter('tmTimeSpan', function () {
	return function (value) {
		if (!value) {
			return ''
		}
		if (parseInt(value) == value) { //it's a number of milliseconds
			milliseconds = value
			var seconds = milliseconds / 1000
			var sec = seconds % 60
			var min = (seconds - sec) / 60
			//hours are not supported yet here
			return Math.floor(min) + ':' + acas.utility.formatting.padZero(Math.floor(sec), 2)
		}
		else { //it's a timespan coming form .NET
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
	}
})