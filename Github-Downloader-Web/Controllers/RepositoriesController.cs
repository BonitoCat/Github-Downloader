using Microsoft.AspNetCore.Mvc;
using Github_Downloader_Web.DTOs;
using Github_Downloader_Web.Services;

namespace Github_Downloader_Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RepositoriesController : ControllerBase
{
    private readonly IGithubDownloaderService _service;
    private readonly ILogger<RepositoriesController> _logger;

    public RepositoriesController(IGithubDownloaderService service, ILogger<RepositoriesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ReposListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReposListResponse>> GetAll()
    {
        var result = await _service.GetAllReposAsync();
        return Ok(result);
    }

    [HttpGet("{repoUrl}")]
    [ProducesResponseType(typeof(RepoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RepoResponse>> Get(string repoUrl)
    {
        var decodedUrl = Uri.UnescapeDataString(repoUrl);
        var repo = await _service.GetRepoAsync(decodedUrl);
        if (repo == null)
            return NotFound(new ErrorResponse("Repository not found", $"No repository found with URL: {decodedUrl}"));
        
        return Ok(repo);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RepoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RepoResponse>> Add([FromBody] AddRepoRequest request)
    {
        try
        {
            var repo = await _service.AddRepoAsync(request.RepoUrl);
            return CreatedAtAction(nameof(Get), new { repoUrl = Uri.EscapeDataString(repo.Url) }, repo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add repository");
            return BadRequest(new ErrorResponse("Failed to add repository", ex.Message));
        }
    }

    [HttpPost("by-name")]
    [ProducesResponseType(typeof(RepoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RepoResponse>> AddByName([FromBody] AddRepoByNameRequest request)
    {
        try
        {
            var repo = await _service.AddRepoAsync(request.PublisherName, request.RepoName);
            return CreatedAtAction(nameof(Get), new { repoUrl = Uri.EscapeDataString(repo.Url) }, repo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add repository by name");
            return BadRequest(new ErrorResponse("Failed to add repository", ex.Message));
        }
    }

    [HttpPut("{repoUrl}")]
    [ProducesResponseType(typeof(RepoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RepoResponse>> Update(string repoUrl, [FromBody] UpdateRepoRequest request)
    {
        var decodedUrl = Uri.UnescapeDataString(repoUrl);
        try
        {
            var repo = await _service.UpdateRepoAsync(decodedUrl, request);
            if (repo == null)
                return NotFound(new ErrorResponse("Repository not found", $"No repository found with URL: {decodedUrl}"));
            
            return Ok(repo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update repository");
            return BadRequest(new ErrorResponse("Failed to update repository", ex.Message));
        }
    }

    [HttpDelete("{repoUrl}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string repoUrl)
    {
        var decodedUrl = Uri.UnescapeDataString(repoUrl);
        var deleted = await _service.DeleteRepoAsync(decodedUrl);
        if (!deleted)
            return NotFound(new ErrorResponse("Repository not found", $"No repository found with URL: {decodedUrl}"));
        
        return NoContent();
    }
}