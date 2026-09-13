using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

/// <summary>
/// 登入後分批預載持有、可交易物品的 Universalis 價格，避免首次查看提示時卡頓。
/// </summary>
public sealed class TwMarketPriceWarmupService : BackgroundService
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan LoginPollDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan BatchDelay = TimeSpan.FromSeconds(6);
    private static readonly TimeSpan FailureBackoff = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CacheMaxAge = TimeSpan.FromHours(24);
    private const int BatchSize = 10;
    private const int QueueHighWaterMark = 60;

    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IMarketCache _marketCache;
    private readonly IUniversalis _universalis;
    private readonly ItemSheet _itemSheet;
    private readonly ILogger<TwMarketPriceWarmupService> _logger;
    private readonly SemaphoreSlim _refreshSignal = new(0, 1);
    private int _forceNextRefresh;

    public string Phase { get; private set; } = "等待啟動";
    public int TotalItems { get; private set; }
    public int QueuedItems { get; private set; }
    public uint WorldId { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public TwMarketPriceWarmupService(
        IInventoryMonitor inventoryMonitor,
        ICharacterMonitor characterMonitor,
        IMarketCache marketCache,
        IUniversalis universalis,
        ItemSheet itemSheet,
        ILogger<TwMarketPriceWarmupService> logger)
    {
        _inventoryMonitor = inventoryMonitor;
        _characterMonitor = characterMonitor;
        _marketCache = marketCache;
        _universalis = universalis;
        _itemSheet = itemSheet;
        _logger = logger;
    }

    public string GetStatus()
    {
        var completed = CompletedAt.HasValue ? $"，上次完成 {CompletedAt.Value:T}" : string.Empty;
        return $"Allagan 市場價格預載：{Phase}，世界 {WorldId}，已排入 {QueuedItems}/{TotalItems}，Universalis 待處理 {_universalis.QueuedCount}{completed}。";
    }

    public bool RequestRefresh(bool force)
    {
        if (force)
            Interlocked.Exchange(ref _forceNextRefresh, 1);
        if (_refreshSignal.CurrentCount != 0)
            return false;
        _refreshSignal.Release();
        return true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Phase = "啟動延遲";
            await Task.Delay(InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested &&
                   (!_characterMonitor.IsLoggedIn || _characterMonitor.ActiveCharacter == null))
            {
                Phase = "等待登入";
                await Task.Delay(LoginPollDelay, stoppingToken);
            }

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            WorldId = _characterMonitor.ActiveCharacter?.WorldId ?? 0;
            if (WorldId == 0)
            {
                Phase = "無法取得所屬世界";
                return;
            }

            await QueueWarmup(false, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                Phase = "待命";
                await _refreshSignal.WaitAsync(stoppingToken);
                var force = Interlocked.Exchange(ref _forceNextRefresh, 0) == 1;
                await QueueWarmup(force, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            Phase = "已停止";
        }
        catch (Exception ex)
        {
            Phase = "失敗";
            _logger.LogError(ex, "TW market price warmup failed.");
        }
    }

    private async Task QueueWarmup(bool force, CancellationToken stoppingToken)
    {
        Phase = "掃描持有物品";
        QueuedItems = 0;
        CompletedAt = null;
        var cutoff = DateTime.Now.Subtract(CacheMaxAge);
        var itemIds = _inventoryMonitor.AllItems
            .Select(item => item.ItemId)
            .Where(itemId => itemId != 0)
            .Distinct()
            .Where(itemId => !(_itemSheet.GetRowOrDefault(itemId)?.Base.IsUntradable ?? true))
            .Where(itemId => force ||
                             !_marketCache.CachedPricing.TryGetValue((itemId, WorldId), out var pricing) ||
                             pricing.LastUpdate < cutoff || pricing.listings == null)
            .ToList();

        TotalItems = itemIds.Count;
        if (TotalItems == 0)
        {
            Phase = "快取已是最新";
            CompletedAt = DateTime.Now;
            return;
        }

        foreach (var batch in itemIds.Chunk(BatchSize))
        {
            stoppingToken.ThrowIfCancellationRequested();
            while (_universalis.QueuedCount >= QueueHighWaterMark ||
                   (_universalis.LastFailure.HasValue &&
                    DateTime.Now - _universalis.LastFailure.Value < FailureBackoff))
            {
                Phase = _universalis.TooManyRequests ? "Universalis 限流，暫停排入" : "Universalis 暫時忙碌，退避中";
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            Phase = force ? "強制更新價格" : "分批預載價格";
            var ids = new List<uint>(batch);
            _marketCache.RequestCheck(ids, WorldId, true);
            QueuedItems += ids.Count;
            await Task.Delay(BatchDelay, stoppingToken);
        }

        Phase = "排入完成";
        CompletedAt = DateTime.Now;
        _logger.LogInformation(
            "TW market price warmup completed for world {WorldId}: {QueuedItems} owned tradable items queued in batches of {BatchSize}.",
            WorldId, QueuedItems, BatchSize);
    }
}
