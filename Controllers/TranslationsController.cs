using System.IO;
using System.Text.RegularExpressions;
using Translations.Models;
using Translations.Services;
using Translations.Common.Enums;
using Translations.Common.Utilities;
using Translations.Common.Constants;
using Microsoft.AspNetCore.Mvc;

// Debugging
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Http.Extensions;
using System.Web;

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
        string? text = HttpContext.Request.Query["text"];
        string? gameFranchise = HttpContext.Request.Query["gameFranchise"];
        string? gameName = HttpContext.Request.Query["gameName"];

        IEnumerable<LocalizedText>? localizedTextEntries = await _translationsService.GetAsync(text, gameFranchise, gameName);

        if(localizedTextEntries is null) return NotFound();

        return Ok(localizedTextEntries);
    }

    /**
    * Skims through the file.
    */
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromForm] IFormFile file)
    {
        string? gameFranchise = HttpContext.Request.Query["gameFranchise"];
        string? gameName = HttpContext.Request.Query["gameName"];
        IEnumerable<LocalizedText> localizedTextCollection = await _translationsService.GetAsync();

        var results = await _translationsService.ProcessFileAsync(file, gameName, gameFranchise, localizedTextCollection); 
        
        if(results.FoundTextEntries is null || results.FoundTextEntries.Count == 0) { return NotFound(); }
        
        var zipFile = await _translationsService.GenerateJSONDocumentsAsync(results.FoundTextEntries, localizedTextCollection);
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
        // TODO: Move to services
        var files = formData.Files;
        if(files == null || files.Length == 0) { return BadRequest("No file uploaded"); }

        for(var index = 0; index < files.Length; index++)
        {
            var file = files[index];
            using(var reader = new StreamReader(file.OpenReadStream()))
            {
                var lineIndex = 0;
                while(reader.Peek() >= lineIndex)
                {
                    var text = reader.ReadLine();

                    if(text == "{" || text == "}" || text == null || text == ""){ continue; }

                    var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    var parsedTextEntry = RegexTools.ParseTextEntry(text);
                    var localizedTextEntry = new LocalizedText() 
                    { 
                        Key = parsedTextEntry.Key, 
                        Text = parsedTextEntry.Value,
                        Language = Language.GetLanguageCodeEnum(fileName),
                        GameFranchise = formData.GameFranchise ?? "",
                        GameName = formData.GameName ?? "" 
                    };

                    lineIndex++;

                    await _translationsService.CreateAsync(localizedTextEntry);
                }
            }
        }        

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
