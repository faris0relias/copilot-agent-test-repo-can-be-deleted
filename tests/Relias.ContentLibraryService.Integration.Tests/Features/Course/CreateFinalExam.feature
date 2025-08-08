Feature: Create Final Exam by Course ID

  I want to create a final exam and its details by course ID

  Rule: An authorized user can create a final exam
    Background: 
      Given A user is authorized to access courses
      And a course exists for the organization

    @valid
    Scenario: User should be able to successfully create a final exam by course ID
      Given the final exam does not exist
      When the user creates a final exam by course ID
      Then the response status code should be 201
      And a final exam is returned

    @invalid
    Scenario: User should not be able to create a final exam with an existing course ID that already has a final exam
      Given the final exam exists
      When the user creates a final exam by course ID
      Then the response status message should be "Bad Request"

  Rule: An unauthorized user cannot access a final exam and its details
    Background:
      Given the user is not authenticated

    @authorization
    Scenario: User should not be able to create a final exam without being logged in
      When the user creates a final exam by course ID
      Then the response status message should be "Unauthorized"