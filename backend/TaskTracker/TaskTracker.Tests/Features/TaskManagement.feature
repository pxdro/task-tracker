Feature: Task Management
  As an authenticated user
  I want to read, create, update, and delete tasks,
  So that I can manage my to-dos.

  Background:
    Given I have registered with email "task_example@email.com" and password "StrongPass123"
    And I am logged in as "task_example@email.com" with password "StrongPass123"
    And I have the following tasks
      | title           | description | status    |
      | Write report    | For work    | Active    |
      | Build slides    | For work    | Completed |
      | Buy groceries   | For home    | Active    |
      | Dump the thrash | For home    | Completed |
    And the API is running

  Scenario: Read all tasks
    When I request my task list
    Then I should be returned code 200 
    And the content returned should not be empty
    
  Scenario Outline: Read filtered tasks
    When I filter my tasks by "<field>" with "<value>"
    Then I should be returned code 200
    And the content returned should not be empty

    Examples:
      | field       | value     |
      | title       | write     |
      | description | home      |
      | status      | Active    |

  Scenario: Read task with Id
    When I request task by Id
    Then I should be returned code 200 
    And the content returned should not be empty

  Scenario: Create a new task
    When I create a task with title "Review PR" and description "ASAP"
    Then I should be returned code 201 
    And the task should have title "Review PR"
    And the task should have description "ASAP"
    And the task should have status "Active"

  Scenario: Update a task
    When I update task to have title "Write report and send email" and description "For work tasks" and status "Active"
    Then I should be returned code 200 
    And the task should have title "Write report and send email"
    And the task should have description "For work tasks"
    And the task should have status "Active"

  Scenario: Delete a task
    When I delete the task
    Then I should be returned code 204 