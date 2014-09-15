tfsMonitor.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

	$urlRouterProvider.otherwise("/build")

	$stateProvider
		.state('home', {
	  		url: "/",
	  		templateUrl: "Views/Home/Home.html",
			abstract: true //this is abstract so that if a user navigates here, it automatically routes using the .otherwise() route
		})
		.state('home.build', {
	  		url: "build",
	  		templateUrl: "Views/BuildMonitor/BuildMonitor.html"
		 })
		.state('home.work', {
			url: "work",
			templateUrl: "Views/WorkMonitor/WorkMonitor.html"
		})
}])