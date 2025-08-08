Feature: Delete course
 
Authorized Users should be able to delete a course only if they have the necessary permissions.  
 
Rule: An authorized user can delete a course
Background:
	Given the user is authorized to delete courses

@valid
Scenario: Successfully deleting a course
	When the user requests to delete a course
	Then the course is deleted
	And the response status message should be "No Content"

@invalid
Scenario: Trying to delete a course without a valid Course ID
	When the user requests to delete a course without a valid course ID
	Then the response status message should be "Bad Request"
	And the validation message should be "The value '0' is not valid."
      
@invalid
Scenario: Trying to delete a course that is not in draft status
	When the user requests to delete a course that is not in draft status
	Then the response status message should be "Bad Request"
	And the validation message should be "Only courses in draft status can be deleted."

@invalid
Scenario: Trying to delete a course that does not exist
	When the user requests to delete a course that does not exist
	Then the response status message should be "Not Found"
	
@invalid
Scenario: Trying to delete a course that is owned by Relias
	When the user requests to delete a Relias owned course
	Then the response status message should be "Bad Request"

Rule: A user cannot delete a course that does not belong to their organization
@authorization
Scenario: Trying to delete a course from a different organization
	Given the user is authorized to delete courses
	When the user requests to delete a course from a different organization
	Then the response status message should be "Forbidden"

Rule: An unauthorized user cannot delete a course and its details

@authorization
Scenario: Trying to delete a course without being logged in
	Given the user is not authenticated
	When the unauthenticated user requests to delete a course
	Then the response status message should be "Unauthorized"