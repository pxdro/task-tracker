Feature: User Registration
  As a new visitor
  I want to register with my email and password
  So that I can create and manage tasks

  Scenario: Successful registration
    When I submit valid email "user@example.com" and password "Str0ngP@ss!"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    Then I should see the message "Email registered successfully" with code 201
      # Checks for HTTP 201 Created with message "Email registered successfully"
    And I should receive a confirmation email
      # Checks for X-Confirmation-Sent header in response

  Scenario: Registration with existing email
    Given I have already registered with email "user@example.com"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    When I submit this email and any password
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    Then I should see the message "Email already registered" with code 409
      # Checks for HTTP 409 Conflict with message "Email already registered"