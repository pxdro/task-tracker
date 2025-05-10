Feature: Task CRUD
  Who:  As an authenticated user
  What: I want to create, read, update, and delete tasks
  Why:  So that I can manage my to-do list

  Background:
    Given I am logged in as "user@example.com"

  Scenario: Create a new task
    When I send a POST request to "/api/tasks" with body:
      """
      {
        "title": "Buy groceries",
        "description": "Milk, eggs, bread",
        "dueDate": "2025-06-01T12:00:00Z"
      }
      """
    Then the response status code should be 201
    And the response body should contain the task with title "Buy groceries"

  Scenario: Read all tasks
    Given I have 3 existing tasks in my account
    When I send a GET request to "/api/tasks"
    Then the response status code should be 200
    And the response body should contain a JSON array of length 3

  Scenario: Update an existing task
    Given I have a task with ID "abc123" and title "Buy groceries"
    When I send a PUT request to "/api/tasks/abc123" with body:
      """
      {
        "title": "Buy groceries and fruits",
        "description": "Milk, eggs, bread, apples"
      }
      """
    Then the response status code should be 200
    And the response body should contain the task with title "Buy groceries and fruits"

  Scenario: Delete a task
    Given I have a task with ID "xyz789"
    When I send a DELETE request to "/api/tasks/xyz789"
    Then the response status code should be 204
    And the task with ID "xyz789" should no longer exist in the system
