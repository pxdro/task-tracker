Feature: User Login
  As a registered user
  I want to log in with my email and password
  So that I can access my private task list

  Scenario: Successful login
    Given I have registered with email "user@example.com" and password "Str0ngP@ss!"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    When I submit these email and password
      # Performs HTTP POST to /api/auth/login with JSON { email, password }
    Then I should see the message "null" with code 200
      # Checks for HTTP 200 OK with no message
    And I should stay logged in
      # Checks for a valid JWT auth token
      # Checks for a valid JWT refresh token

  Scenario: Login with incorrect password
    Given I have registered with email "user@example.com" and password "Str0ngP@ss!"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    When I submit this email and wrong password
      # Performs HTTP POST to /api/auth/login with JSON { email, "WrongPass123" }
    Then I should see the message "Wrong password" with code 401
      # Checks for HTTP 401 Unauthorized with message "Wrong password"

  Scenario: Login with unregistered email
    When I submit unregistered email and any password
      # Performs HTTP POST to /api/auth/login with JSON { "unknown@example.com", "AnyPass123" }
    Then I should see the message "Email unregistered" with code 404
      # Checks for HTTP 404 Not Found with message "Email unregistered"