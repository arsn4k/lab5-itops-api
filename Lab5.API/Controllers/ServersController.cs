using FluentValidation;
using Lab5.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lab5.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServersController : ControllerBase
{
    private readonly IServerService _serverService;
    private readonly IValidator<CreateServerRequest> _createValidator;
    private readonly IValidator<UpdateServerRequest> _updateValidator;
    private readonly IValidator<SetServerStatusRequest> _statusValidator;

    public ServersController(
        IServerService serverService,
        IValidator<CreateServerRequest> createValidator,
        IValidator<UpdateServerRequest> updateValidator,
        IValidator<SetServerStatusRequest> statusValidator)
    {
        _serverService = serverService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
    }

    /// <summary>Повертає список серверів; фільтр за назвою (?name=db).</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServerDto>>> GetAll([FromQuery] string? name)
        => Ok(await _serverService.SearchServersAsync(name ?? string.Empty));

    /// <summary>Повертає сервер за ідентифікатором.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServerDto>> GetById(Guid id)
    {
        var server = await _serverService.GetServerDetailsAsync(id);
        return server is null ? NotFound() : Ok(server);
    }

    /// <summary>Реєструє новий сервер (з валідацією запиту).</summary>
    [HttpPost]
    public async Task<ActionResult<ServerDto>> Create([FromBody] CreateServerRequest request)
    {
        var invalid = await ValidateAsync(_createValidator, request);
        if (invalid is not null) return invalid;

        var created = await _serverService.CreateServerAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Повністю оновлює дані сервера.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServerDto>> Update(
        Guid id, [FromBody] UpdateServerRequest request)
    {
        var invalid = await ValidateAsync(_updateValidator, request);
        if (invalid is not null) return invalid;

        var updated = await _serverService.UpdateServerAsync(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Змінює стан сервера: Online, Offline або Maintenance.</summary>
    /// <param name="id">GUID сервера.</param>
    /// <param name="request">Новий стан.</param>
    /// <returns>204 No Content у разі успіху.</returns>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(
        Guid id, [FromBody] SetServerStatusRequest request)
    {
        var invalid = await ValidateAsync(_statusValidator, request);
        if (invalid is not null) return invalid;

        return await _serverService.SetServerStatusAsync(id, request.Status)
            ? NoContent() : NotFound();
    }

    /// <summary>Видаляє сервер із системи.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _serverService.DeleteServerAsync(id) ? NoContent() : NotFound();

    // Повертає 400 зі структурованим переліком помилок або null, якщо запит коректний.
    private async Task<ActionResult?> ValidateAsync<T>(IValidator<T> validator, T request)
    {
        var result = await validator.ValidateAsync(request);
        if (result.IsValid) return null;

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return ValidationProblem(new ValidationProblemDetails(errors));
    }
}
