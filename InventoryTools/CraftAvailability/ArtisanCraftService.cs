using System;
using CriticalCommonLib.Services;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Microsoft.Extensions.Logging;

namespace InventoryTools.CraftAvailability;

public sealed class ArtisanCraftService
{
    private readonly ICallGateSubscriber<ushort, int, bool, object> _prepareAndCraftWithCraftimizer;
    private readonly ICallGateSubscriber<ushort, string> _startCraftimizerPrediction;
    private readonly ICallGateSubscriber<ushort, string> _getCraftimizerPrediction;
    private readonly IChatUtilities _chatUtilities;
    private readonly ILogger<ArtisanCraftService> _logger;

    public ArtisanCraftService(IDalamudPluginInterface pluginInterface, IChatUtilities chatUtilities,
        ILogger<ArtisanCraftService> logger)
    {
        _prepareAndCraftWithCraftimizer = pluginInterface.GetIpcSubscriber<ushort, int, bool, object>("Artisan.PrepareAndCraftWithCraftimizer");
        _startCraftimizerPrediction = pluginInterface.GetIpcSubscriber<ushort, string>("Artisan.StartCraftimizerHqPrediction");
        _getCraftimizerPrediction = pluginInterface.GetIpcSubscriber<ushort, string>("Artisan.GetCraftimizerHqPrediction");
        _chatUtilities = chatUtilities;
        _logger = logger;
    }

    public void PrepareAndCraft(uint recipeId, int amount, bool includeSubcrafts)
    {
        try
        {
            _prepareAndCraftWithCraftimizer.InvokeAction(checked((ushort)recipeId), amount, includeSubcrafts);
            _chatUtilities.Print($"已交給 Artisan／Craftimizer 2.11：背景預測通過後，才會補充僱員材料、切換職業並製作 {amount} 次。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not start recipe {RecipeId} through Artisan.", recipeId);
            _chatUtilities.PrintError("無法啟動 Craftimizer 2.11 製作；請重新載入成對新版 Artisan 與 Allagan Tools，並確認目前沒有正在製作或取料。");
        }
    }

    public string StartHqPrediction(uint recipeId)
    {
        try
        {
            return _startCraftimizerPrediction.InvokeFunc(checked((ushort)recipeId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not start Craftimizer HQ prediction for recipe {RecipeId} through Artisan.", recipeId);
            return "BLOCK|無法啟動 Craftimizer 2.11 預測；請重新載入成對新版 Artisan 與 Allagan Tools。";
        }
    }

    public string PollHqPrediction(uint recipeId)
    {
        try
        {
            return _getCraftimizerPrediction.InvokeFunc(checked((ushort)recipeId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not poll Craftimizer HQ prediction for recipe {RecipeId} through Artisan.", recipeId);
            return "BLOCK|無法取得 Craftimizer 2.11 預測結果；請重新載入成對新版 Artisan 與 Allagan Tools。";
        }
    }
}
