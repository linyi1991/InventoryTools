using System.Collections.Generic;
using AllaganLib.Shared.Debuggers;
using AllaganLib.Shared.Interfaces;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace InventoryTools.Debuggers;

public class CharacterDebuggerPane : DebugLogPane
{
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IClientState _clientState;
    private readonly InventoryToolsConfiguration _configuration;

    public CharacterDebuggerPane(ICharacterMonitor characterMonitor, IClientState clientState, InventoryToolsConfiguration configuration)
    {
        _characterMonitor = characterMonitor;
        _clientState = clientState;
        _configuration = configuration;
    }
    public override string Name => "Character Monitor";

    public override void SubscribeToEvents()
    {
        _characterMonitor.OnActiveRetainerChanged += OnCharacterMonitorOnOnActiveRetainerChanged;
        RegisterSubscription(() => _characterMonitor.OnActiveRetainerChanged -= OnCharacterMonitorOnOnActiveRetainerChanged);

        _characterMonitor.OnActiveRetainerLoaded += OnCharacterMonitorOnOnActiveRetainerLoaded;
        RegisterSubscription(() => _characterMonitor.OnActiveRetainerLoaded -= OnCharacterMonitorOnOnActiveRetainerLoaded);

        _characterMonitor.OnActiveFreeCompanyChanged += OnCharacterMonitorOnOnActiveFreeCompanyChanged;
        RegisterSubscription(() => _characterMonitor.OnActiveFreeCompanyChanged -= OnCharacterMonitorOnOnActiveFreeCompanyChanged);

        _characterMonitor.OnActiveHouseChanged += OnCharacterMonitorOnOnActiveHouseChanged;
        RegisterSubscription(() => _characterMonitor.OnActiveHouseChanged -= OnCharacterMonitorOnOnActiveHouseChanged);

        _characterMonitor.OnCharacterUpdated += OnCharacterMonitorOnOnCharacterUpdated;
        RegisterSubscription(() => _characterMonitor.OnCharacterUpdated -= OnCharacterMonitorOnOnCharacterUpdated);

        _characterMonitor.OnCharacterRemoved += OnCharacterMonitorOnOnCharacterRemoved;
        RegisterSubscription(() => _characterMonitor.OnCharacterRemoved -= OnCharacterMonitorOnOnCharacterRemoved);

        _characterMonitor.OnCharacterJobChanged += OnCharacterJobChanged;
        RegisterSubscription(() => _characterMonitor.OnCharacterJobChanged -= OnCharacterJobChanged);

        _characterMonitor.OnCharacterLoggedIn += OnCharacterMonitorOnOnCharacterLoggedIn;
        RegisterSubscription(() => _characterMonitor.OnCharacterLoggedIn -= OnCharacterMonitorOnOnCharacterLoggedIn);

        _characterMonitor.OnCharacterLoggedOut += OnCharacterMonitorOnOnCharacterLoggedOut;
        RegisterSubscription(() => _characterMonitor.OnCharacterLoggedOut -= OnCharacterMonitorOnOnCharacterLoggedOut);
    }

    private void OnCharacterMonitorOnOnCharacterLoggedOut(ulong id)
    {
        AddLog($"Character logged out: {id}");
    }

    private void OnCharacterMonitorOnOnCharacterLoggedIn(ulong id)
    {
        AddLog($"Character logged in: {id}");
    }

    private void OnCharacterJobChanged()
    {
        AddLog($"Character job changed");
    }

    private void OnCharacterMonitorOnOnCharacterRemoved(ulong id)
    {
        AddLog($"Character removed: {id}");
    }

    private void OnCharacterMonitorOnOnCharacterUpdated(Character? c)
    {
        AddLog($"Character updated: {c}");
    }

    private void OnCharacterMonitorOnOnActiveHouseChanged(ulong houseId, sbyte wardId, sbyte plotId, byte divisionId, short roomId, bool hasHousePermission)
    {
        AddLog($"Active house changed: {houseId}, {wardId}, {plotId}, {divisionId}, {roomId}, {hasHousePermission}");
    }

    private void OnCharacterMonitorOnOnActiveFreeCompanyChanged(ulong c)
    {
        AddLog($"Active FC changed: {c}");
    }

    private void OnCharacterMonitorOnOnActiveRetainerLoaded(ulong c)
    {
        AddLog($"Active retainer loaded: {c}");
    }

    private void OnCharacterMonitorOnOnActiveRetainerChanged(ulong c)
    {
        AddLog($"Active retainer changed: {c}");
    }

