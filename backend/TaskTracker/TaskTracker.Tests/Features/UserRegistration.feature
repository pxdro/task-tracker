Feature: User Registration
  As a new visitor
  I want to register with my email and password
  So that I can create and manage tasks

  Background:
    Given the API is running

  Scenario Outline: Registration fails due to invalid input
    When I register email "<email>" and password "<password>"
    Then I should be returned code <status>
    And I should see the message "<message>"
  
  Examples:
    | email             | password      | status | message                    |
    | invalid-email     | Str0ngP@ss!   | 400    | "Missing required data"    |
    |                   | Str0ngP@ss!   | 400    | "Missing required data"    |
    | user@example.com  |               | 400    | "Missing required data"    |
    |                   |               | 400    | "Missing required data"    |

  Scenario: Successful registration
    When I register email "user@example.com" and password "Str0ngP@ss!"
    Then I should be returned code 201
    And I should receive a confirmation email
    
  Scenario: Registration with already registered email
    Given I have registered with email "user@example.com" and password "Str0ngP@ss!"
    When I register email "user@example.com" and password "Str0ngP@ss!"
    Then I should be returned code 409
    And I should see the message "Email already registered"