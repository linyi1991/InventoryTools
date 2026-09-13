using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using InventoryTools.Logic.Features;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Ui;

public class ConfigurationWizard : GenericWindow
{
    private readonly ConfigurationWizardService _configurationWizardService;
    private readonly InventoryToolsConfiguration _configuration;

    public ConfigurationWizard(ILogger<ConfigurationWizard> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ConfigurationWizardService configurationWizardService, string name = "Configuration Wizard") : base(logger, mediator, imGuiService, configuration, name)
    {
        _configurationWizardService = configurationWizardService;
        _configuration = configuration;
    }
    private List<IFeature> _availableFeatures = new();
    private int _currentFeature;
    public override void Initialize()
    {
        WindowName = "設定精靈";
        Key = "wizard";
        _availableFeatures = _configurationWizardService.GetNewFeatures();
    }

    public override string GenericKey => "wizard";
    public override string GenericName => "設定精靈";
    public override bool DestroyOnClose => true;
    public override bool SaveState => false;
    public override Vector2? DefaultSize { get; } = new(750, 500);
    public override Vector2? MaxSize { get; } = new(1000, 1000);
    public override Vector2? MinSize { get; } = new(750, 350);

    private bool CanGoPrevious => _currentFeature != 0;
    private bool CanGoNext => _availableFeatures.Count != 0 && _currentFeature != _availableFeatures.Count;

    private void NextStep()
    {
        if (_currentFeature == 0)
        {
            _currentFeature = 1;
        }
        else if(_currentFeature == _availableFeatures.Count)
        {

        }
        else
        {
            _currentFeature++;
        }
    }

    private void PreviousStep()
    {
        if (_currentFeature != 0)
        {
            _currentFeature--;
        }
    }

    public override void Draw()
    {
        using (var sideBar = ImRaii.Child("sideBar", new Vector2(150, 0) * ImGui.GetIO().FontGlobalScale, true))
        {
            if (sideBar)
            {
                using (var sideBarMenu = ImRaii.Child("sideBarMenu",
                           new Vector2(150, -120) * ImGui.GetIO().FontGlobalScale, false))
                {
                    if (sideBarMenu)
                    {
                        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen,
                                   _currentFeature == 0))
                        {
                            ImGui.Text("歡迎");
                        }

                        for (var index = 0; index < _availableFeatures.Count; index++)
                        {
                            var feature = _availableFeatures[index];
                            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen,
                                       index + 1 == _currentFeature))
                            {
                                ImGui.Text((index + 1) + ". " + TwSettingsLocalization.Translate(feature.Name));
                            }
                        }
                    }
                }
                using (var sideBarImage = ImRaii.Child("sideBarImage",
                           new Vector2(150, 0) * ImGui.GetIO().FontGlobalScale, false))
                {
                    if (sideBarImage)
                    {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 12.5f);
                        ImGui.Image(ImGuiService.GetImageTexture("icon").Handle, new (100,100));
                    }
                }
            }
        }
        ImGui.SameLine();
        using (var mainWindow = ImRaii.Child("mainWindow", new Vector2(0, 0)))
        {
            if (mainWindow)
            {
                using (var mainContainer = ImRaii.Child("mainContainer", new Vector2(-1, -80) * ImGui.GetIO().FontGlobalScale, true))
                {
                    if (mainContainer)
                    {
                        if (_currentFeature == 0)
                        {
                            if (_configurationWizardService.ConfiguredOnce)
                            {
                                ImGui.TextWrapped("歡迎回到 Allagan Tools 設定精靈。");
                                ImGui.Separator();
                                ImGui.TextWrapped(
                                    "目前有新功能可供設定；你先前選擇在新增功能時顯示此視窗。");
                                ImGui.NewLine();
                            }
                            else
                            {
                                ImGui.TextWrapped("歡迎使用 Allagan Tools 設定精靈。");
                                ImGui.Separator();
                                ImGui.TextWrapped(
                                    "此精靈會引導你設定最常用的功能。經你允許，未來新增需要手動啟用的功能時也會再次顯示。");
                                ImGui.NewLine();
                                ImGui.TextWrapped("若是第一次使用，建議開啟說明視窗閱讀「一般」章節，快速了解插件功能。");
                                ImGui.TextWrapped("若已熟悉插件，可以直接關閉此視窗。");
                                if (ImGui.Button("開啟說明"))
                                {
                                    MediatorService.Publish(new ToggleGenericWindowMessage(typeof(HelpWindow)));
                                }
                                ImGui.NewLine();
                            }


                        }
                        else
                        {
                            for (var index = 0; index < _availableFeatures.Count; index++)
                            {
                                var feature = _availableFeatures[index];
                                if (_currentFeature - 1 == index)
                                {
                                    ImGui.Text(TwSettingsLocalization.Translate(feature.Name));
                                    ImGui.Separator();
                                    ImGui.PushTextWrapPos();
                                    ImGui.Text(TwSettingsLocalization.Translate(feature.Description));
                                    ImGui.PopTextWrapPos();
                                    ImGui.Separator();
                                    foreach (var setting in _configurationWizardService.GetApplicableSettings(feature))
                                    {
                                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                                        setting.Draw(_configuration, setting.WizardName, true, true);
                                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                                    }
                                }
                            }
                        }
                    }
                }

                using (var nextPrevBar = ImRaii.Child("nextPrevBar", new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, true))
                {
                    if (nextPrevBar)
                    {
                        if (_currentFeature == 0)
                        {
                            if (_configurationWizardService.ConfiguredOnce)
                            {
                                if (ImGui.Button("繼續"))
                                {
                                    NextStep();
                                    _configuration.ShowWizardNewFeatures = true;
                                }

                                if (ImGui.Button("關閉（下次載入時再顯示）"))
                                {
                                    Close();
                                    _configuration.ShowWizardNewFeatures = true;
                                }
                            }
                            else
                            {
                                if (ImGui.Button("繼續（有新功能時再顯示）"))
                                {
                                    NextStep();
                                    _configuration.ShowWizardNewFeatures = true;
                                }

                                ImGui.SameLine();
                                if (ImGui.Button("繼續（不再自動顯示精靈）"))
                                {
                                    NextStep();
                                    _configuration.ShowWizardNewFeatures = false;
                                }

                                if (ImGui.Button("關閉（下次載入時再顯示）"))
                                {
                                    Close();
                                    _configuration.ShowWizardNewFeatures = true;
                                }

                                ImGui.SameLine();
                                if (ImGui.Button("關閉（不再自動顯示精靈）"))
                                {
                                    Close();
                                    _configuration.ShowWizardNewFeatures = false;
                                }
                            }
                        }
                        else
                        {
                            var canGoPrevious = CanGoPrevious;
                            using var disabled = ImRaii.Disabled(!canGoPrevious);

                            if (ImGui.Button("上一步"))
                            {
                                PreviousStep();
                            }

                            disabled.Dispose();

                            ImGui.SameLine();
                            var canGoNext = CanGoNext;

                            if (canGoNext && ImGui.Button("下一步"))
                            {
                                NextStep();
                            }

                            if (!canGoNext && ImGui.Button("完成"))
                            {
                                Finish();
                            }

                        }
                    }
                }
            }
        }
    }

    private void Finish()
    {
        this.Close();
        _currentFeature = 0;
        foreach (var feature in _availableFeatures)
        {
            feature.OnFinish();
        }

        if (!_configurationWizardService.ConfiguredOnce)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
        }
        _configurationWizardService.MarkFeaturesSeen();

    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
}
