using MovieCatalogApiTests.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace MovieCatalogApiTests
{
    public class MovieCatalogApiTests
    {
        private RestClient client;
        private static string createdMovieId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("zgeorgieva@gmail.com", "asdfgh");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:5000")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);
        }
        private string GetJwtToken(string email, string password)
        {
            RestClient client = new RestClient("http://144.91.123.158:5000");
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateMovie_WithRequiredFields_ShouldReturnSuccess()
        {
            MovieDto movie = new MovieDto
            {
                Title = "The Matrix",
                Description = "The Matrix (1999) is a seminal science-fiction action film",
            };
            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            ApiResponseDto responseBody = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(responseBody.Movie, Is.Not.Null);
            Assert.That(responseBody.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(responseBody.Msg, Is.EqualTo("Movie created successfully!"));
            createdMovieId = responseBody.Movie.Id;
        }

        [Order(2)]
        [Test]
        public void EditMovie_ShouldChangeTitleAndDescription()
        {
            MovieDto movie = new MovieDto
            {
                Title = "The Matrix Reloaded",
                Description = "The Matrix Reloaded (2003) is a seminal science-fiction action film",
            };
            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId);
            request.AddJsonBody(movie);
            RestResponse response = client.Execute(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            ApiResponseDto responseBody = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(responseBody.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnNonEmptyArray()
        {
            RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            List<MovieDto> responseBody = JsonSerializer.Deserialize<List<MovieDto>>(response.Content);
            Assert.That(responseBody, Is.Not.Empty);
            Assert.That(responseBody.Count, Is.GreaterThanOrEqualTo(1));
        }

        [Order(4)]
        [Test]
        public void DeleteCreatedMovie_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            ApiResponseDto responseBody = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(responseBody.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            MovieDto movie = new MovieDto
            {
                Title = "",
                Description = "",
            };
            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "08974127983";
            MovieDto movie = new MovieDto
            {
                Title = "The Matrix Reloaded",
                Description = "The Matrix Reloaded (2003) is a seminal science-fiction action film",
            };
            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            request.AddJsonBody(movie);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            ApiResponseDto responseBody = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(responseBody.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            string nonExistingMovieId = "08974127983";
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            ApiResponseDto responseBody = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(responseBody.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}