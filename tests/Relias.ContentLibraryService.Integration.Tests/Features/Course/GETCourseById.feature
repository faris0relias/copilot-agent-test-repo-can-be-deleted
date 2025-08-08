Feature: Get Courses by ID

  I want to retrieve a course and their details using a course ID

  Rule: An authorized user can access a course and its details
    Background: 
      Given A user is authorized to access courses
      And the user has access to a course

    @valid
    Scenario: Successfully retrieving a course by ID
      When the user access an existing course 
      Then The response status message should be "OK"
      And a course is returned

    @invalid
    Scenario: Trying to retrieve course that does not exist
      When the user access a course that does not exist
      Then The response status message should be "Not Found"
      And no course should be returned
      
    @invalid
    Scenario: Trying to retrieve courses with an invalid ID
      When the user access an invalid course ID 
      Then The response status message should be "Bad Request"
 
    @authorization
    Scenario: Preventing users from retrieving courses for another organization
      When the user requests a course from an organization they do not belong to  
      Then The response status message should be "Forbidden"

  Rule: An unauthorized user cannot access a course and its details
    Background:
      Given the user is not authenticated

    @authorization
    Scenario: Trying to retrieve a course without being logged in
      When the unauthenticated user requests to access an existing course
      Then The response status message should be "Unauthorized"