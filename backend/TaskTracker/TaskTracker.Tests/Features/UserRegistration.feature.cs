﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace TaskTracker.Tests.Features
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class UserRegistrationFeature : object, Xunit.IClassFixture<UserRegistrationFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
        public UserRegistrationFeature(UserRegistrationFeature.FixtureData fixtureData, TaskTracker_Tests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en"), "Features", "User Registration", "  As a new user,\r\n  I want to register with an email and password,\r\n  So that I c" +
                    "an login on app.", ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
            testRunner.Given("the API is running", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Successful registration")]
        [Xunit.TraitAttribute("FeatureTitle", "User Registration")]
        [Xunit.TraitAttribute("Description", "Successful registration")]
        public void SuccessfulRegistration()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Successful registration", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                this.FeatureBackground();
                testRunner.When("I register email \"register_example1@email.com\" and password \"StrongPass123\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                testRunner.Then("I should be returned code 201", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                testRunner.And("I should receive a confirmation email", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Registration with already registered email")]
        [Xunit.TraitAttribute("FeatureTitle", "User Registration")]
        [Xunit.TraitAttribute("Description", "Registration with already registered email")]
        public void RegistrationWithAlreadyRegisteredEmail()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Registration with already registered email", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                this.FeatureBackground();
                testRunner.Given("I have registered with email \"register_example2@email.com\" and password \"StrongPa" +
                        "ss123\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                testRunner.When("I register email \"register_example2@email.com\" and password \"StrongPass123\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                testRunner.Then("I should be returned code 403", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                testRunner.And("I should see the message \"Email already registered\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableTheoryAttribute(DisplayName="Registration fails due to invalid input")]
        [Xunit.TraitAttribute("FeatureTitle", "User Registration")]
        [Xunit.TraitAttribute("Description", "Registration fails due to invalid input")]
        [Xunit.InlineDataAttribute("invalid-email", "AnyPass123", "400", "error", new string[0])]
        [Xunit.InlineDataAttribute("", "AnyPass123", "400", "error", new string[0])]
        [Xunit.InlineDataAttribute("register_example3@email.com", "", "400", "error", new string[0])]
        [Xunit.InlineDataAttribute("", "", "400", "error", new string[0])]
        [Xunit.InlineDataAttribute("register_example3@email.com", "12345", "400", "error", new string[0])]
        public void RegistrationFailsDueToInvalidInput(string email, string password, string status, string message, string[] exampleTags)
        {
            string[] tagsOfScenario = exampleTags;
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            argumentsOfScenario.Add("email", email);
            argumentsOfScenario.Add("password", password);
            argumentsOfScenario.Add("status", status);
            argumentsOfScenario.Add("message", message);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Registration fails due to invalid input", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                this.FeatureBackground();
                testRunner.When(string.Format("I register email \"{0}\" and password \"{1}\"", email, password), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                testRunner.Then(string.Format("I should be returned code {0}", status), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                testRunner.And(string.Format("I should see the message \"{0}\"", message), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                UserRegistrationFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                UserRegistrationFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
