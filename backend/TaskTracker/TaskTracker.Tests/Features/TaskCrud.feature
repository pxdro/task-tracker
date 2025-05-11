Feature: Task CRUD
  As an authenticated user
  I want to manage my personal tasks
  So that I can stay organized and productive

  Background:
    Given I am logged in as "user@example.com" with password "Str0ngP@ss!"
      # Performs HTTP POST to /api/auth/register with JSON { email, password }
      # Performs HTTP POST to /api/auth/login with JSON { email, password }

  Scenario: Read all tasks
    Given I have 2 existing tasks in my account
      # Performs 2 HTTP POST to /api/tasks with JSON { title, description }
    When I request my task list
      # Performs HTTP GET to /api/tasks with JSON { email }
    Then I should see my tasks
      # Checks for HTTP 200 OK
      # Validates that are 2 returned tasks

  Scenario: Create a new task
    When I create a task with title "Buy groceries" and description "Milk, eggs, bread"
      # Performs HTTP POST to /api/tasks with JSON { title, description }
    Then the task should be saved
      # Checks for HTTP 201 Created
      # Checks that task ID is returned in response

  Scenario: Update an existing task
    Given I have a task with ID "abc123" and title "Buy groceries" and description "Milk, eggs, bread"
      # Performs HTTP POST to /api/tasks with JSON { title, description }
    When I update this task's title to "Buy groceries and fruits" and description to "Milk, eggs, bread, apples"
      # Performs HTTP PUT to /api/tasks/{id} with updated JSON { title, description }
    Then the task should be updated
      # Checks for HTTP 200 OK
      # Checks if response body contains a task with title "Buy groceries and fruits"
      # Checks if response body contains a task with description "Milk, eggs, bread, apples"

  Scenario: Delete a task
    Given I have a task with ID "abc123" and title "Buy groceries" and description "Milk, eggs, bread"
      # Performs HTTP POST to /api/tasks with JSON { title, description }
    When I delete this task
      # Performs HTTP DELETE to /api/tasks/{id}
    Then the task should no longer exist
      # Checks for HTTP 204 No Content