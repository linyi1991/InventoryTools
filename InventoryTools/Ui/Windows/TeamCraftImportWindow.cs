using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class TeamCraftImportWindow : GenericWindow
{
    private readonly ListImportExportService _importExportService;
    private string _importListItems = "";
    private bool _hasError;
    private List<(uint, uint)>? _parseResult;

    public TeamCraftImportWindow(ILogger<TeamCraftImportWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ListImportExportService importExportService, string name = "Teamcraft Import") : base(logger, mediator, imGuiService, configuration, name)
    {
        _importExportService = importExportService;
        Flags = ImGuiWindowFlags.NoCollapse;
    }

    public List<(uint, uint)>? ParseResult => _parseResult;


    public override string GenericKey { get; } = "tcimport";
    public override string GenericName { get; } = "Teamcraft 匯入";
    public override bool DestroyOnClose { get; }
    public override bool SaveState { get; } = false;
    public override Vector2? DefaultSize { get; } = new Vector2(300, 300);
    public override Vector2? MaxSize { get; }
    public override Vector2? MinSize { get; }
    public override void Initialize()
    {
    }

    public override void Draw()
    {
        ImGui.Text("匯入製作清單：");
        ImGui.SameLine();
        ImGuiService.HelpMarker("清單匯入說明。\r\n\r\n" +
                                "步驟 1：在 Teamcraft 開啟包含待製作物品的清單。\r\n\r\n" +
                                "步驟 2：在 Items 區段使用 Copy as Text，只複製成品項目。\r\n\r\n" +
                                "步驟 3：貼到此視窗下方的文字方塊。\r\n\r\n" +
                                "步驟 4：按「匯入」。");
        ImGui.Text("請在此貼上文字");
        ImGui.InputTextMultiline("###FinalItems", ref _importListItems, 10000000, new Vector2(ImGui.GetContentRegionAvail().X, 100));


        if (ImGui.Button("匯入"))
        {
            var importedList = _importExportService.FromTCString(_importListItems ?? "");
            if (importedList is not null)
            {
                Close();
                MediatorService.Publish(new TeamCraftDataImported(importedList));
            }

        }
        ImGui.SameLine();
        if (ImGui.Button("取消"))
        {
            Close();
        }
    }

    public override void Invalidate()
    {
        
    }

    public override FilterConfiguration? SelectedConfiguration { get; }
}
