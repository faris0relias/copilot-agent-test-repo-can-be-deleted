Feature: Delete lesson
 
Authorized Users should be able to delete a lesson only if they have the necessary permissions.  
 
Rule: An authorized user can delete a lesson
Background:
	Given the user is authorized to delete lessons

@valid
Scenario: Successfully deleting a lesson
	When the user requests to delete a lesson
  	Then the request should be successful
  	And the lesson should be removed from the course

@invalid
Scenario: Trying to delete a lesson without a valid lesson ID
	When the user requests to delete a lesson without a valid lesson ID
	Then the request should return error
	And the error message should contain "not found in section"

@invalid
Scenario: Trying to delete a lesson without a course ID
	When the user requests to delete a lesson without a valid course ID
	Then the request should return error
	And the error message should contain "Learning content for course"

@invalid
Scenario: Trying to delete a lesson that does not exist with a valid course ID
	When the user requests to delete a lesson that does not exist with a valid course ID
	Then the error message should contain "not found in section"

@invalid
Scenario: Trying to delete a lesson that does not exist with a valid section ID
	When the user requests to delete a lesson that does not exist with a section ID
	Then the error message should contain "not found for course"

Rule: An unauthorized user cannot delete a lesson and its details

@authorization
Scenario: Trying to delete a lesson without being logged in
	Given the user is not authenticated
	When the unauthenticated user requests to delete a lesson
	Then the response status message should be "Unauthorized"