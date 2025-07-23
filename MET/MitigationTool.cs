using System.Text.Json;
using MET.Models;
using MITREModels.LAYER;
using MITREModels.STIX;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Spectre.Console;
using Color = System.Drawing.Color;
using Layout = MITREModels.LAYER.Layout;

namespace MET;

public class MitigationTool
{
    private Version StixVersion { get; set; } = new Version("0.0.0.0");
    private Dictionary<string, CourseOfAction> CourseOfActions { get; set; } =
        new Dictionary<string, CourseOfAction>();
    private List<Relationship> Relationships { get; set; } = 
        new List<Relationship>();
    private Dictionary<string, AttackPattern> AttackPatterns { get; set; } =
        new Dictionary<string, AttackPattern>();
    private List<Platform> Platforms { get; set; } = 
        new List<Platform>();
    
    public void CreateMitreMitigationFile()
    {
        var stixFile = SelectJsonFile("Select STIX JSON File");
        var excelFileName = $"MITRE Mitigations {DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
        var excelFile = new FileInfo(Path.Combine(Program.AppDataPath, excelFileName));
        
        ReadStixFile(stixFile);
        CreateMitreMitigationFile(excelFile);
    }
    
    public void UpdateMitreMitigationFile()
    {
        var oldExcelFile = SelectExcelFile("Select Mitigation Excel File");
        var stixFile = SelectJsonFile("Select STIX JSON File");
        var newExcelFileName = string.Format("MITRE Mitigations {0:yyyyMMdd-HHmmss}.xlsx", DateTime.Now);
        var newExcelFile = new FileInfo(Path.Combine(Program.AppDataPath, newExcelFileName));
        
        ReadStixFile(stixFile);
        ReadMitigations(oldExcelFile, true);
        CreateMitreMitigationFile(newExcelFile);
    }
    
    public async Task DownloadAllMitreFiles()
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
            var filePath = Path.Combine(Program.AppDataPath, item.Name);

            if (File.Exists(filePath))
            {
                AnsiConsole.MarkupLine("[grey]Skipping file {0}[/]", item.Name);
                continue;
            }
            
