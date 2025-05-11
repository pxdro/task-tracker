Feature: User Registration
  As a new visitor
  I want to register with my email and password
  So that I can create and manage tasks

  Scenario: Successful registration
    When I submit valid email "user@example.com" and password "Str0ngP@ss!"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    Then my account should be created
      # Checks for HTTP 201 Created
    And I should receive a confirmation email
      # Checks for X-Confirmation-Sent header in response

  Scenario: Registration with existing email
    Given I have already registered with email "user@example.com"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    When I submit this email and any password
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    Then I should see the error message "Email already registered" for user registration
      # Checks for HTTP 409 Conflict with error message