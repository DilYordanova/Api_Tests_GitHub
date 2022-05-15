using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using RestSharp.Serializers.Json;

namespace APITestsGitHub
{
    public class APITestsGitHub
    {
        private RestClient client;
        [SetUp]
        public void Setup()
        {
            this.client = new RestClient("https://api.github.com");
            string[] lines = Array.Empty<string>();
            string path = Directory.GetCurrentDirectory();
            path = path.Substring(0, path.IndexOf("APITestsGitHub"));
            foreach (string file in Directory.EnumerateFiles(path, "setup.txt"))
            {
                lines = File.ReadAllLines(file);
            }
            this.client.Authenticator = new HttpBasicAuthenticator(lines[0], lines[1]);
        }

        #region Helper Methods

        public async Task<Issue> CreateIssue(string title, string body)
        {
            //Arrange
            var request = new RestRequest("/repos/TeamAlabala/APISolutionAndPostman/issues");

            //Act
            request.AddBody(new { body, title });
            var responce = await this.client.ExecuteAsync(request, Method.Post);
            var issue = JsonSerializer.Deserialize<Issue>(responce.Content);
            return issue;

        }

        #endregion

        [Test]
        public async Task Test_GitHub_APIRequest()
        {
            //Arrange
            var request = new RestRequest("/repos/TeamAlabala/APISolutionAndPostman/issues");

            //Act
            var responce = await this.client.ExecuteAsync(request, Method.Get);
            var issues = JsonSerializer.Deserialize<List<Issue>>(responce.Content);

            //Assert
            Assert.That(issues.Count > 1);
            foreach (var item in issues)
            {
                Assert.Greater(item.id, 0);
                Assert.Greater(item.number, 0);
                Assert.IsNotEmpty(item.title);
            }

            Assert.AreEqual(HttpStatusCode.OK, responce.StatusCode);

        }

        [Test]
        public async Task Test_Create_GitHubIssueAsync()
        {
            //Arrange
            string title = "New issue from RestSharp";
            string body = "Something";

            //Act
            var issue = await CreateIssue(title, body);

            //Assert
            Assert.Greater(issue.id, 0);
            Assert.Greater(issue.number, 0);
            Assert.IsNotEmpty(issue.title);
        }

        [Test]
        public async Task GitHubAPI_CreateNewIssue_MissingTitle()
        {
            var request = new RestRequest("/repos/TeamAlabala/APISolutionAndPostman/issues");
            request.AddJsonBody(new
            {
                body = "some body",
                label = new string[] { "bug", "importance:high", "type:UI" }
            }
            );

            var response = await client.ExecuteAsync(request, Method.Post);

            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test]
        public async Task GitHubAPI_CreateCommentIssue11()
        {
            var request = new RestRequest("/repos/TeamAlabala/APISolutionAndPostman/issues/11/comments");           
            request.AddJsonBody(new
            {
                body = "Comment...."
            }
            );

            var response = await client.ExecuteAsync(request, Method.Post);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public async Task GitHubAPI_RenameTitleIssue11()
        {
            var request = new RestRequest("/repos/TeamAlabala/APISolutionAndPostman/issues/11");           
            request.AddJsonBody(new
            {
                title = "New title with RestSharp Nomber"
            }
            );
         
            var response = await client.ExecuteAsync(request, Method.Patch);
            var issues = JsonSerializer.Deserialize<Issue>(response.Content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.That(issues.title.Contains("New title"));
        }

        [Test]
        public async Task Test_GitHubAPI_DeleateComment()
        {
            //Firts create issue comment
            var request = new RestRequest("/repos/TeamAlabala/APISolutionAndPostman/issues/11/comments");
            request.AddJsonBody(new
            {
                body = "One more Comment........"
            }
            );

            var response = await client.ExecuteAsync(request, Method.Post);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newComment = JsonSerializer.Deserialize<CommentResponse>(response.Content);

            //Second delete issue comment

            var requestDelete = new RestRequest("/repos/TeamAlabala/APISolutionAndPostman/issues/comments/" + newComment.id, Method.Delete);
            var responseDelete = await client.ExecuteAsync(requestDelete);

            Assert.AreEqual(HttpStatusCode.NoContent, responseDelete.StatusCode);
        }

        [TestCase("BG", "1000", "Sofija")]
        [TestCase("BG", "8600", "Jambol")]
        [TestCase("CA", "M5S", "Toronto")]
        [TestCase("DE", "01067", "Dresden")]
        [TestCase("GB", "B1", "Birmingham")]
        public async Task TestZipopotamus(string countryCode, string zipCode, string expextedPlace)
        {
            //Arrange
            client = new RestClient("https://api.zippopotam.us/");
            var request = new RestRequest(countryCode + "/" + zipCode);

            //Act
            var responce = await this.client.ExecuteAsync(request, Method.Get);
            var location = new SystemTextJsonSerializer().Deserialize<Location>(responce);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, responce.StatusCode);
            Assert.AreEqual(countryCode, location.Abbreviation);
            Assert.AreEqual(zipCode, location.PostCode);
            StringAssert.Contains(expextedPlace, location.Places[0].PlaceName);
        }
    }
}