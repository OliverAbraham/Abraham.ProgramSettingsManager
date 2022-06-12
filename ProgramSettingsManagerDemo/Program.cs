using Abraham.ProgramSettingsManager;

namespace ProgramSettingsManagerDemo;

/// <summary>
/// Demo of the ProgramSettingsManager Nuget package.
/// This demonstrates how to read the file appsettings.hjson into memory with one line of code.
/// 
/// ATTENTION: 
/// Don't forget to set the Properties of your appsettings.hjson file in your project to "copy if newer".
/// 
/// Author:
/// Oliver Abraham, mail@oliver-abraham.de, https://www.oliver-abraham.de
/// 
/// Source code hosted at: 
/// https://github.com/OliverAbraham/ProgramSettingsManager
/// 
/// Nuget Package hosted at: 
/// https://www.nuget.org/packages/Abraham.ProgramSettingsManager/
/// 
/// </summary>
/// 
internal class Program
{
    private static ProgramSettingsManager<Configuration> _myConfiguration;

    static void Main(string[] args)
    {
        Console.WriteLine("Demo for the Nuget package 'ProgramSettingsManager'");



        // easy version:
        _myConfiguration = new ProgramSettingsManager<Configuration>().Load();
        Console.WriteLine($"A value from my appsettings.hjson file: {_myConfiguration.Data.Option1}");




        //      // use your own filename:
        //      _myConfiguration = new ProgramSettingsManager<Configuration>()
        //          .UseFilename("my_special_appsettings_filename.hjson")
        //          .Load();
        //      
        //      
        //      // use your own folder:
        //      _myConfiguration = new ProgramSettingsManager<Configuration>()
        //          .UseFolder(@"C:\my\special\folder")
        //          .Load();
        //      
        //      
        //      // use your own subfolder in Appdata directory:
        //      _myConfiguration = new ProgramSettingsManager<Configuration>()
        //          .UseAppDataFolder(@"MyCompany")
        //          .Load();
        //      
        //      
        //      // Validate that all settings are present in the file and print out all settings:
        //      _myConfiguration = new ProgramSettingsManager<Configuration>()
        //          .Load()
        //          .Validate()
        //          .PrintConfiguration();
        //      
        //      
        //      // Another variant to print out all configuration values with your own logger:
        //      _myConfiguration = new ProgramSettingsManager<Configuration>()
        //          .Load()
        //          .Validate()
        //          .PrintConfiguration(delegate(string message)
        //      	  {
        //      	  	  Console.WriteLine(message);
        //      	  });
    }
}