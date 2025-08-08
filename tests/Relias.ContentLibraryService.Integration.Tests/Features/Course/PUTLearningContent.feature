Feature: Put Learning Content for a Course
I want to update learning content, allowing updates to lesson titles and properties.

  Rule: An authorized user can manage learning content and its details
    Background:
      Given the user is authorized to update courses
      And learning content exists for a given section

    @valid
    Scenario: Successfully updating a lesson's title
      When the user updates the lesson title "Updated Lesson Title"
      Then the update should be successful
      And the lesson should be updated
    
    @valid
    Scenario: Successfully updating a lesson's duration
      When the user updates the lesson duration to 60
      Then the update should be successful
      And the lesson should be updated
      
    @valid
    Scenario: Successfully updating a lesson's content path
      When the user updates the lesson content path "https://newContentPath.fake"
      Then the update should be successful
      And the lesson should be updated
      
    @valid
    Scenario: Successfully updating a lesson's completion requirement
      When the user updates the lesson completion requirement "false"
      Then the update should be successful
      And the lesson should be updated
    
    @valid
    Scenario: Successfully updating a lesson's open in new tab property
      When the user updates the lesson open in new tab property "true"
      Then the update should be successful
      And the lesson should be updated
    
    @valid
    Scenario: Successfully updating a lesson's requires video property
      When the user updates the lesson requires video property "true"
      Then the update should be successful
      And the lesson should be updated

    @valid
    Scenario: Successfully updating a lesson's requires audio property
      When the user updates the lesson requires audio property "true"
      Then the update should be successful
      And the lesson should be updated

    @valid
    Scenario: Delete a lesson file by setting deleteFile to true
	  When the user updates the lesson to delete the file
	  Then the update should be successful
	  And the lesson file should be deleted

    @valid
    Scenario: Setting deleteFile to true for a URL lesson should not clear the content path
	  When the user updates the URL lesson with deleteFile set to true
	  Then the update should be successful
	  And the URL should remain in the content path and fileName

    @valid
    Scenario Outline:Updating existing lesson with LessonType correctly determines valid FormatType
      Given the lesson type is "<LessonType>"
      And the format type is "<FormatType>"
      When the user updates a lesson
      Then the lesson should be updated

    Examples:
      | LessonType | FormatType |
      | file       | Pdf        |
      | file       | Audio      |
      | file       | Video      |
      | file       | Scorm      |
      | file       | Aicc       |
      | url        | Url        |


    @invalid
    Scenario: Attempting to update a lesson title to an empty string
      When the user updates the lesson title ""
      Then the request should be rejected
      And the validation message should be "Name.En is required."

    @Invalid
    Scenario: User tries to modify lesson's type
      When the user updates the lesson type "url"
      Then the request should be rejected

    @invalid
    Scenario: Duration includes a negative value
      When the user updates the lesson duration to -10
      Then the request should be rejected
      And the validation message should be "DurationMinutes must be greater than or equal to 0."

    @invalid
    Scenario: Attempting to update a non-existent lesson
      When the user tries to update a lesson that does not exist
      Then the request should return error
      And the error message should contain "not found in section"

  Rule: An unauthorized user cannot access courses and their learning content
    Background:
      Given the user is not authorized

    @authorization
    Scenario: Trying to retrieve learning content without access to Courses
      When the user attempts to access a course
      Then the user should be asked to authorize before accessing the course
