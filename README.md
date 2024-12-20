# SparkStats API

I'm trying to migrate SparkStats' server-side logic to a separate web service to make my future plans for the projects easier, and using C# because why not

## Prerequisites

- [Spotify developer account](https://developer.spotify.com/)

And either one of:

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or newer
- [Docker](https://docs.docker.com/engine/install/)

## Configuration

Create an app in your Spotify developer dashboard

Add `http://localhost:8080/auth/callback` to redirect URIs

Create a `.env` file with the client ID and secret

```
SPOTIFY_CLIENT_ID=your-client-id
SPOTIFY_CLIENT_SECRET=your-client-secret
```

## Running the program

- as a regular .NET app (must have .NET 8 installed)

```bash
./start
```

- same as above but with hot-reloading enabled

```bash
./start -w # or ./start --watch
```

- as a Docker container (must have Docker installed)

```bash
./start -d # or ./start --docker
```
