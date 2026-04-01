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

    [HttpGet("all")]
    public IActionResult GetAllConfigs()
    {
        try
        {
            var configs = store
                .ListAreas()
                .Select(descriptor =>
                {
                    var config = store.GetConfig(descriptor.Key);
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

    [HttpGet("{areaKey}/schema")]
    public IActionResult GetSchema(string areaKey)
    {
        try
        {
            var descriptor = store.ListAreas().FirstOrDefault(x => x.Key == areaKey);
            if (descriptor is null)
            {
                return NotFound(new { error = $"Unknown area '{areaKey}'." });
            }

            if (string.IsNullOrWhiteSpace(descriptor.SchemaFilePath))
            {
                return Ok(new JsonObject());
            }

            if (!System.IO.File.Exists(descriptor.SchemaFilePath))
            {
                return NotFound(new { error = $"Schema not found for area '{areaKey}'." });
            }

            var schemaJson = System.IO.File.ReadAllText(descriptor.SchemaFilePath);
            var schema = JsonNode.Parse(schemaJson) as JsonObject;
            if (schema is null)
            {
                return Problem(
                    title: "Unable to load area schema.",
                    detail: $"Schema for area '{areaKey}' is not a JSON object."
                );
            }

            return Ok(schema);
        }
        catch (Exception ex)
        {
            return Problem(title: "Unable to load area schema.", detail: ex.Message);
        }
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
