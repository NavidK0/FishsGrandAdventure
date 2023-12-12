using System.IO.Compression;
using System.Runtime.CompilerServices;
using FishsGrandAdventure;
using Newtonsoft.Json;

const string zipFileName = "LC-NavidK0-FishsGrandAdventure";

var projectDir = $"{GetThisFilePath()}/..";

var manifestObject = new
{
    name = ModInfo.Name,
    author = "NavidK0",
    version_number = ModInfo.Version,
    description = "Private mod for the Fish Clan. Adds a bunch of \"special\" content to the game.",
    dependencies = new[] { "2018-LC_API-2.1.2" },
};

string jsonObject = JsonConvert.SerializeObject(manifestObject, Formatting.Indented);

File.WriteAllText($"{projectDir}/manifest.json", jsonObject);

Console.WriteLine($"Packing {manifestObject.name} v{manifestObject.version_number}...");

var zipPath = $"{projectDir}/../{zipFileName}v{manifestObject.version_number}.zip";

File.Delete(zipPath);
ZipFile.CreateFromDirectory($"{projectDir}/plugins", zipPath, CompressionLevel.Optimal, true);

using ZipArchive zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

var toCopy = new List<string>
{
    $"{projectDir}/manifest.json",
    $"{projectDir}/icon.png",
    $"{projectDir}/README.md",
    $"{projectDir}/../bin/Debug/net46/FishsGrandAdventure.dll",
    $"{projectDir}/../bin/Debug/net46/FishsGrandAdventure.pdb",
};

foreach (string path in toCopy)
{
    ZipArchiveEntry manifestEntry = zipArchive.CreateEntry(Path.GetFileName(path));
    using Stream manifestStream = manifestEntry.Open();
    manifestStream.Write(File.ReadAllBytes(path));
    manifestStream.Close();
}


Console.WriteLine("Done!");
return;

static string GetThisFilePath([CallerFilePath] string path = "")
{
    return path;
}