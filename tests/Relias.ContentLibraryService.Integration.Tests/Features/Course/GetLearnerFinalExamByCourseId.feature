Feature: Get Learner Final Exam by Course ID

I want to  retrieve learner final exam and their details by course ID

 Background:
    Given A user is authorized to access courses
    And learner final exam exists
    
    @valid
    Scenario: Learner start final exam
        When the learner start exam
        Then final exam questions and options should be returned

    Scenario: Learner preview question of final exam
        When the learner preview exam
        Then final exam questions and options should be returned with correct answers and response



