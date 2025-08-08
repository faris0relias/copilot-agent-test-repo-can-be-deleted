Feature: Get Learner Final Exam Setting by Course ID

I want to  retrieve learner final exam setting by course ID

 Background:
    Given A user is authorized to access courses
    And learner final exam setting exists
    
    @valid
    Scenario: Learner start final exam
        When the learner start final exam
        Then final exam setting should be returned



