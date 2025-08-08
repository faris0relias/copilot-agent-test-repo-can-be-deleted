Feature: Retrieve Courses 
  As a user with access to the content library service,
  I want to retrieve a list of courses that belong to that Org

  Rule: An authorized user can access courses
    Background:
      Given  A user is authorized to access courses
    
    @valid
    Scenario: Successfully retrieving a list of courses
    When the user requests the list of available courses
    Then The response status message should be "OK"
    And A list of courses with its attributes should be returned

    @valid
    Scenario: No courses available
    When the user requests the list of available courses for an organization with no course data
    Then The response status message should be "OK"
    And An empty list is returned

    @authorization
    Scenario: Preventing users from viewing courses of another organization  
    When the user requests the list of available courses from a different organization
    Then  The response status message should be "Forbidden"

  Rule: An unauthorized user cannot access courses
    Background:
      Given the user is not authenticated

    @authorization
    Scenario: Trying to retrieve courses without being logged in
    When the unauthenticated user requests the list of available courses
    Then The response status message should be "Unauthorized"

