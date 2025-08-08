Feature: GETContentInfo
As a user I want to get more information about specific pieces of content so I know what they are about

Rule: An authorized user who can get content info
  Background:
    Given An authorized user with access to content library service

    @valid
    Scenario: Successfully retrieving a list of courses
	When The user requests content info given valid content ids
    Then The response status message should be "OK"
    And A list of content information should be returned

    @valid
    Scenario: No courses available
    When The user requests content info given invalid content ids
    Then The response status message should be "OK"
    And An empty list is returned
 
Rule: An unauthorized user cannot get content info
    Background:
      Given An unauthorized user

 @authorization
 Scenario: Trying to retrieve content info without being logged in
    When The unauthenticated user requests a list of Content Info
    Then The response status message should be "Unauthorized"
