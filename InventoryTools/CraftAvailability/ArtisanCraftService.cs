using System;
using CriticalCommonLib.Services;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Microsoft.Extensions.Logging;

namespace InventoryTools.CraftAvailability;

public sealed class ArtisanCraftService
{
    private readonly ICallGateSubscriber<ushort, int, bool, object> _prepareAndCraft;
    private readonly ICallGateSubscriber<ushort, string> _hqPrediction;
    private readonly IChatUtilities _chatUtilities;
    private readonly ILogger<ArtisanCraftService> _logger;

    public ArtisanCraftService(IDalamudPluginInterface pluginInterface, IChatUtilities chatUtilities,
        ILogger<ArtisanCraftService> logger)
    {
        _prepareAndCraft = pluginInterface.GetIpcSubscriber<ushort, int, bool, object>("Artisan.PrepareAndCraft");
        _hqPrediction = pluginInterface.GetIpcSubscriber<ushort, string>("Artisan.GetHqPrediction");
        _chatUtilities = chatUtilities;
        _logger = logger;
    }

    public void PrepareAndCraft(uint recipeId, int amount, bool includeSubcrafts)
    {
        try
        {
            var checkedRecipeId = checked((ushort)recipeId);
            var prediction = _hqPrediction.InvokeFunc(checkedRecipeId);
            if (!prediction.StartsWith("SAFE|", StringComparison.Ordinal))
            {
                var reason = prediction.Contains('|') ? prediction[(prediction.IndexOf('|') + 1)..] : prediction;
                _chatUtilities.PrintError($"HQ 安全鎖：{reason}");
                return;
            }

            _prepareAndCraft.InvokeAction(checkedRecipeId, amount, includeSubcrafts);
            _chatUtilities.Print($"已交給 Artisan：補充僱員材料後切換職業並製作 {amount} 次。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not start recipe {RecipeId} through Artisan.", recipeId);
            _chatUtilities.PrintError("無法執行 HQ 預測或啟動製作；請重新載入新版 Artisan，並確認目前沒有正在製作或取料。");
        }
    }

    public string PredictHq(uint recipeId)
    {
        try
        {
            return _hqPrediction.InvokeFunc(checked((ushort)recipeId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not predict HQ for recipe {RecipeId} through Artisan.", recipeId);
            return "BLOCK|無法取得 HQ 預測；請重新載入新版 Artisan。";
        }
    }
}
