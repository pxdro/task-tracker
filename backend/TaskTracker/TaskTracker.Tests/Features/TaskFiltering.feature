Feature: Task Filtering
  As an authenticated user
  I want to filter my tasks by status
  So that I can focus on what matters

  Background:
    Given I am logged in as "user@example.com" with password "Str0ngP@ss!"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
      # Performs HTTP POST to /api/auth/login with JSON { email, password }
    And I have the following tasks
      | id     | title           | status     |
      | 1      | Write report    | Active     |
      | 2      | Review PR       | Completed  |
      | 3      | Plan meeting    | Active     |
      # Performs 3 HTTP POST to /api/tasks with JSON { title, status }

  Scenario: Filter active tasks
    When I filter tasks by status "Active"
      # Performs HTTP GET to /api/tasks?status=Active
    Then I should see the message "null" with code 200
      # Checks for HTTP 200 OK with no message
    And I should see 2 active tasks
      # Validates that 2 tasks are returned, both with status = "Active"

  Scenario: Filter with invalid status
    When I filter tasks by status "Unknown"
      # Performs HTTP GET to /api/tasks?status=Unknown
    Then I should see the message "Invalid status filter" with code 400
      # Checks for HTTP 400 Bad Request with message "Invalid status filter"
