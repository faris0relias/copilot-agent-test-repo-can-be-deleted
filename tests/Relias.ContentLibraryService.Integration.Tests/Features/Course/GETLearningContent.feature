Feature: Get Learning Content for a Course
  I want to retrieve the learning content and its details for an existing Course 

  Rule: An authorized user can access learning content and its details
    Background:
      Given A user is authorized to access courses
      And the user has access to a course with learning content

    @valid
    Scenario: Successfully retrieving the learning content for a Course
      When the user accesses the course 
      Then the user should see the learning content details
  
    @invalid
    Scenario: Trying to retrieve learning content for an invalid Course 
      When the user tries to access an invalid course
      Then the user should be informed that the request was invalid 
      And the user should not see any learning content

    @invalid
    Scenario: Trying to retrieve learning content for a course that does not exist
      When the user tries to access a course that does not exist
      Then the user should be informed that the course was not found
      And the user should not see any learning content

    @authorization
    Scenario: Preventing users from retrieving learning content for another organization
      When the user tries to access a course from another organization  
      Then the user should not be allowed to view the course or its learning content

  Rule: An unauthorized user cannot access courses and their learning content
    Background:
      Given the user is not authorized

    @authorization
    Scenario: Trying to retrieve learning content without access to Courses
      When the user attempts to access a course
      Then the user should be asked to authorize before accessing the course