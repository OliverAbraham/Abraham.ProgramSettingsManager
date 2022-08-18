using Abraham.ProgramSettingsManager;

namespace ProgramSettingsManagerDemo;

internal class Configuration
{
    public string Option1   { get; set; }
    public string Option2   { get; set; }
    public string Option3   { get; set; }

    // Note: 
    // You can mark individual properties as "Optional".
    // this will exclude them in "Validate" and "IsValid" methods.
    // like this:
    //
    //    [Optional]
    //    public string Option4   { get; set; }
}
