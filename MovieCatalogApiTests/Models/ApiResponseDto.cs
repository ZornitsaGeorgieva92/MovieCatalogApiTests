using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MovieCatalogApiTests.Models;

public class ApiResponseDto
{
    [JsonPropertyName("msg")]
    public string Msg { get; set; }

    [JsonPropertyName("movie")]
    public MovieDto Movie { get; set; } = new MovieDto();
}
