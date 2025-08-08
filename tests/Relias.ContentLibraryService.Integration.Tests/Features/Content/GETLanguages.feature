Feature: GETLanguages
As an Admin user I want to get the list of languages so that I can see what content types are available


Scenario: An authorized user getting the list of languages is succesful with a response message of "OK" and returns a list of objects with values LanguageId Code and Name 
	Given An authorized user with access to content library service
	And   A list of languages
	When  The user makes a request to get a list of Languages
	Then  The response status message should be "OK"
	And   A list of Language objects should be returned with values LanguageId Code and Name
   
Scenario: An unauthorized user getting a list of languages returns "Unauthorized"
    Given An unauthorized user
	And   A list of languages
	When  The user makes a request to get a list of Languages
    Then  The response status message should be "Unauthorized"