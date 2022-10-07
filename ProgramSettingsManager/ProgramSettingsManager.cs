using Newtonsoft.Json;
using System.Reflection;
using Hjson;

namespace Abraham.ProgramSettingsManager;

/// <summary>
/// Load and save program settings with your own class, using JSON or HJSON format
/// 
/// ATTENTION: 
/// Don't forget to set the Properties of your appsettings.hjson file in your project to "copy if newer".
/// 
/// Author:
/// Oliver Abraham, mail@oliver-abraham.de, https://www.oliver-abraham.de
/// 
/// Source code hosted at: 
/// https://github.com/OliverAbraham/Abraham.ProgramSettingsManager
/// 
/// Nuget Package hosted at: 
/// https://www.nuget.org/packages/Abraham.ProgramSettingsManager/
/// 
/// </summary>
/// 
/// <typeparam name="T">your class containing your data (typically named Configuration)</typeparam>
/// 
public class ProgramSettingsManager<T> where T : class
{
    #region ------------- Properties ----------------------------------------------------------
    public string ConfigFilename        { get; set; } = "appsettings.hjson";
    public string ConfigPathAndFilename { get; set; }
    public T	  Data                  { get; set; }
    #endregion



    #region ------------- Private types -----------------------------------------------------------
    private class PropertyData
    {
        public string Name { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public object? Obj { get; set; }

        public PropertyData(string name, PropertyInfo propertyInfo, object? obj)
        {
            Name = name;
            PropertyInfo = propertyInfo;
            Obj = obj;
        }
    }
    #endregion



    #region ------------- Init ----------------------------------------------------------------
    /// <summary>
    /// Create an instance of your program configuration.
    /// You can use hjson extension to use the HJSON format, otherwise JSON will be used.
    /// </summary>
    public ProgramSettingsManager()
    {
        ConfigPathAndFilename = Path.Combine(Directory.GetCurrentDirectory(), ConfigFilename);
    }
    #endregion



    #region ------------- Methods -------------------------------------------------------------
    /// <summary>
    /// Call this method to set a certain full path and filename, for example from command line parameters (args[0])
    /// </summary>
    public ProgramSettingsManager<T> UseFullPathAndFilename(string fullpath)
    {
        ConfigFilename = Path.GetFileName(fullpath);
        ConfigPathAndFilename = fullpath;
        return this;
    }

    /// <summary>
    /// Call this method to set a certain path and filename, relative to a known folder
    /// Examples:
    /// %APPLICATIONDATA%\AcmeCompany\Appsettings.json
    /// %LOCALAPPLICATIONDATA%\AcmeCompany\Appsettings.json
    /// %COMMONDOCUMENTS%\MyProgram\Appsettings.json
    /// %MYDOCUMENTS%\MyProgram\Appsettings.json
    /// %TEMP%\MyProgram\Appsettings.json
    /// </summary>
    public ProgramSettingsManager<T> UsePathRelativeToSpecialFolder(string fullpath)
    {
        // you can use some keywords to locate your configuration file relative to a special "known windows folder"
        ReplaceVariable(ref fullpath, "%APPLICATIONDATA%"     , Environment.SpecialFolder.ApplicationData);
        ReplaceVariable(ref fullpath, "%LOCALAPPLICATIONDATA%", Environment.SpecialFolder.LocalApplicationData);
        ReplaceVariable(ref fullpath, "%COMMONDOCUMENTS%"     , Environment.SpecialFolder.CommonDocuments);
        ReplaceVariable(ref fullpath, "%MYDOCUMENTS%"         , Environment.SpecialFolder.MyDocuments);
        ReplaceVariable(ref fullpath, "%TEMP%"                , Path.GetTempPath());

        ConfigFilename = Path.GetFileName(fullpath);
        ConfigPathAndFilename = fullpath;
        return this;
    }

    /// <summary>
    /// Call this method to set a certain filename, if you don't want appsettings.hjson
    /// </summary>
    public ProgramSettingsManager<T> UseFilename(string filename)
    {
        ConfigFilename = filename;
        return this;
    }

    /// <summary>
    /// Call this method to set the appdata folder as storage, default is the current folder
    /// </summary>
    public ProgramSettingsManager<T> UseAppDataFolder()
    {
        string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        ConfigPathAndFilename = Path.Combine(appDataFolder, ConfigFilename);
        return this;
    }

    /// <summary>
    /// Call this method to set the appdata folder as storage, default is the current folder
    /// </summary>
    public ProgramSettingsManager<T> UseAppDataFolder(string mySubdirectory)
    {
        string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        ConfigPathAndFilename = Path.Combine(appDataFolder, mySubdirectory, ConfigFilename);
        return this;
    }

    /// <summary>
    /// Call this method to set a special folder as storage
    /// </summary>
    public ProgramSettingsManager<T> UseFolder(string folder)
    {
        ConfigPathAndFilename = Path.Combine(folder, ConfigFilename);
        return this;
    }

