Feature: Create a New Lesson
  I want to add a new lesson to a section so that it can be used in a course

  Rule: A user who is allowed to add lessons to a section
    Background:
      Given the user is authorized to update courses
      And a section already exists

    @valid
    Scenario: Adding a new lesson with a URL
      When the user adds a lesson called "Intro Video" to the section
      And includes a duration of 30 minutes
      Then the lesson should be added successfully
      

    @valid
    Scenario: Adding a new lesson with a file name
      When the user adds a lesson called "PDF Guide" to the section
      And includes a duration of 20 minutes
      Then the lesson should be added successfully

    @valid
    Scenario Outline:Adding a new lesson with LessonType correctly determines valid FormatType
      Given the lesson type is "<LessonType>"
      And the format type is "<FormatType>"
      When the user adds a new lesson
      Then the lesson should be added successfully

    Examples:
      | LessonType | FormatType |
      | file       | Pdf        |
      | file       | Audio      |
      | file       | Video      |
      | file       | Scorm      |
      | file       | Aicc       |
      | url        | Url        |

    @invalid
    Scenario: Adding a lesson without a title
      When the user tries to add a lesson to a section but leaves the title blank
      Then the lesson should not be added
      And the validation message should be "Name.En is required."

    @invalid
    Scenario: Duration includes a negative value
      When the user creates a new lesson with a duration of -10 minutes
      Then the request should be rejected
      And the validation message should be "DurationMinutes must be greater than or equal to 0."

    @invalid
    Scenario: Adding a lesson to a section that does not exist
      When the user tries to add a lesson to a section that doesn't exist
      Then the lesson should not be added
      And the error message should contain "not found for course"


  Rule: An unauthorized user cannot access courses and their learning content
    Background:
      Given the user is not authorized

    @authorization
    Scenario: Trying to retrieve learning content without access to Courses
      When the user attempts to access a course
      Then the user should be asked to authorize before accessing the course