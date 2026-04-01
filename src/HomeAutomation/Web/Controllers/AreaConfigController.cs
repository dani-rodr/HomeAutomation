using System.Linq;
using System.Text.Json.Nodes;
using HomeAutomation.apps.Common.Config;
using Microsoft.AspNetCore.Mvc;

namespace HomeAutomation.Web.Controllers;

[ApiController]
[Route("api/area-config")]
public sealed class AreaConfigController(IAreaConfigStore store) : ControllerBase
{
    [HttpGet]
    public IActionResult ListAreas()
    {
        var areas = store
            .ListAreas()
            .Select(descriptor => new
            {
                key = descriptor.Key,
                name = descriptor.Name,
                description = descriptor.Description,
            })
            .OrderBy(static descriptor => descriptor.name)
            .ToList();

        return Ok(areas);
    }

    [HttpGet("{areaKey}")]
    public IActionResult GetConfig(string areaKey)
    {
        try
        {
            var config = store.GetConfig(areaKey);
            return Ok(config);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Unknown area '{areaKey}'." });
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to load area config.", detail: ex.Message);
        }
    }

    [HttpPut("{areaKey}")]
    public IActionResult SaveConfig(string areaKey, [FromBody] JsonObject config)
    {
        try
        {
            var result = store.SaveConfig(areaKey, config);
            if (!result.IsValid)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(new { message = "Saved." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Unknown area '{areaKey}'." });
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to save area config.", detail: ex.Message);
        }
    }

    [HttpPost("{areaKey}/reset")]
    public IActionResult ResetConfig(string areaKey)
    {
        try
        {
            var config = store.ResetConfig(areaKey);
            return Ok(config);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Unknown area '{areaKey}'." });
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to reset area config.", detail: ex.Message);
        }
    }
}
