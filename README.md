# AspNetCore.AsyncInitialization

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


2. Create a class (or several) that implements `IAsyncInitializer`. This class can depend on any non-scoped service.

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

3. Register your initializer(s) in the `Startup.ConfigureServices` method:

    ```csharp
        services.AddAsyncInitializer<MyAppInitializer>();
    ```

4. In the `Program` class, make the `Main` method async and change its code to initialize the host before running it:

    ```csharp
    public static async Task Main(string[] args)
    {
        var host = CreateWebHostBuilder(args).Build();
        await host.InitAsync();
        host.Run();
    }
    ```

(Note that you need to [set the C# langage version to 7.1 or higher in your project](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version#edit-the-csproj-file) to enable the "async Main" feature.)

This will run each initializer, in the order in which they were registered.
