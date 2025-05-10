# docs/features/UserRegistration.feature
Feature: User Registration
  Who:  As a new visitor
  What: I want to register with my email and password
  Why:  So that I can create and manage tasks

  Scenario: Successful registration
    Given I am on the registration page
    When I submit valid email "user@example.com" and password "Str0ngP@ss!"
    Then my account should be created
    And I should receive a confirmation email

  Scenario: Registration with existing email
    Given I have already registered with email "user@example.com"
    When I submit the same email and a password
    Then I should see an error message "Email already in use"
