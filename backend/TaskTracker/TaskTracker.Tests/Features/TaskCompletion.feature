Feature: Task Completion
  As an authenticated user
  I want to mark tasks as completed or uncompleted
  So that I can track my progress

  Background:
    Given I am logged in as "user@example.com" with password "Str0ngP@ss!"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
      # Performs HTTP POST to /api/auth/login with JSON { email, password }
    And I have the following tasks
      | id     | title           | status     |
      | 1      | Write report    | Active     |
      | 2      | Buy groceries   | Completed  |
      # Performs HTTP POST to /api/tasks with JSON { title, status }

  Scenario: Mark task as completed
    When I mark the task with ID "1" as completed
      # Performs HTTP PATCH to /api/tasks/1/complete
    Then the task status should be "Completed"
      # Checks for HTTP 200 OK
      # Checks that the response body has status = "Completed"