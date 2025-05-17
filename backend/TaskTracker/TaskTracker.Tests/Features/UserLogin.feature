Feature: User Login
  As a registered user
  I want to log in with my email and password
  So that I can access my private task list

  Background:
    Given the API is running

  Scenario: Successful login
    Given I have registered with email "user@example.com" and password "Str0ngP@ss!"
    When I login with email "user@example.com" and password "Str0ngP@ss!"
    Then I should be returned code 200
    And I should receive a valid JWT auth token
    And I should receive a valid refresh token

  Scenario: Login with unregistered email
    When I login with email "unknown@example.com" and password "AnyPass123"
    Then I should be returned code 401
    And I should see the message "Email unregistered"

  Scenario: Login with wrong password
    Given I have registered with email "user@example.com" and password "Str0ngP@ss!"
    When I login with email "user@example.com" and password "WrongPass123"
    Then I should be returned code 401
    And I should see the message "Invalid credentials"