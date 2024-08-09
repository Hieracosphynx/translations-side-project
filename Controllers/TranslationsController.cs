using Translations.Models;
using Translations.Services;
using Translations.Common.Types;
using Microsoft.AspNetCore.Mvc;

namespace Translations.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationsController : ControllerBase
{
    private readonly TranslationsService _translationsService;

    public TranslationsController(TranslationsService translationsService) =>
        _translationsService = translationsService;

    [HttpGet]
    public async Task<List<LocalizedText>> Get() =>
        await _translationsService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<LocalizedText>> Get(string id)
    {
        var localizedText = await _translationsService.GetAsync(id);

        if(localizedText is null) return NotFound();

        return localizedText;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<LocalizedText>>> Search()
    {
        string? gameFranchise = HttpContext.Request.Query["gameFranchise"];
        string? gameName = HttpContext.Request.Query["gameName"];
        string? text = HttpContext.Request.Query["text"];

        SearchTextContext textContext = new(gameFranchise, gameName, text);

        IEnumerable<LocalizedText>? localizedTextEntries = await _translationsService.GetAsync(textContext);

        if(localizedTextEntries is null) return NotFound();

        return Ok(localizedTextEntries);
    }

    /// <summary>
    /// Reads the file given. This will return found and not found text entries for each languages.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<LocalizedText.FileAndContent>>> Search([FromForm] IFormFile file)
    {
        string? gameFranchise = HttpContext.Request.Query["gameFranchise"];
        string? gameName = HttpContext.Request.Query["gameName"];
        IEnumerable<LocalizedText> localizedTextCollection = await _translationsService.GetAsync();

        SearchFileContext fileContext = new(gameFranchise, gameName, file);

        var results = await _translationsService.ProcessFileAsync(fileContext, localizedTextCollection); 
        
        if(results is null) { return NotFound(); }
        
        var jsonResults = _translationsService.GenerateJSONDocumentsAsync(results.FoundTextEntries, localizedTextCollection);
        
        return Ok(jsonResults);
    }

    /// <summary>
    /// Generates the zip file using the results from POST /search. 
    /// </summary>
    /// <param name="results">Results that came from POST /search</param>
    /// <returns>Zip file that contains json files.</returns>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate(IEnumerable<LocalizedText.FileAndContent> results)
    {
        string? gameFranchise = HttpContext.Request.Query["gameFranchise"];
        string? gameName = HttpContext.Request.Query["gameName"];

        var zipFile = await _translationsService.GenerateZipFileAsync(results);
        var zipFilename = DateTime.Now.ToString() + "_" + gameFranchise + "_" + gameName;
        var zipFilepath = Path.ChangeExtension(zipFilename, ".zip");

        return File(zipFile, "application/zip", zipFilepath);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromForm] LocalizedText newLocalizedText)
    {
        await _translationsService.CreateAsync(newLocalizedText);

        return CreatedAtAction(nameof(Get), new { id = newLocalizedText.Id }, newLocalizedText);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] LocalizedText.FormData formData)
    {
        var files = formData.Files;

        if(files == null || files.Length == 0) { return BadRequest("No file uploaded"); }

        await _translationsService.UploadAsync(formData);

        return Ok(new { message = "Successfully uploaded" });
    }


    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, LocalizedText updatedLocalizedText)
    {
        var localizedText = await _translationsService.GetAsync(id);

        if(localizedText is null)
        {
            return NotFound();
        }

        updatedLocalizedText.Id = localizedText.Id;

        await _translationsService.UpdateAsync(id, updatedLocalizedText);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        var localizedText = await _translationsService.GetAsync(id);

        if(localizedText is null)
        {
            return NotFound();
        }

        await _translationsService.RemoveAsync(id);

        return NoContent();
    }
}
