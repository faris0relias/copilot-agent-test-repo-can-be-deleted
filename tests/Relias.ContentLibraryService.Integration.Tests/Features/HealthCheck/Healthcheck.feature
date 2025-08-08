Feature: Healthcheck Returns Successful Response

Scenario: Calling the Healthcheck returns a 200 OK response
	When I check the content library service api is running
	Then The response status code should be 200
	And The response status message should be "OK"