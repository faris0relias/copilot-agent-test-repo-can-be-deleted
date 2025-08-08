Feature: GETContentTypes
As a  user I want to get the list of content types so that I can see what content types are available

Rule: An authorized user who can get content types
  Background:
    Given An authorized user with access to content library service
    And A list of content types

  @valid
  Scenario: Successfully retrieving a list of content types
    When  The user access the list of Content Types
    Then  The response status message should be "OK"
    And   A list of Content Types should be returned with values ContentTypeId and ContentTypeDescription
 
Rule: An unauthorized user cannot get content types
    Background:
      Given An unauthorized user

 @authorization
 Scenario: Trying to retrieve a content types without being logged in
    When The user access the list of Content Types
    Then The response status message should be "Unauthorized"

