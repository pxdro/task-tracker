Feature: User Registration
  As a new user,
  I want to register with an email and password,
  So that I can login on app.

  Background:
    Given the API is running
    
  Scenario: Successful registration
    When I register email "register_example1@email.com" and password "StrongPass123"
    Then I should be returned code 201
    And I should receive a confirmation email
    
  Scenario: Registration with already registered email
    Given I have registered with email "register_example2@email.com" and password "StrongPass123"
    When I register email "register_example2@email.com" and password "StrongPass123"
    Then I should be returned code 403
    And I should see the message "Email already registered"

  Scenario Outline: Registration fails due to invalid input
    When I register email "<email>" and password "<password>"
    Then I should be returned code <status>
    And I should see the message "<message>"
  
  Examples:
    | email                         | password      | status | message  |
    | invalid-email                 | AnyPass123    | 400    | error    |
    |                               | AnyPass123    | 400    | error    |
    | register_example3@email.com   |               | 400    | error    |
    |                               |               | 400    | error    |   
    | register_example3@email.com   | 12345         | 400    | error    | 