    public override unsafe void DrawInfo()
    {
        if (ImGui.CollapsingHeader("工作階段／目前狀態###Session / Active State"))
        {
            ImGui.TextUnformatted($"Is Logged In: {_characterMonitor.IsLoggedIn}");
            ImGui.TextUnformatted($"Local Content ID: {_characterMonitor.LocalContentId}");
            ImGui.TextUnformatted($"Internal Character ID: {_characterMonitor.InternalCharacterId}");

            ImGui.Separator();
            ImGui.TextUnformatted("目前角色：");
            ImGui.TextUnformatted(_characterMonitor.ActiveCharacter != null
                ? $"{_characterMonitor.ActiveCharacter.Name} ({_characterMonitor.ActiveCharacterId})"
                : "<none>");

            ImGui.TextUnformatted("目前雇員：");
            ImGui.TextUnformatted(_characterMonitor.ActiveRetainer != null
                ? $"{_characterMonitor.ActiveRetainer.Name} ({_characterMonitor.ActiveRetainerId})"
                : "<none>");

            ImGui.TextUnformatted("目前部隊：");
            ImGui.TextUnformatted(_characterMonitor.ActiveFreeCompany != null
                ? $"{_characterMonitor.ActiveFreeCompany.Name} ({_characterMonitor.ActiveFreeCompanyId})"
                : "<none>");
        }

        if (ImGui.CollapsingHeader("房屋###Housing"))
        {
            ImGui.TextUnformatted($"Active House ID: {_characterMonitor.ActiveHouseId}");
            ImGui.TextUnformatted($"Cached Ward Id: {_characterMonitor.InternalWardId}");
            ImGui.TextUnformatted($"Cached Plot Id: {_characterMonitor.InternalPlotId}");
            ImGui.TextUnformatted($"Cached Division Id: {_characterMonitor.InternalDivisionId}");
            ImGui.TextUnformatted($"Cached Room Id: {_characterMonitor.InternalRoomId}");
            ImGui.TextUnformatted($"Cached House Id: {_characterMonitor.InternalHouseId}");
            ImGui.TextUnformatted($"Territory Type Id: {_characterMonitor.CorrectedTerritoryTypeId}");

            var hm = HousingManager.Instance();
            if (hm != null)
            {
                if (hm->OutdoorTerritory != null)
                    ImGui.TextUnformatted($"Outdoor HouseId: {hm->OutdoorTerritory->HouseId.Id}");
                if (hm->IndoorTerritory != null)
                    ImGui.TextUnformatted($"Indoor HouseId: {hm->IndoorTerritory->HouseId.Id}");
                if (hm->CurrentTerritory != null)
                    ImGui.TextUnformatted($"Current Territory: {(ulong)hm->CurrentTerritory:X}");
            }

            ImGui.Separator();
            ImGui.TextUnformatted("擁有的房屋：");
            foreach (var id in _characterMonitor.GetOwnedHouseIds())
                ImGui.BulletText(id.ToString());

            ImGui.TextUnformatted("具有房屋權限：" +
                (_characterMonitor.InternalHasHousePermission ||
                 _characterMonitor.GetOwnedHouseIds().Contains(_characterMonitor.InternalHouseId)
                    ? "Yes"
                    : "No"));
        }

        //
        // Worlds
        //
        if (ImGui.CollapsingHeader("伺服器###Worlds"))
        {
            foreach (var wid in _characterMonitor.GetWorldIds())
                ImGui.BulletText($"World {wid}");
        }

        if (ImGui.CollapsingHeader("角色###Characters"))
        {
            foreach (var kv in _characterMonitor.Characters)
                ImGui.BulletText($"{kv.Key}: {kv.Value.Name}");
        }

        if (ImGui.CollapsingHeader("雇員###Retainers"))
        {
            using (var table = ImRaii.Table("retainerTable", 6))
            {
                if (table)
                {
                    ImGui.TableSetupColumn("雇用順序###Hire Order");
                    ImGui.TableSetupColumn("名稱###Name");
                    ImGui.TableSetupColumn("類型###Type");
                    ImGui.TableSetupColumn("金幣###Gil");
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("持有者 ID###Owner ID");
                    ImGui.TableHeadersRow();

                    foreach (var retainer in _characterMonitor.GetRetainerCharacters())
                    {
                        if (retainer.Value.Name == "Unhired")
                            continue;

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted((retainer.Value.HireOrder + 1).ToString());

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(retainer.Value.CharacterType == CharacterType.Housing
                            ? retainer.Value.HousingName
                            : retainer.Value.Name);

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(retainer.Value.CharacterType.ToString());

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(retainer.Value.Gil.ToString());

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(retainer.Value.CharacterId.ToString());

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(retainer.Value.OwnerId.ToString());
                    }
                }
            }
        }

        if (ImGui.CollapsingHeader("角色物件###Character Objects"))
        {
            foreach (var kv in _characterMonitor.Characters)
            {
                var label = kv.Value.CharacterType == CharacterType.Housing
                    ? kv.Value.HousingName
                    : kv.Value.Name;

                if (ImGui.TreeNode($"{label}##{kv.Key}"))
                {
                    Utils.PrintOutObject(kv.Value, 0, new List<string>());
                    ImGui.TreePop();
                }
            }
        }

        if (ImGui.CollapsingHeader("已取得物品###Acquired Items"))
        {
            foreach (var characterPair in _configuration.AcquiredItems)
            {
                var character = _characterMonitor.GetCharacterById(characterPair.Key);
                ImGui.TextUnformatted(character?.FormattedName ?? "Unknown Character");
                ImGui.Text($"{characterPair.Value.Count} unlocked items");
            }
        }
    }
}