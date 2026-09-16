using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.Pages
{
    public class ImportExportPage : Page
    {
        private readonly IListService _listService;
        private readonly IChatUtilities _chatUtilities;
        private readonly PluginLogic _pluginLogic;
        private readonly ListImportExportService _importExportService;
        private readonly IClipboardService _clipboardService;

        public ImportExportPage(ILogger<ImportExportPage> logger, ImGuiService imGuiService, IListService listService, IChatUtilities chatUtilities, PluginLogic pluginLogic, ListImportExportService importExportService, IClipboardService clipboardService) : base(logger, imGuiService)
        {
            _listService = listService;
            _chatUtilities = chatUtilities;
            _pluginLogic = pluginLogic;
            _importExportService = importExportService;
            _clipboardService = clipboardService;
        }
        private bool _isSeparator = false;
        public override void Initialize()
        {

        }

        public override string Name { get; } =  "匯入／匯出";
        public override List<MessageBase>? Draw()
        {
            ImGui.PushID("ImportSection");
            if (ImGui.CollapsingHeader("匯出###Export", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var filterConfigurations = _listService.Lists;
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
                using (var table = ImRaii.Table("FilterConfigTable", 3, ImGuiTableFlags.BordersV |
                                                             ImGuiTableFlags.BordersOuterV |
                                                             ImGuiTableFlags.BordersInnerV |
                                                             ImGuiTableFlags.BordersH |
                                                             ImGuiTableFlags.BordersOuterH |
                                                             ImGuiTableFlags.BordersInnerH))
                {
                    if (table)
                    {
                        ImGui.TableSetupColumn("名稱###Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)0);
                        ImGui.TableSetupColumn("類型###Type", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)1);
                        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint)2);
                        ImGui.TableHeadersRow();
                        if (filterConfigurations.Count == 0)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.TextUnformatted("尚未建立清單！");
                            ImGui.TableNextColumn();
                            ImGui.TableNextColumn();
                        }

                        for (var index = 0; index < filterConfigurations.Count; index++)
                        {
                            ImGui.TableNextRow();
                            var filterConfiguration = filterConfigurations[index];
                            ImGui.TableNextColumn();
                            if (filterConfiguration.Name != "")
                            {
                                ImGui.TextUnformatted(filterConfiguration.Name);
                                ImGui.SameLine();
                            }

                            /*if (PluginFont.AppIcons.HasValue && filterConfiguration.Icon != null)
                            {
                                ImGui.PushFont(PluginFont.AppIcons.Value);
                                ImGui.Text(filterConfiguration.Icon);
                                ImGui.PopFont();
                            }*/

                            ImGui.TableNextColumn();
                            ImGui.TextUnformatted(filterConfiguration.FormattedFilterType);
                            ImGui.TableNextColumn();
                            if (ImGui.SmallButton("匯出設定###Export Configuration##" + index))
                            {
                                var base64 = _importExportService.ToBase64(filterConfiguration);
                                _clipboardService.CopyToClipboard(base64);
                                _chatUtilities.PrintClipboardMessage("[匯出] ", "清單設定");
                            }
                        }
                    }
                }

                ImGui.PopStyleVar();
            }

            if (ImGui.CollapsingHeader("匯入###Import", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var importData = ImportData;
                if (ImGui.InputTextMultiline("在此貼上清單###Paste list here",ref importData, 10000, new Vector2(400, 200) * ImGui.GetIO().FontGlobalScale))
                {
                    ImportData = importData;
                    ImportFailed = false;
                }

                if (ImGui.Button("匯入###Import##ImportBtn"))
                {
                    if (ImportData == "")
                    {
                        ImportFailed = true;
                        FailedReason =
                            "請先貼上透過匯出功能產生的清單，再按匯入。";
                    }
                    else
                    {
                        try
                        {
                            if (_importExportService.FromBase64(ImportData, out var newFilter))
                            {
                                _pluginLogic.AddFilter(newFilter);
                            }
                            else
                            {
                                ImportFailed = true;
                                FailedReason =
                                    "匯入資料無效，請確認已完整複製清單內容。";
                            }
                        }
                        catch (ListImportVersionException e)
                        {
                            ImportFailed = true;
                            FailedReason =
                                $"此清單版本不相容。目前版本：{(e.ImportingVersion?.ToString() ?? "0")}；所需版本：{e.RequiredVersion}.";
                        }
                    }
                }

                if (ImportFailed)
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed, FailedReason);
                }
            }
            ImGui.PopID();
            return null;
        }

        public override bool IsMenuItem => _isSeparator;
        public override bool DrawBorder => true;

        public string FailedReason { get; set; } = "";

        public bool ImportFailed { get; set; } = false;

        public string ImportData { get; set; } = "";
    }
}
