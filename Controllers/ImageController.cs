using Microsoft.AspNetCore.Mvc;
using Rekognition_Client.Models;

public class ImageController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ImageController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult UploadImage()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No se ha subido ninguna imagen.");
        }

        using var memStream = new MemoryStream();
        await file.CopyToAsync(memStream);
        var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(memStream.ToArray()), "file", file.FileName }
        };

        var client = _httpClientFactory.CreateClient("RekognitionApi");
        var response = await client.PostAsync("detect-labels", content);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Error al procesar la imagen.");
        }

        var labels = await response.Content.ReadFromJsonAsync<List<LabelResult>>();
        return View("Result", labels);
    }
}
