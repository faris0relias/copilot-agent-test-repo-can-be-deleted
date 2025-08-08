Feature: Get Final Exam by Course ID

  I want to  retrieve final exam and their details by course ID

  Rule: An authorized user can access a final exam and its details
    Background: 
      Given A user is authorized to access courses
      And a course exists for the organization

    @valid
    Scenario: Successfully retrieving a final exam by Course ID
      Given the final exam exists
      When the user accesses an existing final exam 
      Then the response status message should be "OK"
      And a final exam is returned

    @invalid
    Scenario: Trying to retrieve a final exam that does not exist
      When the user accesses a final exam that doesn't exist
      Then the response status message should be "No Content"

  Rule: An unauthorized user cannot access a final exam and its details
    Background:
      Given the user is not authenticated
      And the final exam exists

    @authorization
    Scenario: Trying to retrieve a final exam without being logged in
      When the user accesses an existing final exam
      Then The response status message should be "Unauthorized"