Feature: Edit Course
    
  I want to edit an existing course  
  Rule: An authorized user can edit courses  
    Background:  
      Given the user is authorized to update courses  

    @valid  
    Scenario: Successfully editing a course  
      When the user provides valid updated course details  
      Then the response status message should be "OK"  
      And the course should be updated with the new values  

#    @invalid  
#    Scenario: Trying to edit a course without a name which is a required field  
#      When the user clears the course name  
#      Then the response status message should be "Bad Request"  
#      And the validation message should be "Name for the course is required."  
#
    @invalid  
    Scenario: Trying to edit a course without a content code which is a required field  
      When the user clears the content code  
      Then the response status message should be "Bad Request"  
      And  the validation message should be "Content Code is required."  
#    
#    @invalid  
#    Scenario: Trying to edit a course with a name that exceeds 500 characters  
#      When the user provides a course name longer than 500 characters  
#      Then the response status message should be "Bad Request"  
#      And the validation message should be "Name for the course cannot exceed 500 characters in length."  
#
    @invalid  
    Scenario: Trying to edit a course with a content code that exceeds 100 characters  
      When the user provides a content code longer than 100 characters  
      Then the response status message should be "Bad Request"  
      And the validation message should be "Content code can not exceed 100 characters in length."  

    @invalid
    Scenario: Trying to edit a course with a content code that has special characters  
      When the user updates the content code with special characters  
      Then the response status message should be "Bad Request"  
      And the validation message should be "Content code can only contain numbers, letters, and/or dashes."  

    @invalid  
    Scenario: Trying to edit a course with a content code that starts with special characters  
      When the user updates the content code to start with special characters  
      Then the response status message should be "Bad Request"  
      And the validation message should be "Content code should start with a number or letter."  

    @invalid  
    Scenario: Trying to edit a course to use a duplicate content code    
      When the user tries to update the current course with the same content code  
      Then the response status message should be "Bad Request"  
      And the error message should be "Value 'C101' is invalid. Reason: The content code is already in use."
      
    @invalid  
    Scenario: Trying to edit a Relias-owned course
      When the user attempts to update a Relias owned course  
      Then the response status message should be "Bad Request"

    @authorization  
    Scenario: Trying to edit a course without required permissions  
      When the user does not have the required permissions to edit  
      Then the response status message should be "Forbidden"  
  Rule: An unauthorized user cannot edit courses  
    Background:  
      Given an unauthorized user  

    @authorization  
    Scenario: Trying to edit a course without being logged in  
      When the user is not authenticated to update 
      Then the response status message should be "Unauthorized"
