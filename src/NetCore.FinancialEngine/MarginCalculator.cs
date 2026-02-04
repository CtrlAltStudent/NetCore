using System.Collections.Generic;
using System.Linq;
using NetCore.FinancialEngine.Models;

namespace NetCore.FinancialEngine;

public class MarginCalculator : IMarginCalculator
{
    public MarginResult Calculate(
        Guid periodId,
        IReadOnlyList<RevenueInput> revenues,
        IReadOnlyList<CostInput> costs,
        IReadOnlyDictionary<Guid, string>? channelNames = null,
        IReadOnlyDictionary<Guid, string>? departmentNames = null)
    {
        channelNames ??= new Dictionary<Guid, string>();
        departmentNames ??= new Dictionary<Guid, string>();

        var totalRevenue = revenues.Sum(r => r.Amount);
        var totalCosts = costs.Sum(c => c.Amount);
        var operatingProfit = totalRevenue - totalCosts;
        var marginPercent = totalRevenue > 0 ? (operatingProfit / totalRevenue) * 100 : 0;

        // Allocate costs by assignment weights/amounts to channel and department
        var costByChannel = new Dictionary<Guid, decimal>();
        var costByDepartment = new Dictionary<Guid, decimal>();

        foreach (var cost in costs)
        {
            if (cost.Assignments.Count == 0)
                continue;

            decimal totalWeight = 0;
            foreach (var a in cost.Assignments)
            {
                if (a.Weight.HasValue)
                    totalWeight += a.Weight.Value;
                else if (a.Amount.HasValue)
                    totalWeight += 1; // treat as one "share" for proportional split
            }

            if (totalWeight <= 0)
                continue;

            foreach (var a in cost.Assignments)
            {
                decimal portion = 0;
                if (a.Amount.HasValue)
                    portion = a.Amount.Value;
                else if (a.Weight.HasValue && totalWeight > 0)
                    portion = cost.Amount * (a.Weight.Value / totalWeight);

                if (a.SalesChannelId.HasValue)
                    costByChannel[a.SalesChannelId.Value] = costByChannel.GetValueOrDefault(a.SalesChannelId.Value) + portion;
                if (a.DepartmentId.HasValue)
                    costByDepartment[a.DepartmentId.Value] = costByDepartment.GetValueOrDefault(a.DepartmentId.Value) + portion;
            }
        }

        // Revenue by channel (only channels with non-null ChannelId; revenue with null channel counts in total only)
        var revenueByChannel = revenues
            .Where(r => r.ChannelId.HasValue)
            .GroupBy(r => r.ChannelId)
            .Where(g => g.Key.HasValue)
            .ToDictionary(g => g.Key!.Value, g => g.Sum(x => x.Amount));

        foreach (var ch in revenueByChannel.Keys.ToList())
            if (!costByChannel.ContainsKey(ch))
                costByChannel[ch] = 0;

        var byChannel = revenueByChannel.Select(kv => new MarginByChannel
        {
            ChannelId = kv.Key,
            ChannelName = channelNames.TryGetValue(kv.Key, out var name) ? name : null,
            Revenue = kv.Value,
            Costs = costByChannel.GetValueOrDefault(kv.Key),
            Profit = kv.Value - costByChannel.GetValueOrDefault(kv.Key),
            MarginPercent = kv.Value > 0 ? ((kv.Value - costByChannel.GetValueOrDefault(kv.Key)) / kv.Value) * 100 : 0
        }).ToList();

        // By department: we don't have revenue per department in input; we can only report costs per department
        // and total company revenue. So ByDepartment shows cost allocation and 0 revenue unless we add revenue-to-department later.
        var byDepartment = costByDepartment.Select(kv => new MarginByDepartment
        {
            DepartmentId = kv.Key,
            DepartmentName = departmentNames.TryGetValue(kv.Key, out var name) ? name : null,
            Revenue = 0,
            Costs = kv.Value,
            Profit = -kv.Value,
            MarginPercent = 0
        }).ToList();

        return new MarginResult
        {
            TotalRevenue = totalRevenue,
            TotalCosts = totalCosts,
            OperatingProfit = operatingProfit,
            MarginPercent = marginPercent,
            ByChannel = byChannel,
            ByDepartment = byDepartment
        };
    }
}
