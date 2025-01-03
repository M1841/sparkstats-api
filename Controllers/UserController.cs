using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SparkStatsAPI.Utils;

namespace SparkStatsAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class UserController(
  SpotifyClientBuilder builder
) : ControllerBase
{
  [HttpGet("profile")]
  public async Task<IActionResult> GetProfile(
    [FromHeader(Name = "Authorization")] string authHeader)
  {
    try
    {
      var (spotify, error) = _builder.Build(authHeader);
      if (error != null)
      {
        return StatusCode(
          StatusCodes.Status500InternalServerError,
          error);
      }

      var profile = await spotify!.UserProfile.Current();

      return Ok(new UserProfile(
        profile.Id,
        profile.DisplayName,
        profile.ExternalUrls.FirstOrDefault().Value,
        profile.Images.LastOrDefault()?.Url));
    }
    catch (Exception error)
    {
      return StatusCode(
        StatusCodes.Status500InternalServerError,
        error.Message);
    }
  }

  [HttpGet("signout")]
  public async Task<IActionResult> LogOut()
  {
    await HttpContext.SignOutAsync();
    return Redirect("/");
  }

  private readonly SpotifyClientBuilder _builder = builder;
}
