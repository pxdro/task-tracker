using Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Entities;
using TaskTracker.Application.Enums;
using TaskTracker.Infrastructure.Context;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.CommonModels;

namespace TaskTracker.Tests.Steps
{
    [Binding]
    public class TaskSteps(WebApplicationFactory<Program> factory, ScenarioContext ctx)
    {
        private readonly HttpClient _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing")).CreateClient();
        private readonly ScenarioContext _ctx = ctx;

        [Given(@"I am logged in as ""(.*)"" with password ""(.*)""")]
        public async Task GivenIAmLoggedInAsWithPassword(string email, string password)
        {
            var dto = new { Email = email, Password = password };
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<ResultDto<TokensDto>>();
            Assert.NotNull(content);
            Assert.NotNull(content?.Data?.AuthToken);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content?.Data?.AuthToken);
        }

        [Given(@"I have the following tasks")]
        public async Task GivenIHaveTheFollowingTasks(Table table)
        {
            foreach (var row in table.Rows)
            {
                var dto = new
                {
                    Title = row["title"],
                    Description = row["description"],
                    Status = row["status"]
                };
                await _client.PostAsJsonAsync("/api/tasks", dto);
            }
        }

        [When(@"I request my task list")]
        public async Task WhenIRequestMyTaskList()
        {
            var response = await _client.GetAsync("/api/tasks");
            _ctx["response"] = response;
        }

        [When(@"I filter my tasks by ""(.*)"" with ""(.*)""")]
        public async Task WhenIFilterMyTasksByWith(string field, string value)
        {
            var response = await _client.GetAsync($"/api/tasks/filter?field={field}&value={value}");
            _ctx["response"] = response;
        }

        [When(@"I request task by Id")]
        public async Task WhenIRequestTaskById()
        {
            await WhenIRequestMyTaskList();
            var preResponse = (HttpResponseMessage)_ctx["response"];
            var preContent = await preResponse.Content.ReadFromJsonAsync<ResultDto<IEnumerable<TaskReturnDto>>>();
            Assert.NotNull(preContent?.Data);
            var task = preContent?.Data?.FirstOrDefault();
            Assert.NotNull(task);

            var response = await _client.GetAsync($"/api/tasks/{task.Id}");
            _ctx["response"] = response;
        }

        [When(@"I create a task with title ""(.*)"" and description ""(.*)""")]
        public async Task WhenICreateATaskWithTitleAndDescription(string title, string description)
        {
            var dto = new { Title = title, Description = description };
            var response = await _client.PostAsJsonAsync("/api/tasks", dto);
            _ctx["response"] = response;
        }

        [When(@"I update task to have title ""(.*)"" and description ""(.*)"" and status ""(.*)""")]
        public async Task WhenIUpdateTaskToHaveTitleAndDescriptionAndStatus(string title, string description, EnumTaskStatus status)
        {
            await WhenIRequestMyTaskList();
            var preResponse = (HttpResponseMessage)_ctx["response"];
            var preContent = await preResponse.Content.ReadFromJsonAsync<ResultDto<IEnumerable<TaskReturnDto>>>();
            Assert.NotNull(preContent?.Data);
            var task = preContent?.Data?.FirstOrDefault();
            Assert.NotNull(task);

            var dto = new { Title = title, Description = description, Status = status };
            var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", dto);
            _ctx["response"] = response;
        }

        [When(@"I delete the task")]
        public async Task WhenIDeleteTheTask()
        {
            await WhenIRequestMyTaskList();
            var preResponse = (HttpResponseMessage)_ctx["response"];
            var preContent = await preResponse.Content.ReadFromJsonAsync<ResultDto<IEnumerable<TaskReturnDto>>>();
            Assert.NotNull(preContent?.Data);
            var task = preContent?.Data?.FirstOrDefault();
            Assert.NotNull(task);

            var response = await _client.DeleteAsync($"/api/tasks/{task.Id}");
            _ctx["response"] = response;
        }

        [Then(@"the content returned should not be empty")]
        public async Task ThenTheContentReturnedShouldNotBeEmpty()
        {
            var response = (HttpResponseMessage)_ctx["response"];
            var content = await response.Content.ReadFromJsonAsync<ResultDto<object>>();

            Assert.NotNull(content?.Data);
        }

        [Then(@"the task should have title ""(.*)""")]
        public async Task ThenTheTaskShouldHaveTitle(string title)
        {
            var response = (HttpResponseMessage)_ctx["response"];
            var content = await response.Content.ReadFromJsonAsync<ResultDto<TaskReturnDto>>();

            Assert.NotNull(content?.Data);
            Assert.Equal(title, content.Data.Title);

            _ctx["lastTask"] = content.Data;
        }

        [Then(@"the task should have description ""(.*)""")]
        public async Task ThenTheTaskShouldHaveDescription(string description)
        {
            var task = _ctx.Get<TaskReturnDto>("lastTask");

            Assert.NotNull(task);
            Assert.Equal(description, task.Description);
        }

        [Then(@"the task should have status ""(.*)""")]
        public async Task ThenTheTaskShouldHaveStatus(EnumTaskStatus status)
        {
            var task = _ctx.Get<TaskReturnDto>("lastTask");

            Assert.NotNull(task);
            Assert.Equal(status, task.Status);
        }
    }
}