            AnsiConsole.MarkupLine("[grey]Downloading file {0}[/]", item.Name);
            await client.DownloadFileTaskAsync(item.DownloadUrl.AbsoluteUri, filePath);
        }
    }

    public void CreateMitreNavigatorLayers()
    {
        var stixFile = SelectJsonFile("Select STIX JSON File");
        var excelFile = SelectExcelFile("Select Mitigation Excel File");
        // var layerFile = SelectJsonFile("Select Navigator Layer File");

        ReadStixFile(stixFile);
        CreateMitrePlatforms();
        ReadMitigations(excelFile, false);

        var appDataDir = new DirectoryInfo(Program.AppDataPath);
        var layersDir = appDataDir.CreateSubdirectory($"Layers {DateTime.Now:yyyyMMdd-HHmmss}");
        
        foreach (var platform in Platforms)
        {
            // var jsonText = File.ReadAllText(layerFile.FullName);
            // var layer = JsonSerializer.Deserialize<Layer>(jsonText) ?? throw new JsonException();

            var layer1 = new Layer();
            layer1.Name = platform.Name!;
            layer1.Versions = new Versions()
            {
                Navigator = "5.1.1", 
                Attack = "17", 
                Layer = "4.5"
            };
            layer1.Domain = "enterprise-attack";
            layer1.Description = "";
            var platformFilters = new List<string>();
            platformFilters.Add(platform.Name!);
            layer1.Filters = new Filters() { Platforms = platformFilters};
            layer1.Sorting = 0;
            layer1.Layout = new Layout()
            {
                LayoutType = "side",
                AggregateFunction = "average",
                CountUnscored = false,
                ExpandedSubtechniques = "none",
                ShowAggregateScores = false,
                ShowID = false,
                ShowName = true
            };
            layer1.HideDisabled = false;

            layer1.Gradient = new Gradient()
            {
                Colors = new List<string>()
                {
                    "#ff6666ff",
                    "#ffe766ff",
                    "#8ec843ff",
                },
                MinValue = 0,
                MaxValue = 100,
            };

            var groups = 
                platform.OldRelationships.Values
                    .GroupBy(x => x.AttackPatternExternalId);
            

            foreach (var group in groups)
            {
                var ap = AttackPatterns[group.Key];

                if (ap.Id == "attack-pattern--b17a1a56-e99c-403c-8948-561df0cffe81")
                {
                    AnsiConsole.MarkupLine("[grey]Skipping group {0}[/]", group.Key);
                }
                
                foreach (var killChainPhase in ap.KillChainPhases)
                {
                    var technique = new Technique();
                    technique.TechniqueId =
                        ap.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId;
                    technique.Tactic = killChainPhase.PhaseName;
                    technique.Color = "";
                    technique.Comment = "";
                    technique.Enabled = true;
                    technique.Links = new List<string>();
                    technique.ShowSubTechniques = false;
                    technique.Metadata = new List<MetaData>();
                    
                    double scoreSum = 0;
                    double scoreCount = 0;
                    
                    foreach (var relationship in group)
                    {
                        var metadata = new MetaData();
                        metadata.Name = relationship.MitigationId;
                        
                        foreach (var system in relationship.Systems.Values)
                        {
                            if (system.Score != null)
                            {
                                metadata.Value += system.Score + ", ";
                                scoreSum += system.Score.Value;
                                scoreCount += 1;
                            }
                        }
                        
                        technique.Metadata.Add(metadata);
                        
                        if (scoreCount > 0)
                        {
                            technique.Score = (int)Math.Round(scoreSum / scoreCount);;
                        }
                    }
                    
                    layer1.Techniques.Add(technique);
                }
            }
            
            var layerJson = JsonSerializer.Serialize(layer1, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(layersDir.FullName, $"Layer_{platform.Name}.json"), layerJson);
        }
    }
    
    private void ReadMitigations(FileInfo excelFile, bool onlyGroupReference)
    {
        using (var package = new ExcelPackage(excelFile))
        {
            foreach (var platform in Platforms)
                ReadPlatform(package, platform, onlyGroupReference);
        }
    }
    
    private void ReadPlatform(ExcelPackage package, Platform platform, bool onlyGroupReference)
    {
        var ws = package.Workbook.Worksheets[platform.Name];
        var rowIndex = 1;
        var rowOffset = 1;
        
        var columns = CreateColumns(platform);
        var cellContent = ws.Cells[rowIndex + rowOffset, 1].GetValue<String>();

        var relationships = new Dictionary<string, Relationship>();
        
        while (!string.IsNullOrEmpty(cellContent))
        {
            var relationship = new Relationship();
            
            if (ws.Cells[rowIndex + rowOffset, columns["Group Reference"].ColIndex].GetValue<bool>() || !onlyGroupReference)
            {
                relationship.Id = ws.Cells[rowIndex + rowOffset, columns["Relationship STIX ID"].ColIndex].GetValue<string>();
                relationship.TechniqueId = ws.Cells[rowIndex + rowOffset, columns["Technique ID"].ColIndex].GetValue<string>();
                relationship.MitigationId = ws.Cells[rowIndex + rowOffset, columns["Mitigation ID"].ColIndex].GetValue<string>();
                relationship.AttackPatternExternalId = ws.Cells[rowIndex + rowOffset, columns["Technique STIX ID"].ColIndex].GetValue<string>();
                
                foreach (var systemCol in columns.Values.Where(x => x.System && !x.ColumnName.Contains("Score")))
                {
                    var sys = new Sys();
                    sys.Name = systemCol.ColumnName;
                    sys.Mitigation = ws.Cells[rowIndex + rowOffset, systemCol.ColIndex].GetValue<string>();
                    sys.Score = ws.Cells[rowIndex + rowOffset, systemCol.ColIndex + 1].GetValue<double?>();
                    
                    relationship.Systems.Add(sys.Name, sys);
                }
                
                relationships.Add(relationship.Id, relationship);
            }

            rowIndex += 1;
            cellContent = ws.Cells[rowIndex + rowOffset, 1].GetValue<String>();
        }
        
        platform.OldRelationships = relationships;
    }
    
    private FileInfo SelectExcelFile(string title)
    {
        var files = new List<FileInfo>();

        foreach (var filePath in Directory.EnumerateFiles(Program.AppDataPath, "*.xlsx", SearchOption.TopDirectoryOnly))
        {
            var file = new FileInfo(filePath);
            files.Add(file);
        }
        
        if (!files.Any())
            throw new Exception("No Excel files found.");
        
        var excelFileName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .EnableSearch()
                .PageSize(100)
                .AddChoices(files.Select(x => x.Name).OrderByDescending(x => x)));

        return files.Single(x => x.Name == excelFileName);
    }
    
    private FileInfo SelectJsonFile(string title)
    {
        var files = new List<FileInfo>();

        foreach (var filePath in Directory.EnumerateFiles(Program.AppDataPath, "*.json", SearchOption.TopDirectoryOnly))
        {
            var file = new FileInfo(filePath);
            files.Add(file);
        }
        
        if (!files.Any())
            throw new Exception("No JSON files found.");
        
        var stixFileName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(100)
                .EnableSearch()
                .AddChoices(files.Select(x => x.Name).OrderByDescending(x => x)));

        return files.Single(x => x.Name == stixFileName);
    }
    
    private void CreateMitrePlatforms()
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
    
    private void CreateMitreMitigationFile(FileInfo excelFile)
    {
        using (var package = new ExcelPackage())
        {
            foreach (var platform in Platforms)
                CreatePlatform(package, platform);

            var ws = package.Workbook.Worksheets.Add("CONFIG");
            
            ws.Cells[1, 1].Value = "Document Version";
            ws.Cells[1, 2].Value = "1.0";
            
            ws.Cells[2, 1].Value = "STIX Version";
            ws.Cells[2, 2].Value = StixVersion.ToString();
            
            package.SaveAs(excelFile);
        }
    }

    private void CreatePlatform(ExcelPackage package, Platform platform)
    {
        GenerateGroupGuids(platform.Relationships);

        var ws = package.Workbook.Worksheets.Add(platform.Name);
        var rowIndex = 1;
        var rowOffset = 1;
        var colIndex = 1;

        var columns = CreateColumns(platform);

        ws.Cells[1, 1, 1, columns.Count].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells[1, 1, 1, columns.Count].Style.Fill.BackgroundColor.SetColor(Color.Gray);
        // ws.Cells[1, 1, 1, columns.Count].AutoFilter = true;
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
            NumberToLetter(columns["Group Reference ID"].ColIndex);

        var colAlphaRelationshipStixId = 
            NumberToLetter(columns["Relationship STIX ID"].ColIndex);
        
        foreach (var item in platform.Relationships
                     .OrderBy(x => x.CourseOfActionExternalId)
                     .ThenBy(x => x.AttackPatternExternalId))
        {
            if (item.SourceRef == null)
                continue;
            if (item.TargetRef == null)
                continue;
            
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
                        if (item.Id == null)
                            throw new ApplicationException("Id can't be null");
                        
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

    private Dictionary<string, Column> CreateColumns(Platform platform)
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
    
    private void ReadStixFile(FileInfo stixFile)
    {
        var stixDoc = JsonDocument.Parse(File.ReadAllText(stixFile.FullName));
        var stixObjects = stixDoc.RootElement.GetProperty("objects");

        StixVersion = GetStixVersion(stixObjects);
        CourseOfActions = GetCourseOfActions(stixObjects);
        Relationships = GetRelationships(CourseOfActions, stixObjects);
        AttackPatterns = GetAttackPatterns(stixObjects);
        
        ExtendRelationships();
        CreateMitrePlatforms();
    }

    private void GenerateGroupGuids(List<Relationship> relationships)
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

    private void ExtendRelationships()
    {
        foreach (var item in Relationships)
        {
            if (item.SourceRef == null)
                continue;
            if (item.TargetRef == null)
                continue;
            
            var coa = CourseOfActions[item.SourceRef];
            var ap = AttackPatterns[item.TargetRef];
            
            item.CourseOfActionExternalId = 
                coa.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId ?? string.Empty;
            
            item.AttackPatternExternalId = 
                ap.ExternalReferences.Single(x => x.SourceName == "mitre-attack").ExternalId ?? string.Empty;;

            item.XMitrePlatforms = ap.XMitrePlatforms;
        }
    }

    private Version GetStixVersion(JsonElement stixObjects)
    {
        foreach (var item in stixObjects.EnumerateArray())
        {
            var type = item.GetProperty("type").GetString();
            
            if (type == "x-mitre-collection")
            {
                return new Version(item.GetProperty("x_mitre_version").GetString()!);
            }
        }
        
        throw new Exception("STIX Version not found");
    }
    
    private Dictionary<string, AttackPattern> GetAttackPatterns(JsonElement stixObjects)
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

    private List<Relationship> GetRelationships(Dictionary<string, CourseOfAction> courseOfActions, JsonElement stixObjects)
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

                if (relationship.SourceRef == null)
                    continue;
                
                if (courseOfActions.ContainsKey(relationship.SourceRef))
                    relationships.Add(relationship);
            }
        }

        return relationships;
    }
    
    private Dictionary<string, CourseOfAction> GetCourseOfActions(JsonElement stixObjects)
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
    
    private string NumberToLetter(int number)
    {
        if (number < 1 || number > 26)
            throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 1 and 26.");

        return ((char)(number + 64)).ToString();
    }
}