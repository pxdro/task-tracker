Feature: User Registration
  As a new user,
  I want to register with an email and password,
  So that I can login on app.

  Background:
    Given the API is running

  Scenario Outline: Registration fails due to invalid input
    When I register email "<email>" and password "<password>"
    Then I should be returned code <status>
    And I should see the message "<message>"
  
  Examples:
    | email             | password      | status | message  |
    | invalid-email     | Str0ngP@ss!   | 400    | error    |
    |                   | Str0ngP@ss!   | 400    | error    |
    | user1@example.com |               | 400    | error    |
    |                   |               | 400    | error    |

  Scenario: Successful registration
    When I register email "user2@example.com" and password "Str0ngP@ss!"
    Then I should be returned code 201
    And I should receive a confirmation email
    
  Scenario: Registration with already registered email
    Given I have registered with email "user3@example.com" and password "Str0ngP@ss!"
    When I register email "user3@example.com" and password "Str0ngP@ss!"
    Then I should be returned code 403
    And I should see the message "Email already registered"