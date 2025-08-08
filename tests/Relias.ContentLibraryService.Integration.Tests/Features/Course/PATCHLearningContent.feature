Feature: Patch Learning Content for a Course
  I want to update or create learning content, allowing updates to section names as well as creating and removing sections

  Rule: An authorized user can manage learning content and its details
    Background:
      Given the user is authorized to update courses
      And learning content exists

    @valid
    Scenario: Successfully updating the name of an existing section
      When the user updates the section title
      Then the update should be successful
      And the section should have the updated title 

    @valid
    Scenario: Successfully creating a new section
      When the user creates a new section 
      Then a new section should be created 
      And the update should be successful

   @valid
    Scenario: Moving a section 
      When the user moves a section 
      Then the section should be moved

    @valid
    Scenario: Removing a section 
      When the user deletes a section 
      Then the section should be deleted
      
    @invalid
    Scenario: Attempting to update a section with an empty name
      When the user updates the section name with an empty string
      Then the request should be rejected
      And the validation message should contain "Name.En cannot be empty"

  Rule: An unauthorized user cannot access courses and their learning content
    Background:
      Given the user is not authorized

    @authorization
    Scenario: Trying to retrieve learning content without access to Courses
      When the user attempts to access a course
      Then the user should be asked to authorize before accessing the course