    public ProgramSettingsManager<T> Load()
    {
        try
        {
            Data = Load(ConfigPathAndFilename);
            if (Data is null)
                throw new Exception($"Unable to read the configuration file '{ConfigPathAndFilename}'");
            return this;
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to read the configuration file '{ConfigPathAndFilename}'.More Info: {ex}");
        }
    }

    /// <summary>
    /// Call this method to validate every property of your data is filled
    /// You can exclude a Property with the [Optional] attribute.
    /// </summary>
    /// 
    /// <returns>
    /// true, if all properties are filled 
    /// (string not null or whitespace, numeric data types not zero)
    /// </returns>
    public ProgramSettingsManager<T> Validate()
    {
        var properties = DictionaryFromType(Data);
        var t = Data.GetType();

        foreach (var property in properties)
        {
            var propertyAttributes = property.Value.PropertyInfo.GetCustomAttributes(typeof(OptionalAttribute));
            var propertyIsOptional = propertyAttributes.Where(x => x is OptionalAttribute).Any();

            if (!propertyIsOptional && !PropertyHasValue(property))
                throw new Exception($"There's an error in your configuration file '{ConfigPathAndFilename}'. There's no value for property '{property.Value.Name}'");
        }

        return this;
    }

    /// <summary>
    /// Call this method to validate every property of your data is filled
    /// You can exclude a Property with the [Optional] attribute.
    /// </summary>
    /// 
    /// <returns>
    /// true, if all properties are filled 
    /// (string not null or whitespace, numeric data types not zero)
    /// </returns>
    public bool IsValid(T data)
    {
        var properties = DictionaryFromType(data);
        var t = data.GetType();

        foreach (var property in properties)
        {
            var propertyAttributes = property.Value.PropertyInfo.GetCustomAttributes(typeof(OptionalAttribute));
            var propertyIsOptional = propertyAttributes.Where(x => x is OptionalAttribute).Any();

            if (!propertyIsOptional && !PropertyHasValue(property))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Call this method to load a file into memory
    /// </summary>
    public T Load(string filename)
    {
        if (!File.Exists(filename))
            throw new Exception($"Read error! The file named {filename} does not exist");

        string jsonString;
        if (Path.GetExtension(filename) == ".hjson")
            jsonString = HjsonValue.Load(filename).ToString();
        else
            jsonString = File.ReadAllText(filename);

        return JsonConvert.DeserializeObject<T>(jsonString);
    }

    /// <summary>
    /// Call this method to save your data back to the file we read from.
    /// </summary>
    public void Save(T data)
    {
        var json = JsonConvert.SerializeObject(data);

        HjsonValue.Save(json, ConfigPathAndFilename, new HjsonOptions(){EmitRootBraces = false });

        var Temp = File.ReadAllText(ConfigPathAndFilename);
        Temp = Temp.Trim('\'');
        File.WriteAllText(ConfigPathAndFilename, Temp);
    }

    /// <summary>
    /// Call this method to print out all configuration settings to the console (or your preferred logger)
    /// </summary>
    public ProgramSettingsManager<T> PrintConfiguration()
    {
        PrintConfiguration(Console.WriteLine);
        return this;
    }

    /// <summary>
    /// Call this method to print out all configuration settings to the console (or your preferred logger)
    /// </summary>
    public ProgramSettingsManager<T> PrintConfiguration(Action<string> logger)
    {
        logger("Configuration:");

        var properties = DictionaryFromType(Data);
        foreach (var property in properties)
        {
            logger($"{property.Key,-50}: {property.Value.Obj}");
        }

        return this;
    }
    #endregion



    #region ------------- Implementation ----------------------------------------------------------
    private static Dictionary<string, PropertyData> DictionaryFromType(object atype)
    {
        var results = new Dictionary<string, PropertyData>();
        if (atype == null) 
            return results;

        Type t = atype.GetType();
        PropertyInfo[] properties = t.GetProperties();
            
        foreach (var property in properties)
        {
            object? value = property.GetValue(atype, new object[]{});
            var data = new PropertyData(property.Name, property, value);
            results.Add(property.Name, data);
        }
        return results;
    }
 
    private bool PropertyHasValue(KeyValuePair<string, ProgramSettingsManager<T>.PropertyData> property)
    {
        if (property.Value.Obj is string value1 && string.IsNullOrWhiteSpace(value1))
            return false;

        if (property.Value.Obj is int value2 && value2 == 0)
            return false;

        if (property.Value.Obj is double value3 && value3 == 0)
            return false;

        if (property.Value.Obj is decimal value4 && value4 == 0)
            return false;

        return true;
    }
 
    private void ReplaceVariable(ref string path, string keyword, Environment.SpecialFolder folderCode)
    {
        ReplaceVariable(ref path, keyword, Environment.GetFolderPath(folderCode));
    }
 
    private void ReplaceVariable(ref string path, string keyword, string pathToUse)
    {
        if (path.Contains(keyword))
            path = path.Replace(keyword, pathToUse);
    }
    #endregion
}