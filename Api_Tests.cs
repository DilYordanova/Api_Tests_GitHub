using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api_Test_GitHub_Homework
{
    public class Tests
    {
        const string GitHubApiUserName = "ENTER_YOUR_GITHUB_USERNAME";
        const string GitHubApiToken = "ENTER_YOUR_GITHUB_TOKEN";
        private RestClient client;

        [SetUp]
        public void Setup()
        {
            client = new RestClient("https://api.github.com");
        }

        [Test]
        public async Task Get_AllIssues()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues");
            var response = await this.client.ExecuteAsync(request, Method.Get);
            var issues = JsonSerializer.Deserialize<List<Issue>>(response.Content);

            Assert.That(issues.Count > 1);
            foreach (var issue in issues)
            {
                Assert.Greater(issue.id, 0);
                Assert.Greater(issue.number, 0);
                Assert.IsNotEmpty(issue.title);
            }

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Get_IssueByNumber()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues/10");
            var response = await this.client.ExecuteAsync(request, Method.Get);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            Assert.Greater(issue.id, 0);
            Assert.Greater(issue.number, 0);
            Assert.IsNotEmpty(issue.title);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Get_IssueByInvalidNumber()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues/63163341265");
            var response = await this.client.ExecuteAsync(request, Method.Get);         

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Create_Issue()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues");
            client.Authenticator = new HttpBasicAuthenticator(GitHubApiUserName, GitHubApiPassword);
            request.AddHeader("Content-type", "application/json");
            string title = "New issue from RestSharp";
            string body = "Something";
            request.AddBody(new { title, body });
            var response = await this.client.ExecuteAsync(request, Method.Post);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            Assert.Greater(issue.id, 0);
            Assert.Greater(issue.number, 0);
            Assert.IsNotEmpty(issue.title);
            Assert.IsNotEmpty(issue.body);
            Assert.AreEqual("New issue from RestSharp", issue.title);
        }

        [Test]
        public async Task Create_IssueWithoutAuthorization()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues");            
            request.AddHeader("Content-type", "application/json");
            string title = "New issue from RestSharp";
            string body = "Something";
            request.AddBody(new { title, body });
            var response = await this.client.ExecuteAsync(request, Method.Post);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
           }

        [Test]
        public async Task Create_IssueMissingTitle()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues");
            client.Authenticator = new HttpBasicAuthenticator(GitHubApiUserName, GitHubApiPassword);
            request.AddHeader("Content-type", "application/json");            
            string body = "Something";
            request.AddBody(new { body });
            var response = await this.client.ExecuteAsync(request, Method.Post);

            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test]
        public async Task Edit_Issue()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues/10");
            client.Authenticator = new HttpBasicAuthenticator(GitHubApiUserName, GitHubApiPassword);
            request.AddJsonBody(new
            {
                title = "New title with RestSharp"
            }
            );

            var response = await client.ExecuteAsync(request, Method.Patch);
            var issues = JsonSerializer.Deserialize<Issue>(response.Content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var issue = JsonSerializer.Deserialize<Issue>(response.Content);

            Assert.Greater(issue.id, 0);
            Assert.Greater(issue.number, 0);
            Assert.That(issues.title.Contains("New title"));
        }

        [Test]
        public async Task Edit_NonExistingIssue()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues/63163341265");
            client.Authenticator = new HttpBasicAuthenticator(GitHubApiUserName, GitHubApiPassword);
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(new
            {
                title = "New title with RestSharp"
            }
            );

            var response = await client.ExecuteAsync(request, Method.Patch);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Edit_IssueWithoutAuthorization()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues/10");           
            request.AddHeader("Content-type", "application/json");
            request.AddJsonBody(new
            {
                title = "New title with RestSharp"
            }
            );

            var response = await client.ExecuteAsync(request, Method.Patch);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public async Task Create_CommentIssue()
        {
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues/2/comments");
            client.Authenticator = new HttpBasicAuthenticator(GitHubApiUserName, GitHubApiPassword);
            request.AddJsonBody(new
            {
                body = "Comment...."
            }
            );

            var response = await client.ExecuteAsync(request, Method.Post);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public async Task Deleate_CommentIssue()
        {
            //Firts create issue comment
            var request = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues/2/comments");
            client.Authenticator = new HttpBasicAuthenticator(GitHubApiUserName, GitHubApiPassword);
            request.AddJsonBody(new
            {
                body = "One more Comment........"
            }
            );

            var response = await client.ExecuteAsync(request, Method.Post);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newComment = JsonSerializer.Deserialize<CommentIssue>(response.Content);

            //Second delete issue comment

            var requestDelete = new RestRequest("/repos/DiYordanova/SoftUni-QA-Automation/issues/comments/" + newComment.id, Method.Delete);
            var responseDelete = await client.ExecuteAsync(requestDelete);

            Assert.AreEqual(HttpStatusCode.NoContent, responseDelete.StatusCode);
        }
    }
}