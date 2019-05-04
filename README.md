# AspNetCore.AsyncInitialization

[![NuGet version](https://img.shields.io/nuget/v/AspNetCore.AsyncInitialization.svg)](https://www.nuget.org/packages/AspNetCore.AsyncInitialization)
[![AppVeyor build](https://img.shields.io/appveyor/ci/thomaslevesque/aspnetcore-asyncinitialization.svg)](https://ci.appveyor.com/project/thomaslevesque/aspnetcore-asyncinitialization)
[![AppVeyor tests](https://img.shields.io/appveyor/tests/thomaslevesque/aspnetcore-asyncinitialization.svg)](https://ci.appveyor.com/project/thomaslevesque/aspnetcore-asyncinitialization/build/tests)

A simple helper to perform async application initialization in ASP.NET Core 2.0 or higher.

## Usage

1. Install the [AspNetCore.AsyncInitialization](https://www.nuget.org/packages/AspNetCore.AsyncInitialization/) NuGet package:

    Command line:

    ```PowerShell
    dotnet add package AspNetCore.AsyncInitialization
    ```

    Package manager console:
    ```PowerShell
    Install-Package AspNetCore.AsyncInitialization
    ```


1. Create a class (or several) that implements `IAsyncInitializer`. This class can depend on any registered service.

    ```csharp
    public class MyAppInitializer : IAsyncInitializer
    {
        public MyAppInitializer(IFoo foo, IBar bar)
        {
            ...
        }

        public async Task InitializeAsync()
        {
            // Initialization code here
        }
    }
    ```

1. Register your initializer(s) in the `Startup.ConfigureServices` method:

    ```csharp
        services.AddAsyncInitializer<MyAppInitializer>();
    ```

1. In the `Program` class, make the `Main` method async and change its code to initialize the host before running it:

    ```csharp
    public static async Task Main(string[] args)
    {
        var host = CreateWebHostBuilder(args).Build();
        await host.InitAsync();
        host.Run();
    }
    ```

(Note that you need to [set the C# language version to 7.1 or higher in your project](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version#edit-the-csproj-file) to enable the "async Main" feature.)

This will run each initializer, in the order in which they were registered.
