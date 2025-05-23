Feature: User Login
  As a registered user,
  I want to log in with my email and password,
  So that I can manage my tasks.

  Background:
    Given the API is running
    
  Scenario Outline: Login fails due to invalid input
    When I login with email "<email>" and password "<password>"
    Then I should be returned code <status>
    And I should see the message "<message>"
  
  Examples:
    | email             | password      | status | message  |
    | invalid-email     | Str0ngP@ss!   | 400    | error    |
    |                   | Str0ngP@ss!   | 400    | error    |
    | user4@example.com |               | 400    | error    |
    |                   |               | 400    | error    |

  Scenario: Successful login
    Given I have registered with email "user5@example.com" and password "Str0ngP@ss!"
    When I login with email "user5@example.com" and password "Str0ngP@ss!"
    Then I should be returned code 200
    And I should receive valid tokens

  Scenario: Login with unregistered email
    When I login with email "user6@example.com" and password "AnyPass123"
    Then I should be returned code 404
    And I should see the message "Email unregistered"

  Scenario: Login with wrong password
    Given I have registered with email "user7@example.com" and password "Str0ngP@ss!"
    When I login with email "user7@example.com" and password "WrongPass123"
    Then I should be returned code 401
    And I should see the message "Invalid credentials"