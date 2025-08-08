Feature: Create Course
  I want to create a course 

  Rule: An authorized user can create courses
    Background:
      Given A user is authorized to access courses

    @valid
    Scenario: Successfully creating a course
      When the user provides valid course details
      Then the response status message should be "Created"
      And a course ID should be returned
      And a LearningContent document should exist in Cosmos for the created course

    @invalid
    Scenario: Trying to create a course without a name which is a required field
      When the user does not provide a course name
      Then the response status message should be "Bad Request"
      And the validation message should be "Name for the course is required."

    @invalid
    Scenario: Trying to create a course without a content code which is a required field
      When the user does not provide a content code
      Then the response status message should be "Bad Request"
      And the validation message should be "Content code is required."

    @invalid
    Scenario: Trying to create a course with a name that is more than 500 characters
      When the user provides a course name that exceeds the allowed limit
      Then the response status message should be "Bad Request"
      And the validation message should be "Name for the course cannot exceed 500 characters in length."

    @invalid
    Scenario: Trying to create a course with a content code that is more than 100 characters
      When the user provides a content code that exceeds the allowed limit
      Then the response status message should be "Bad Request"
      And the validation message should be "Content code can not exceed 100 characters in length."

    @invalid
    Scenario: Trying to create a course with a content code with special characters
      When the user provides a content code with special characters
      Then the response status message should be "Bad Request"
      And the validation message should be "Content code can only contain numbers, letters, and/or dashes."

    @invalid
    Scenario: Trying to create a course with a content code that begins with special characters
      When the user provides a content code starting with special characters
      Then the response status message should be "Bad Request"
      And the validation message should be "Content code should start with a number or letter."

    @invalid
    Scenario: Trying to create a course with a duplicate content code
      When the user provides a content code that already exists
      Then the response status message should be "Bad Request"
      And the error message should be "Value 'C101' is invalid. Reason: The content code is already in use."

    @authorization
    Scenario: Trying to create a course without required permissions
      When the user does not have the required permissions
      Then the response status message should be "Forbidden"

  Rule: An unauthorized user cannot create courses
    Background:
      Given an unauthorized user

    @authorization
    Scenario: Trying to create a course without being logged in
      When the user is not authenticated
      Then the response status message should be "Unauthorized"
