Feature: Edit Final Exam by Course ID

  I want to edit an existing final exam by course ID 

  Rule: An authorized user can edit a course's final exam
    Background:  
      Given A user is authorized to access courses
      And a course exists for the organization

    @authorization
    Scenario: Trying to edit a course's final exam without required permissions
      When the user does not have the required permissions
      Then the response status message should be "Forbidden"

    @valid  
    Scenario: Successfully updating a course with final exam details 
      Given the final exam exists
      When provided input has valid values
      And the user makes a request to update a final exam
      Then the response status message should be "OK"
      And the final exam should be updated

    @invalid  
    Scenario: Unsuccessfully updating a course without a final exam
      Given the final exam does not exist
      When the user makes a request to update a final exam
      Then the response status message should be "Bad Request"

  Rule: An unauthorized user cannot edit course's final exam
    Background:  
      Given an unauthorized user

    @authorization
    Scenario: User should not be able to edit a final exam without being logged in
      When the user makes a request to update a final exam
      Then the response status message should be "Unauthorized"