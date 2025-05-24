Feature: User Login
  As a registered user,
  I want to log in with my email and password,
  So that I can manage my tasks.

  Background:
    Given the API is running    

  Scenario: Successful login
    Given I have registered with email "login_example1@email.com" and password "StrongPass123"
    When I login with email "login_example1@email.com" and password "StrongPass123"
    Then I should be returned code 200
    And I should receive valid tokens

  Scenario: Login with unregistered email
    When I login with email "login_example2@email.com" and password "AnyPass123"
    Then I should be returned code 404
    And I should see the message "Email unregistered"

  Scenario: Login with wrong password
    Given I have registered with email "login_example3@email.com" and password "StrongPass123"
    When I login with email "login_example3@email.com" and password "WrongPass123"
    Then I should be returned code 401
    And I should see the message "Invalid credentials"

  Scenario Outline: Login fails due to invalid input
    When I login with email "<email>" and password "<password>"
    Then I should be returned code <status>
    And I should see the message "<message>"
  
  Examples:
    | email                     | password      | status | message  |
    | invalid-email             | StrongPass123 | 400    | error    |
    |                           | StrongPass123 | 400    | error    |
    | login_example4@email.com  |               | 400    | error    |
    |                           |               | 400    | error    |
    | login_example5@email.com  | 12345         | 400    | error    | 