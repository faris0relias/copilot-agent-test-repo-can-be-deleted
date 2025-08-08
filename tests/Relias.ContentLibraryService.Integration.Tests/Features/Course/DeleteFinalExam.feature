Feature: Delete Final Exam
 
  Authorized Users should be able to delete final exam only if they have the necessary permissions.  
 
  Rule: An authorized user can delete a final exam
    Background:
	  Given A user is authorized to access courses
    And a course exists for the organization

    @valid
    Scenario: Successfully deleting a final exam
	  Given the final exam exists
	  When the user requests to delete a final exam
	  Then the response status message should be "OK"

    @invalid
    Scenario: Trying to delete a final exam that does not exist
	  When the user requests to delete a final exam that does not exist
	  Then the response status message should be "Not Found"
    
  Rule: An unauthorized user cannot delete final exam
    Background:  
      Given an unauthorized user

    @authorization
    Scenario: User should not be able to delete a final exam without being logged in
      When the user requests to delete a final exam
      Then the response status message should be "Unauthorized"