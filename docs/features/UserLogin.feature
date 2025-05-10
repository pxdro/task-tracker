Feature: User Login
  Who:  As a registered user
  What: I want to log in with my email and password
  Why:  So that I can access my private task list

  Scenario: Successful login
    Given I have an existing account with email "user@example.com" and password "Str0ngP@ss!"
    When I send a POST request to "/api/auth/login" with body:
      """
      {
        "email": "user@example.com",
        "password": "Str0ngP@ss!"
      }
      """
    Then the response status code should be 200
    And the response body should contain a valid JWT access token
    And the response body should contain a valid JWT refresh token

  Scenario: Login with incorrect password
    Given I have an existing account with email "user@example.com" and password "Str0ngP@ss!"
    When I send a POST request to "/api/auth/login" with body:
      """
      {
        "email": "user@example.com",
        "password": "WrongP@ss!"
      }
      """
    Then the response status code should be 401
    And the response body should contain the error message "Invalid credentials"

  Scenario: Login with unregistered email
    Given I do not have an account with email "unknown@example.com"
    When I send a POST request to "/api/auth/login" with body:
      """
      {
        "email": "unknown@example.com",
        "password": "AnyP@ss123"
      }
      """
    Then the response status code should be 404
    And the response body should contain the error message "User not found"
