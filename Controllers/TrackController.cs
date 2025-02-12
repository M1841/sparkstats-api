using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using SparkStatsAPI.Utils;

namespace SparkStatsAPI
{
  namespace Controllers
  {
    [Route("[controller]")]
    [ApiController]
    public class TrackController(
      SpotifyClientBuilder builder
    ) : ControllerBase
    {
      [HttpGet("current")]
      public async Task<IActionResult> GetCurrent(
        [FromHeader(Name = "Authorization")] string authHeader)
      {
        try
        {
          var buildResult = _builder.Build(authHeader);
          if (!buildResult.IsSuccess)
          {
            return StatusCode(
              buildResult.Error!.Status,
              buildResult.Error.Message);
          }
          var spotify = buildResult.Ok!;

          var response = await spotify.Player.GetCurrentlyPlaying(
            new PlayerCurrentlyPlayingRequest(
              PlayerCurrentlyPlayingRequest.AdditionalTypes.Track));

          var isPlaying = response?.IsPlaying ?? false;
          var item = response?.Item ?? null;

          if (!isPlaying || item?.Type != ItemType.Track)
          {
            return NoContent();
          }

          var track = (FullTrack)item;

          return Ok(new TrackSimple(
            track.Id,
            track.Name,
            track.ExternalUrls.FirstOrDefault().Value,
            track.Album.Images.LastOrDefault()?.Url,
            SelectArtists(track)));
        }
        catch (APIUnauthorizedException error)
        {
          return Unauthorized(error.Message);
        }
        catch (Exception error)
        {
          return StatusCode(
            StatusCodes.Status500InternalServerError,
            error.Message);
        }
      }

      [HttpGet("history")]
      public async Task<IActionResult> GetHistory(
        [FromHeader(Name = "Authorization")] string authHeader)
      {
        try
        {
          var buildResult = _builder.Build(authHeader);
          if (!buildResult.IsSuccess)
          {
            return StatusCode(
              buildResult.Error!.Status,
              buildResult.Error.Message);
          }
          var spotify = buildResult.Ok!;

          var response = await spotify.Player
            .GetRecentlyPlayed(new PlayerRecentlyPlayedRequest());

          var tracks = new List<TrackSimple>();
          await foreach (var item in spotify.Paginate(response))
          {
            var track = item.Track;
            if (track.Type == ItemType.Track)
            {
              tracks.Add(new TrackSimple(
                track.Id,
                track.Name,
                track.ExternalUrls.FirstOrDefault().Value,
                track.Album.Images.LastOrDefault()?.Url,
                SelectArtists(track)));
              if (tracks.Count == 50) { break; }
            }
          }

          return Ok(tracks.ToArray());
        }
        catch (APIUnauthorizedException error)
        {
          return Unauthorized(error.Message);
        }
        catch (Exception error)
        {
          return StatusCode(
            StatusCodes.Status500InternalServerError,
            error.Message);
        }
      }

      [HttpGet("top")]
      public async Task<IActionResult> GetTop(TimeRange range,
        [FromHeader(Name = "Authorization")] string authHeader)
      {
        try
        {
          var buildResult = _builder.Build(authHeader);
          if (!buildResult.IsSuccess)
          {
            return StatusCode(
              buildResult.Error!.Status,
              buildResult.Error.Message);
          }
          var spotify = buildResult.Ok!;

          var request = new UsersTopItemsRequest(range)
          { Limit = 50 };
          var response = await spotify
            .UserProfile.GetTopTracks(request);

          var paging = PagingAdapter.TrackPages(response);

          var tracks = new List<TrackSimple>();
          await foreach (var track in spotify.Paginate(paging))
          {
            tracks.Add(new TrackSimple(
              track.Id,
              track.Name,
              track.ExternalUrls.FirstOrDefault().Value,
              track.Album.Images.LastOrDefault()?.Url,
              SelectArtists(track)
            ));
            if (tracks.Count == 100) { break; }
          }

          return Ok(tracks.ToArray());
        }
        catch (APIUnauthorizedException error)
        {
          return Unauthorized(error.Message);
        }
        catch (Exception error)
        {
          return StatusCode(
            StatusCodes.Status500InternalServerError,
            error.Message);
        }
      }

      private static ArtistBase[] SelectArtists(FullTrack track)
      {
        return [.. track.Artists.Select(
          artist => new ArtistBase(
            artist.Name,
            artist.ExternalUrls.FirstOrDefault().Value
          ))];
      }

      private readonly SpotifyClientBuilder _builder = builder;
    }
  }
}
