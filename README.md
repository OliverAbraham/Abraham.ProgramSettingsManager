# Abraham.ProgramSettingsManager

## OVERVIEW

Enables you to use a JSON file for configuration of your app
(typically appsettings.hjson or appsettings.json).



## INSTALLATION

Install the Nuget package "Abraham.ProgramSettingsManager" into you application (from https://www.nuget.org).

Add the following code:

```c#
    private static ProgramSettingsManager<Configuration> _myConfiguration;

    static void Main(string[] args)
    {
        // easy version:
        _myConfiguration = new ProgramSettingsManager<Configuration>().Load();
        Console.WriteLine($"A value from my appsettings.hjson file: {_myConfiguration.Data.Option1}");

    . . . your code
    }

    class Configuration
    {
	    public string Option1 { get; set; }
	    public string Option2 { get; set; }
	    public string Option3 { get; set; }
    }
```

Add a file named "appsettings.hjson" to your project, make sure it's 
copied to the output directory (bin directory) of your app, with this content:

```json
    {
	    Option1: "my value 1",
	    Option2: "my value 2",
	    Option3: "my value 3",
    }
```

That's it!

For more options, please refer to my Demo application in the github repository (see below).
The Demo and the nuget source code is well documented.



## HOW TO INSTALL A NUGET PACKAGE
This is very simple:
- Start Visual Studio (with NuGet installed) 
- Right-click on your project's References and choose "Manage NuGet Packages..."
- Choose Online category from the left
- Enter the name of the nuget package to the top right search and hit enter
- Choose your package from search results and hit install
- Done!


or from NuGet Command-Line:

    Install-Package Abraham.ProgramSettingsManager





## AUTHOR

Oliver Abraham, mail@oliver-abraham.de, https://www.oliver-abraham.de

Please feel free to comment and suggest improvements!



## SOURCE CODE

The source code is hosted at:

https://github.com/OliverAbraham/Abraham.ProgramSettingsManager

The Nuget Package is hosted at: 

https://www.nuget.org/packages/Abraham.ProgramSettingsManager
