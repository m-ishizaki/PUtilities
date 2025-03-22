using RKSoftware.IPUtilities.AddOns;
using System.Reflection;

var executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location)!;
var addonDirectory = Path.Combine(executingDirectory, "Addons");

if ((!Directory.Exists(addonDirectory)))
{
    try { Directory.CreateDirectory(addonDirectory); }
    catch { }
}

var files = Directory.GetFiles(addonDirectory, "*.dll");
var addons = new List<IAddon>();

{
    addons.Add(new RKSoftware.DllVersionOverride.Addon());
}
foreach (var file in files)
{
    var assembly = Assembly.LoadFrom(file);
    var types = assembly.GetTypes().Where(type => type.GetInterfaces().Contains(typeof(IAddon)));
    foreach (var type in types)
    {
        var ins = (IAddon?)Activator.CreateInstance(type);
        if (ins != null) addons.Add(ins);
    }
}

if (args.Length == 0)
{
    Console.WriteLine("Usage: PUtilities <command> [arguments]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    foreach (var addon in addons)
    {
        Console.WriteLine($"{addon.Command} - {addon.Description} - UtilityName: {addon.Name} {addon.Version}");
    }
    Console.WriteLine();
    return;
}

{
    var addon = addons.FirstOrDefault(addon => addon.Command == args[0]);
    if (addon == null)
    {
        Console.WriteLine("Command not found");
        return;
    }

    {
        addon.Initialize();
        addon.Execute(args.Skip(1).ToArray());
    }
}
