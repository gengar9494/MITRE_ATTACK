using System.Text.Json;
using MET.Models;
using MITREModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Spectre.Console;
using Color = System.Drawing.Color;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MET;

class Program
{
    public static string AppDataPath { get; set; } = string.Empty;
    private static Dictionary<string, CourseOfAction> CourseOfActions { get; set; } =
        new Dictionary<string, CourseOfAction>();
    private static List<Relationship> Relationships { get; set; } = 
        new List<Relationship>();
    private static Dictionary<string, AttackPattern> AttackPatterns { get; set; } =
        new Dictionary<string, AttackPattern>();
    
    private static List<Platform> Platforms { get; set; } = 
        new List<Platform>();
    
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
                    .PageSize(10)
                    .AddChoices(new[]
                    {
                        "00. Download all MITRE Enterprise STIX JSONs",
                        "01. Create MITRE Excel File",
                        "02. Update MITRE Excel File",
                        "03. Open Application Folder (Explorer/Finder)",
                        "Quit",
                    }));
            
            switch (operation.Substring(0, 2))
            {
                case "00":
                    await DownloadAllMitreFiles();
                    break;
                case "01":
                    CreateMitreFile();
                    break;
                case "02":
                    UpdateMitreFile();
                    break;
                case "03":
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

    private static void CreateMitreFile()
    {
        var stixFile = SelectStixFile();
        var excelFileName = AnsiConsole.Prompt(
            new TextPrompt<string>("Excel Filename:").DefaultValue(stixFile.Name.Replace(".json", ".xlsx")));
        var excelFile = new FileInfo(Path.Combine(AppDataPath, excelFileName));
        
        ReadStixFile(stixFile);
        CreateMitreFile(excelFile);
    }
    
    private static void UpdateMitreFile()
    {
        var oldExcelFile = SelectExcelFile();
        var stixFile = SelectStixFile();
        var newExcelFileName = AnsiConsole.Prompt(
            new TextPrompt<string>("Excel Filename:").DefaultValue(stixFile.Name.Replace(".json", ".xlsx")));
        var newExcelFile = new FileInfo(Path.Combine(AppDataPath, newExcelFileName));
        
        ReadStixFile(stixFile);
        ReadMitigations(oldExcelFile);
        CreateMitreFile(newExcelFile);
    }

    private static void ReadMitigations(FileInfo excelFile)
    {
        using (var package = new ExcelPackage(excelFile))
        {
            var ws = package.Workbook.Worksheets["METADATA"];

            foreach (var platform in Platforms)
                ReadPlatform(package, platform);
        }
    }

    private static void ReadPlatform(ExcelPackage package, Platform platform)
    {
        var ws = package.Workbook.Worksheets[platform.Name];
        var rowIndex = 1;
        var rowOffset = 1;
        
        var columns = CreateColumns(platform);
        var cellContent = ws.Cells[rowIndex + rowOffset, 1].GetValue<String>();

        var relationships = new Dictionary<string, Relationship>();
        
        while (!string.IsNullOrEmpty(cellContent))
        {
            var relationShip = new Relationship();

            if (ws.Cells[rowIndex + rowOffset, columns["Group Reference"].ColIndex].GetValue<bool>())
            {
                relationShip.Id = ws.Cells[rowIndex + rowOffset, columns["Relationship STIX ID"].ColIndex].GetValue<string>();

                foreach (var systemCol in columns.Values.Where(x => x.System && !x.ColumnName.Contains("Score")))
                {
                    var sys = new Sys();
                    sys.Name = systemCol.ColumnName;
                    sys.Mitigation = ws.Cells[rowIndex + rowOffset, systemCol.ColIndex].GetValue<string>();
                    sys.Score = ws.Cells[rowIndex + rowOffset, systemCol.ColIndex + 1].GetValue<double?>();
                    
                    relationShip.Systems.Add(sys.Name, sys);
                }
                
                relationships.Add(relationShip.Id, relationShip);
            }

            rowIndex += 1;
            cellContent = ws.Cells[rowIndex + rowOffset, 1].GetValue<String>();
        }
        
        platform.OldRelationships = relationships;
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

        AppDataPath = appDataPath;
    }
    
    private static async Task DownloadAllMitreFiles()
    {
        var url = "https://api.github.com/repos/mitre-attack/attack-stix-data/contents/enterprise-attack";
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("SpectreConsoleApp");

        var resp = await client.GetAsync(url);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();
        var items = JsonSerializer.Deserialize<List<MitreJsonFile>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        if (items == null)
            throw new Exception("Json doesn't contain any items.");
        
        foreach (var item in items)
            item.CalcVersion();
        
        var last5Versions = items
            .OrderByDescending(x => x.Version)
            .Take(5)
            .ToList();

        foreach (var item in last5Versions)
        {
            var filePath = Path.Combine(AppDataPath, item.Name);

            if (File.Exists(filePath))
            {
                AnsiConsole.MarkupLine("[grey]Skipping file {0}[/]", item.Name);
                continue;
            }
            
            AnsiConsole.MarkupLine("[grey]Downloading file {0}[/]", item.Name);
            await client.DownloadFileTaskAsync(item.DownloadUrl.AbsoluteUri, filePath);
        }
    }

    private static FileInfo SelectExcelFile()
    {
        var files = new List<FileInfo>();

        foreach (var filePath in Directory.EnumerateFiles(AppDataPath, "*.xlsx", SearchOption.AllDirectories))
        {
            var file = new FileInfo(filePath);
            files.Add(file);
        }
        
        var excelFileName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select Excel File to be updated?")
                .PageSize(20)
                .AddChoices(files.Select(x => x.Name).OrderByDescending(x => x)));

        return files.Single(x => x.Name == excelFileName);
    }
    
    private static FileInfo SelectStixFile()
    {
        var files = new List<FileInfo>();

        foreach (var filePath in Directory.EnumerateFiles(AppDataPath, "*.json", SearchOption.AllDirectories))
        {
            var file = new FileInfo(filePath);
            files.Add(file);
        }
        
        var stixFileName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select STIX JSON File?")
                .PageSize(20)
                .AddChoices(files.Select(x => x.Name).OrderByDescending(x => x)));

        return files.Single(x => x.Name == stixFileName);
    }

    private static void CreateMitrePlatforms()
    {
        Platforms = new List<Platform>();
        
        var p1 = new Platform();
        p1.Name = "Windows";
        p1.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p1.Name)).ToList();
        p1.Systems.Add("Windows 11");
        p1.Systems.Add("Windows Server 2019");
        p1.Systems.Add("Windows Server 2022");
        
        Platforms.Add(p1);
        
        var p2 = new Platform();
        p2.Name = "Linux";
        p2.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p2.Name)).ToList();
        p2.Systems.Add("RHEL 8");
        p2.Systems.Add("RHEL 9");
        
        Platforms.Add(p2);
        
        var p3 = new Platform();
        p3.Name = "Network Devices";
        p3.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p3.Name)).ToList();
        p3.Systems.Add("Netzwerk FITS");
        p3.Systems.Add("Netzwerk CC");
        p3.Systems.Add("Netzwerk Azure");
        p3.Systems.Add("Netzwerk LUX");
        
        Platforms.Add(p3);
        
        var p4 = new Platform();
        p4.Name = "Containers";
        p4.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p4.Name)).ToList();
        p4.Systems.Add("Kubernetes Cluster");

        Platforms.Add(p4);
        
        var p5 = new Platform();
        p5.Name = "IaaS";
        p5.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p5.Name)).ToList();
        p5.Systems.Add("FCPI");
        p5.Systems.Add("Azurblau");
        
        Platforms.Add(p5);
        
        var p6 = new Platform();
        p6.Name = "Identity Provider";
        p6.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p6.Name)).ToList();
        p6.Systems.Add("Entra ID");
        
        Platforms.Add(p6);
        
        var p7 = new Platform();
        p7.Name = "Office Suite";
        p7.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p7.Name)).ToList();
        p7.Systems.Add("M365");
        
        Platforms.Add(p7);
        
        var p8 = new Platform();
        p8.Name = "SaaS";
        p8.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p8.Name)).ToList();
        p8.Systems.Add("M365");
        
        Platforms.Add(p8);
        
        var p9 = new Platform();
        p9.Name = "PRE";
        p9.Relationships = Relationships.Where(x => x.XMitrePlatforms.Contains(p9.Name)).ToList();
        p9.Systems.Add("PRE");
        
        Platforms.Add(p9);
    }
    
    private static void CreateMitreFile(FileInfo excelFile)
    {
        using (var package = new ExcelPackage())
        {
            foreach (var platform in Platforms)
                CreatePlatform(package, platform);
            
            package.SaveAs(excelFile);
        }
    }

    private static void CreatePlatform(ExcelPackage package, Platform platform)
    {
        GenerateGroupGuids(platform.Relationships);

        var ws = package.Workbook.Worksheets.Add(platform.Name);
        var rowIndex = 1;
        var rowOffset = 1;
        var colIndex = 1;

        var columns = CreateColumns(platform);

        ws.Cells[1, 1, 1, columns.Count].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells[1, 1, 1, columns.Count].Style.Fill.BackgroundColor.SetColor(Color.Gray);
        ws.Cells[1, 1, 1, columns.Count].AutoFilter = true;
        ws.Row(1).Height = 26;
        ws.View.FreezePanes(2, 1);

        foreach (var column in columns.Values)
        {
            ws.Cells[1, colIndex].Value = column.ColumnName;
            ws.Cells[1, colIndex].Style.Font.Bold = true;

            ws.Column(colIndex).Width = column.ColumnWidth;
            ws.Column(colIndex).Style.WrapText = column.WrapText;
            ws.Column(colIndex).Hidden = column.Hidden;

            if (!string.IsNullOrEmpty(column.NumberFormat))
                ws.Column(colIndex).Style.Numberformat.Format = column.NumberFormat;

            if (column.System)
            {
                ws.Cells[1, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, colIndex].Style.Fill.BackgroundColor.SetColor(Color.RoyalBlue);
            }

            ws.Cells[1, colIndex].Style.WrapText = true;

            colIndex += 1;
        }

        var colAlphaGroupRefId =
            NumberToLetter( columns["Group Reference ID"].ColIndex);

        var colAlphaRelationshipStixId = 
            NumberToLetter(columns["Relationship STIX ID"].ColIndex);
        
        foreach (var item in platform.Relationships
                     .OrderBy(x => x.CourseOfActionExternalId)
                     .ThenBy(x => x.AttackPatternExternalId))
        {
            var coa = CourseOfActions[item.SourceRef];
            var ap = AttackPatterns[item.TargetRef];

            colIndex = 1;
            
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = rowIndex;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.Id;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = coa.Id;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value =
                coa.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = coa.Name;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = ap.Id;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = 
                ap.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.Description;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = "YES";
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = DateTime.UtcNow;
            
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = coa.Created;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = coa.Modified;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.Created;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.Modified;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = ap.Created;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = ap.Modified;
            
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = "CHECK";
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.GroupGuid.ToString();
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.GroupReference;
            ws.Cells[rowIndex + rowOffset, colIndex++].Value = item.GroupReferenceId;

            foreach (var system in platform.Systems)
            {
                var colSystem = columns[system].ColIndex;
                var colSystemScore = colSystem + 1;
                
                if (!item.GroupReference)
                {
                    ws.Cells[rowIndex + rowOffset, colSystem].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[rowIndex + rowOffset, colSystem].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws.Cells[rowIndex + rowOffset, colSystem].Formula =
                        string.Format("=IF(INDIRECT(ADDRESS(MATCH({0}{1},{2}:{2},0),{3}))=\"\",\"\"," +
                                      "INDIRECT(ADDRESS(MATCH({0}{1},{2}:{2},0),{3})))",
                            colAlphaGroupRefId,rowIndex + rowOffset, colAlphaRelationshipStixId, colSystem);
                    
                    ws.Cells[rowIndex + rowOffset, colSystemScore].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[rowIndex + rowOffset, colSystemScore].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                    ws.Cells[rowIndex + rowOffset, colSystemScore].Formula =
                        string.Format("=IF(INDIRECT(ADDRESS(MATCH({0}{1},{2}:{2},0),{3}))=\"\",\"\"," +
                                      "INDIRECT(ADDRESS(MATCH({0}{1},{2}:{2},0),{3})))",
                            colAlphaGroupRefId,rowIndex + rowOffset, colAlphaRelationshipStixId, colSystemScore);
                }
                else
                {
                    if (platform.OldRelationships.Count > 0)
                    {
                        if (platform.OldRelationships.ContainsKey(item.Id))
                        {
                            ws.Cells[rowIndex + rowOffset, colSystem].Value = platform.OldRelationships[item.Id].Systems[system].Mitigation;
                            ws.Cells[rowIndex + rowOffset, colSystemScore].Value = platform.OldRelationships[item.Id].Systems[system].Score;
                        }
                    }
                }

            }
            
            rowIndex += 1;
        }

    }

    private static Dictionary<string, Column> CreateColumns(Platform platform)
    {
        var columns = new Dictionary<string, Column>();
        columns.Add("Sort No.", new Column() {ColumnName = "Sort No.", ColumnWidth = 10, WrapText = false, Hidden = false, ColIndex = 1});
        columns.Add("Relationship STIX ID", new Column() {ColumnName = "Relationship STIX ID", ColumnWidth = 55, WrapText = false, Hidden = true, ColIndex = 2});
        columns.Add("Mitigation STIX ID", new Column() {ColumnName = "Mitigation STIX ID", ColumnWidth = 55, WrapText = false, Hidden = true,  ColIndex = 3});
        columns.Add("Mitigation ID", new Column() {ColumnName = "Mitigation ID", ColumnWidth = 15, WrapText = false, ColIndex = 4});
        columns.Add("Mitigation Name", new Column() {ColumnName = "Mitigation Name", ColumnWidth = 35, WrapText = false, ColIndex = 5});
        columns.Add("Technique STIX ID", new Column() {ColumnName = "Technique STIX ID", ColumnWidth = 55, WrapText = false, Hidden = true,  ColIndex = 6});
        columns.Add("Technique ID", new Column() {ColumnName = "Technique ID", ColumnWidth = 15, WrapText = false, ColIndex = 7});
        columns.Add("Description", new Column() {ColumnName = "Description", ColumnWidth = 50, WrapText = true,  Hidden = false, ColIndex = 8});
        columns.Add("Latest", new Column() {ColumnName = "Latest", ColumnWidth = 11, WrapText = false, ColIndex = 9});
        columns.Add("Added At", new Column() {ColumnName = "Added At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd", ColIndex = 10});
        columns.Add("Mitigation Created At", new Column() {ColumnName = "Mitigation Created At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd",  ColIndex = 11});
        columns.Add("Mitigation Modified At", new Column() {ColumnName = "Mitigation Modified At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd",  ColIndex = 12});
        columns.Add("Relationship Created At", new Column() {ColumnName = "Relationship Created At", ColumnWidth = 15, WrapText = false, NumberFormat = "yyyy-mm-dd",  ColIndex = 13});
        columns.Add("Relationship Modified At", new Column() {ColumnName = "Relationship Modified At", ColumnWidth = 15, WrapText = false, NumberFormat = "yyyy-mm-dd",  ColIndex = 14});
        columns.Add("Technique Created At", new Column() {ColumnName = "Technique Created At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd",  ColIndex = 15});
        columns.Add("Technique Modified At", new Column() {ColumnName = "Technique Modified At", ColumnWidth = 15, WrapText = false, Hidden = true, NumberFormat = "yyyy-mm-dd",  ColIndex = 16});
        columns.Add("Status", new Column() {ColumnName = "Status", ColumnWidth = 11, WrapText = false, ColIndex = 17});
        columns.Add("Group Guid", new Column() {ColumnName = "Group Guid", ColumnWidth = 35, WrapText = false, Hidden = true, ColIndex = 18});
        columns.Add("Group Reference", new Column() {ColumnName = "Group Reference", ColumnWidth = 11, WrapText = false, Hidden = true, ColIndex = 19});
        columns.Add("Group Reference ID", new Column() {ColumnName = "Group Reference ID", ColumnWidth = 55, WrapText = false, Hidden = true, ColIndex = 20});
        
        var colIndex = columns.Last().Value.ColIndex;
        
        foreach (var system in platform.Systems)
        {
            columns.Add(system, new Column() {ColumnName = system, ColumnWidth = 35, WrapText = true, Hidden = false, System = true, ColIndex = ++colIndex});
            columns.Add(system + "-" + "Score", new Column() {ColumnName = "Score", ColumnWidth = 9, WrapText = false, Hidden = false, System = true,  ColIndex = ++colIndex});
        }

        return columns;
    }
    
    private static void ReadStixFile(FileInfo stixFile)
    {
        var stixDoc = JsonDocument.Parse(File.ReadAllText(stixFile.FullName));
        var stixObjects = stixDoc.RootElement.GetProperty("objects");
        
        CourseOfActions = GetCourseOfActions(stixObjects);
        Relationships = GetRelationships(CourseOfActions, stixObjects);
        AttackPatterns = GetAttackPatterns(stixObjects);

        ExtendRelationships();
        CreateMitrePlatforms();
    }

    private static void GenerateGroupGuids(List<Relationship> relationships)
    {
        var groups = relationships.GroupBy(x => new { x.CourseOfActionExternalId, x.Description });

        foreach (var group in groups)
        {
            var groupGuid = Guid.NewGuid();
            var first = true;
            var groupReferenceId = string.Empty;
            
            foreach (var item in group)
            {
                if (first)
                {
                    item.GroupReference = true;
                    groupReferenceId = item.Id;
                    first = false;
                }
                else
                {
                    item.GroupReferenceId = groupReferenceId;
                }

                item.GroupGuid = groupGuid;
            }
        }
    }

    private static void ExtendRelationships()
    {
        foreach (var item in Relationships)
        {
            var coa = CourseOfActions[item.SourceRef];
            var ap = AttackPatterns[item.TargetRef];
            
            item.CourseOfActionExternalId = 
                coa.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId ?? string.Empty;
            
            item.AttackPatternExternalId = 
                ap.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId ?? string.Empty;;

            item.XMitrePlatforms = ap.XMitrePlatforms;
        }
    }

    private static Dictionary<string, AttackPattern> GetAttackPatterns(JsonElement stixObjects)
    {
        var attackPatterns = new Dictionary<string, AttackPattern>();

        foreach (var item in stixObjects.EnumerateArray())
        {
            var type = item.GetProperty("type").GetString();
            
            if (type == "attack-pattern")
            {
                var attackPattern = item.Deserialize<AttackPattern>() ?? throw new JsonException();

                if (attackPattern.Revoked || attackPattern.XMitreDeprecated)
                    continue;

                attackPatterns.Add(attackPattern.Id, attackPattern);
            }
        }

        return attackPatterns;
    }

    private static List<Relationship> GetRelationships(Dictionary<string, CourseOfAction> courseOfActions, JsonElement stixObjects)
    {
        var relationships = new List<Relationship>();

        foreach (var item in stixObjects.EnumerateArray())
        {
            var type = item.GetProperty("type").GetString();
            
            if (type == "relationship")
            {
                var relationship = item.Deserialize<Relationship>() ?? throw new JsonException();

                if (relationship.Revoked || relationship.XMitreDeprecated)
                    continue;

                if (courseOfActions.ContainsKey(relationship.SourceRef))
                    relationships.Add(relationship);
            }
        }

        return relationships;
    }
    
    private static Dictionary<string, CourseOfAction> GetCourseOfActions(JsonElement stixObjects)
    {
        var courseOfActions = new Dictionary<string, CourseOfAction>();

        foreach (var item in stixObjects.EnumerateArray())
        {
            var type = item.GetProperty("type").GetString();
            
            if (type == "course-of-action")
            {
                var coa = item.Deserialize<CourseOfAction>() ?? throw new JsonException();

                if (coa.Revoked || coa.XMitreDeprecated)
                    continue;

                // if (coa.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId != "M1013")
                //     continue;
                
                courseOfActions.Add(coa.Id, coa);
            }
        }

        return courseOfActions;
    }
    
    private static string NumberToLetter(int number)
    {
        if (number < 1 || number > 26)
            throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 1 and 26.");

        return ((char)(number + 64)).ToString();
    }
}

public static class HttpClientUtils
{
    public static async Task DownloadFileTaskAsync(this HttpClient client, string uri, string filePath)
    {
        using (var stream = await client.GetStreamAsync(uri))
        using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
        {
            await stream.CopyToAsync(fileStream);
        }
    }
}