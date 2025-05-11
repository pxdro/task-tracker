Feature: User Login
  As a registered user
  I want to log in with my email and password
  So that I can access my private task list

  Scenario: Successful login
    Given I have registered with email "user@example.com" and password "Str0ngP@ss!"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    When I submit these email and password
      # Performs HTTP POST to /api/auth/login with JSON { email, password }
    Then I should login successfully
      # Checks for HTTP 200 OK
      # Checks for a valid JWT auth token
      # Checks for a valid JWT refresh token

  Scenario: Login with incorrect password
    Given I have registered with email "user@example.com" and password "Str0ngP@ss!"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
    When I submit this email and wrong password
      # Performs HTTP POST to /api/auth/login with JSON { email, "WrongPass123" }
    Then I should see the error message "Wrong password" for user login
      # Checks for HTTP 401 Unauthorized 
      # Checks for the error message "Wrong password" in response body 

  Scenario: Login with unregistered email
    When I submit unregistered email and any password
      # Performs HTTP POST to /api/auth/login with JSON { "unknown@example.com", "AnyPass123" }
    Then I should see the error message "User not found" for user login
      # Checks for HTTP 404 Not Found 
      # Checks for the error message "User not found" in response body 