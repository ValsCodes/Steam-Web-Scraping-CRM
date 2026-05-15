using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;

[assembly: NonParallelizable]
[assembly: WebApplicationFactoryContentRoot(
    "SteamApp.WebAPI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
    "../../../../SteamApp.WebAPI",
    "SteamApp.WebAPI.csproj",
    "0")]
