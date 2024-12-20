using Microsoft.AspNetCore.Authentication.Cookies;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;
using SparkStatsAPI.Utils;

DotEnv.Load(Path.Combine(
  Directory.GetCurrentDirectory(),
  ".env"));

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddHttpContextAccessor();
services.AddSingleton(
  SpotifyClientConfig
  .CreateDefault()
  .WithHTTPLogger(new SimpleConsoleHTTPLogger())
);
services.AddScoped<SpotifyClientBuilder>();

services.AddAuthorizationBuilder()
  .AddPolicy("Spotify", policy =>
  {
    policy.AuthenticationSchemes.Add("Spotify");
    policy.RequireAuthenticatedUser();
  });

services
  .AddAuthentication(options =>
  {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
  })
  .AddCookie(options =>
  {
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
  })
  .AddSpotify(options =>
  {
    options.ClientId = Environment
      .GetEnvironmentVariable("SPOTIFY_CLIENT_ID")!;
    options.ClientSecret = Environment
      .GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET")!;
    options.CallbackPath = "/auth/callback";
    options.SaveTokens = true;

    var scopes = new List<string> {
      Scopes.PlaylistModifyPrivate,
      Scopes.PlaylistModifyPublic,
      Scopes.PlaylistReadCollaborative,
      Scopes.PlaylistReadPrivate,
      Scopes.UserReadCurrentlyPlaying,
      Scopes.UserReadEmail,
      Scopes.UserReadPrivate,
      Scopes.UserReadRecentlyPlayed,
      Scopes.UserTopRead
    };
    Console.WriteLine(scopes);
    options.Scope.Add(string.Join(",", scopes));
  });

services.AddControllers();
services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
