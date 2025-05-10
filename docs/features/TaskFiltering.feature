Feature: Task Filtering
  Who:  As an authenticated user
  What: I want to filter my tasks by status
  Why:  So that I can view only the tasks I need

  Background:
    Given I am logged in as "user@example.com"
    And I have the following tasks:
      | id     | title           | status     |
      | t1     | Write report    | Active     |
      | t2     | Review PR       | Completed  |
      | t3     | Plan meeting    | Active     |

  Scenario: Filter active tasks
    When I send a GET request to "/api/tasks?status=Active"
    Then the response status code should be 200
    And the response body should contain only tasks with status "Active"
    And the response body should be a JSON array of length 2

  Scenario: Filter completed tasks
    When I send a GET request to "/api/tasks?status=Completed"
    Then the response status code should be 200
    And the response body should contain only tasks with status "Completed"
    And the response body should be a JSON array of length 1

  Scenario: Filter with invalid status
    When I send a GET request to "/api/tasks?status=Unknown"
    Then the response status code should be 400
    And the response body should contain the error message "Invalid status filter"
