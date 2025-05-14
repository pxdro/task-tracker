Feature: Task Management
  As an authenticated user
  I want to manage my personal tasks
  So that I can stay organized and productive

  Background:
    Given I am logged in as "user@example.com" with password "Str0ngP@ss!"
    And I have the following tasks
      | title           | description | status    |
      | Write report    | For work    | Active    |
      | Buy groceries   | For home    | Completed |
      | Dump the thrash | For home    | Completed |
    And the API is running

  Scenario: Read all tasks
    When I request my task list
    Then I should be returned code 200 
    And I should see 3 tasks

  Scenario Outline: Filter tasks
    When I request tasks filtered by <field> "<value>"
    Then I should be returned code 200 
    And I should see <count> tasks

    Examples:
      | field       | value         | count |
      | title       | Write         | 1     |
      | description | For home      | 2     |
      | status      | Completed     | 2     |
      | status      | Active        | 1     |

  Scenario: Create a new task
    When I create a task with title "Review PR" and description "ASAP"
    Then I should be returned code 200 
    And the task should be saved with status "Active"

  Scenario: Update a task
    When I update the task titled "Buy groceries" to have title "Buy groceries and fruits"
    Then I should be returned code 200 
    And I should see <count> tasks 
    And the task should have title "Buy groceries and fruits"

  Scenario: Change task's status
    When I mark the task titled "Write report" as completed
    Then I should be returned code 200 
    And the task "Write report" should have status "Completed"

  Scenario: Delete a task
    When I delete the task titled "Buy groceries"
    Then I should be returned code 204 
    And the task "Buy groceries" should not exist anymore