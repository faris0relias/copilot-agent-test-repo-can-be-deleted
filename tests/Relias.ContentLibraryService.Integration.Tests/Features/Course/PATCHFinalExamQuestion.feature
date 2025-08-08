Feature: Patch Question for a Final Exam
  Allows authorized users to create, update, or remove final exam questions and options.

  Background:
    Given the user is authorized to update courses
    And Final Exam exists

  @valid
  Scenario: Successfully add a new single-select question with valid options in the final exam
    When the user tries to add a new single-select question with options and one correct answer
    Then the new single-select question and its options should be saved correctly

  Scenario: Successfully add a new multi-select question with valid options in the final exam
    When the user tries to add a new multi-select question with options and more than one correct answer
    Then the new multi-select question and its options should be saved correctly

  Scenario: Successfully remove a question in the final exam
    When the user patch with remove action and its QuestionId
    Then the question should be deleted from the final exam

  
  

  @invalid
  Scenario: Updating a question with empty text in the final exam
    When the user tries to update the question with empty text
    Then the update should fail with "QuestionText.En cannot be empty."

  Scenario: Submitting an invalid question type in the final exam
    When the user tries to add a question with an invalid question type
    Then the update should fail with "Question type must be valid"

  Scenario: Add single-select question with multiple correct answers in the final exam
    When the user add two correct options
    Then the update should fail with "Question must contain only one correct answer."

  Scenario: Add multi-select with no correct answers in the final exam
    When the user submit options with none marked correct
    Then the update should fail with "Question must contain at least one correct answer."

  Scenario: Add a question with more than 50 options in the final exam
    When the user patch the question with more than 50 options
    Then the update should fail with "Question must contain at least 2 options and a maximum of 50."

  Scenario: Remove only correct answer from a question in the final exam
    When the user try to remove correct option
    Then the update should fail with "Cannot remove the only correct option from the question."

  
