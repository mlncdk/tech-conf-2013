These dll files adds two things to Dynamicweb

1. A new notification, Notifications.Standard.Application.AuthenticateRequest, sent on Application_AuthenticateRequest
2. Overriding ItemPublisher "Select items under this page" setting from url by adding

		ItemParentId=«some page id»

	 to the url
