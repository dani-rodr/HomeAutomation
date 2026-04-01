using System.Linq;
using System.Text.Json.Nodes;
using HomeAutomation.apps.Common.Settings;
using Microsoft.AspNetCore.Mvc;

namespace HomeAutomation.Web.Controllers;

[ApiController]
[Route("api/area-config")]
public sealed class AreaConfigController(IAreaSettingsStore store) : ControllerBase
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

    [HttpGet("all")]
    public IActionResult GetAllConfigs()
    {
        try
        {
            var configs = store
                .ListAreas()
                .Select(descriptor =>
                {
                    var config = store.GetSettings(descriptor.Key);
                    return new
                    {
                        key = descriptor.Key,
                        name = descriptor.Name,
                        description = descriptor.Description,
                        config,
                    };
                })
                .OrderBy(static x => x.name)
                .ToList();

            return Ok(configs);
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to load area configs.", detail: ex.Message);
        }
    }

    [HttpGet("{areaKey}")]
    public IActionResult GetConfig(string areaKey)
    {
        try
        {
            var config = store.GetSettings(areaKey);
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
            var result = store.SaveSettings(areaKey, config);
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
            var config = store.ResetSettings(areaKey);
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
