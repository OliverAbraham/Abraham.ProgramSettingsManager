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
/// https://github.com/OliverAbraham/ProgramSettingsManager
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



    #region ------------- Fields --------------------------------------------------------------
	#endregion



	#region ------------- Init ----------------------------------------------------------------
    /// <summary>
    /// Create a static instance (singleton) of your program configuration.
    /// You can use hjson extension so use the Hjson format, otherwise json will be used.
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
        if (!IsValid(Data))
			throw new Exception($"There's an error in your configuration file '{ConfigPathAndFilename}'. Please check this file.");
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

        foreach (var property in properties)
		{
            if (property.Value is string value1 && string.IsNullOrWhiteSpace(value1))
                return false;

            if (property.Value is int value2 && value2 == 0)
                return false;

            if (property.Value is double value3 && value3 == 0)
                return false;

            if (property.Value is decimal value4 && value4 == 0)
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
            logger($"{property.Key,-50}: {property.Value}");
		}

        return this;
	}
	#endregion



	#region ------------- Implementation ----------------------------------------------------------
    private static Dictionary<string, object> DictionaryFromType(object atype)
    {
        if (atype == null) 
            return new Dictionary<string, object>();

        Type t = atype.GetType();
        PropertyInfo[] props = t.GetProperties();
        Dictionary<string, object> dict = new Dictionary<string, object>();
            
        foreach (PropertyInfo prp in props)
        {
            object value = prp.GetValue(atype, new object[]{});
            dict.Add(prp.Name, value);
        }
        return dict;
    }
	#endregion
}