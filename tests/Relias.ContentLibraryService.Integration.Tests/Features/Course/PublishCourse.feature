Feature: Publish Course
  I want to publish a course immediately or schedule it for publishing

  Rule: An authorized user can publish courses
    Background:
      Given A user is authorized to access courses
	    And a course exists for the organization
      And all the required fields to be filled out to publish or schedule publish a course

    @valid
    Scenario: Successfully publishing a course immediately
      When the user publishes a course immediately
      Then the request should be successful 
      And the course should be marked as published with the current dateTime

    @valid
    Scenario: Successfully scheduling a course to be published for a future date
      When the user schedules a course to be published for a future date and time 
      Then the request should be successful 
      And  the course should be marked as scheduled for publish with a future date and time

    @invalid
    Scenario: Trying to publish a course that does not exist
      When the user tries to publish a course that does not exist
      Then the user should be informed that the course was not found 

    @invalid
    Scenario: Trying to schedule a course with a past date
      When the user tries to schedule a course with a past date
      Then the request should be rejected
      And the validation message should be "Scheduled publish date cannot be in the past."

    @invalid
    Scenario: Trying to publish course that is already published
      When the user tries to publish a course that is already published
      Then the request should return error 
      And the error message should contain "is already published."


  Rule: An unauthorized user cannot publish courses
    Background:
      Given an unauthorized user

    @authorization
    Scenario: Trying to publish a course without being logged in
      When the user is not authenticated
      Then the response status message should be "Unauthorized"
