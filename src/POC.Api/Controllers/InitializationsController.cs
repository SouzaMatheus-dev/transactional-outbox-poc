using Microsoft.AspNetCore.Mvc;
using POC.Application.Commands;
using POC.Application.Handlers;

namespace POC.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class InitializationsController : ControllerBase
{
    private readonly CreateInitializationHandler _handler;
    private readonly ILogger<InitializationsController> _logger;

    public InitializationsController(CreateInitializationHandler handler, ILogger<InitializationsController> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova Inicialização. Persiste o aggregate e o evento na Outbox na mesma transação.
    /// CorrelationId pode vir no header X-Correlation-Id ou é gerado pelo middleware.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateInitializationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInitializationRequest request, CancellationToken ct)
    {
        var correlationId = HttpContext.Response.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;
        _logger.LogInformation("POST /initializations - ExternalId={ExternalId}, CorrelationId={CorrelationId}", request?.ExternalId, correlationId);

        if (string.IsNullOrWhiteSpace(request?.ExternalId))
            return BadRequest(new { error = "ExternalId é obrigatório." });

        try
        {
            var command = new CreateInitializationCommand(request.ExternalId.Trim(), correlationId);
            var result = await _handler.HandleAsync(command, ct);
            _logger.LogInformation("Initialization criada - Id={Id}, CorrelationId={CorrelationId}, persist+outbox na mesma transação", result.Id, correlationId);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém uma inicialização por Id (opcional, para testes).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        // Requer IInitializationRepository; podemos injetar e buscar
        return Ok(new { id, message = "Use repositório se precisar retornar a entidade." });
    }
}

public record CreateInitializationRequest(string ExternalId);
