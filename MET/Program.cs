using Spectre.Console;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MET;

class Program
{
    public static string AppDataPath { get; set; } = string.Empty;
    public static string PayloadsPath { get; set; } = string.Empty;
    public static string AtomicsPath { get; set; } = string.Empty;
    private static MitigationTool MitigationTool { get; } = new MitigationTool();
    private static AtomicTestTool AtomicTestTool { get; } = new AtomicTestTool();
    
    static async Task Main(string[] args)
    {
        InitializeApplicationFolder();
        
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[green]MITRE Excel Tool[/]");
            AnsiConsole.MarkupLine("");
            
            var operation = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Operation")
                    .PageSize(100)
                    .AddChoiceGroup("MITRE ATT&CK Navigator", new[]
                        {
                            "00. Download last 5 MITRE Enterprise STIX JSONs",
                            "01. Create MITRE Mitigation Excel File",
                            "02. Update MITRE Mitigation Excel File",
                            "03. Create MITRE ATT&CK Navigator Layers"
                        })
                    .AddChoiceGroup("MITRE Atomic Test",new[]
                    {
                        "10. Clone MITRE Atomic Red Team Repository",
                        "11. Create MITRE Atomic Test Excel File",
                    })
                    .AddChoiceGroup("Tools",new[]
                    {
                        "90. Open Application Folder (Explorer/Finder)",
                        "Quit",
                    }));
            
            switch (operation.Substring(0, 2))
            {
                case "00":
                    await MitigationTool.DownloadAllMitreFiles();
                    break;
                case "01":
                    MitigationTool.CreateMitreMitigationFile();
                    break;
                case "02":
                    MitigationTool.UpdateMitreMitigationFile();
                    break;
                case "03":
                    MitigationTool.CreateMitreNavigatorLayers();
                    break;
                case "10":
                    AtomicTestTool.CloneAtomicTestRepo();
                    break;
                case "11":
                    AtomicTestTool.CreateMitreAtomicTestFile();
                    break;
                case "90":
                    OpenApplicationFolder();
                    break;
                default:
                    return;
            }
        }
    }
    


    private static void OpenApplicationFolder()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("explorer", AppDataPath)
            {
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start(new ProcessStartInfo("open")
            {
                Arguments = $"\"{AppDataPath}\"",
                UseShellExecute = false
            });
        }
    }
    
    private static void InitializeApplicationFolder()
    {
        // AnsiConsole.MarkupLine("[grey]Initialize Application Folder[/]");

        var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDataPath = Path.Combine(localAppDataPath, "MET");
        
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }
        
        var payloadsPath = Path.Combine(appDataPath, "payloads");
        
        if (!Directory.Exists(payloadsPath))
        {
            Directory.CreateDirectory(payloadsPath);
        }
        
        var atomicTestsPath = Path.Combine(appDataPath, "atomic-tests");
        
        if (!Directory.Exists(atomicTestsPath))
        {
            Directory.CreateDirectory(atomicTestsPath);
        }
        
        AppDataPath = appDataPath;
        PayloadsPath = payloadsPath;
        AtomicsPath = atomicTestsPath;
    }
}

public static class HttpClientUtils
{
    public static async Task DownloadFileTaskAsync(this HttpClient client, string uri, string filePath)
    {
        try
        {
            using (var stream = await client.GetStreamAsync(uri))
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
        }
    }
}