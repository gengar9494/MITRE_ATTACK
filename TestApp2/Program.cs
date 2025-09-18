using LibGit2Sharp;

namespace TestApp2;

class Program
{
    public static string AppDataPath { get; set; } = string.Empty;
    public static string PayloadsPath { get; set; } = string.Empty;
    public static string AtomicTestsPath { get; set; } = string.Empty;
    
    public static string AtomicInvokePath { get; set; } = string.Empty;
    
    static void Main(string[] args)
    {
        InitializeApplicationFolder();
        
        Step2();
    }

    private static void Step1()
    {
        var repoUrl = "https://github.com/redcanaryco/atomic-red-team.git";

        Console.WriteLine("Cloning repository...");
        Repository.Clone(repoUrl, AtomicTestsPath);
    }
    
    private static void Step2()
    {
        var repoUrl = "https://github.com/redcanaryco/invoke-atomicredteam.git";

        Console.WriteLine("Cloning repository...");
        Repository.Clone(repoUrl, AtomicInvokePath);
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
        
        var atomicInvokePath = Path.Combine(appDataPath, "atomic-invoke");
        
        if (!Directory.Exists(atomicInvokePath))
        {
            Directory.CreateDirectory(atomicInvokePath);
        }
        
        AppDataPath = appDataPath;
        PayloadsPath = payloadsPath;
        AtomicTestsPath = atomicTestsPath;
        AtomicInvokePath = atomicInvokePath;
    }
}