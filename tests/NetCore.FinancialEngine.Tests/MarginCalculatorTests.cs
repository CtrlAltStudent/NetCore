using NetCore.Domain.Enums;
using NetCore.FinancialEngine;
using NetCore.FinancialEngine.Models;
using Xunit;

namespace NetCore.FinancialEngine.Tests;

public class MarginCalculatorTests
{
    private readonly IMarginCalculator _calculator = new MarginCalculator();

    [Fact]
    public void Calculate_NoData_ReturnsZeroTotals()
    {
        var result = _calculator.Calculate(
            Guid.NewGuid(),
            new List<RevenueInput>(),
            new List<CostInput>());

        Assert.Equal(0, result.TotalRevenue);
        Assert.Equal(0, result.TotalCosts);
        Assert.Equal(0, result.OperatingProfit);
        Assert.Equal(0, result.MarginPercent);
    }

    [Fact]
    public void Calculate_RevenueOnly_ProfitEqualsRevenue()
    {
        var periodId = Guid.NewGuid();
        var revenues = new List<RevenueInput>
        {
            new() { ChannelId = null, PeriodId = periodId, Amount = 10000 },
            new() { ChannelId = null, PeriodId = periodId, Amount = 5000 }
        };

        var result = _calculator.Calculate(periodId, revenues, new List<CostInput>());

        Assert.Equal(15000, result.TotalRevenue);
        Assert.Equal(0, result.TotalCosts);
        Assert.Equal(15000, result.OperatingProfit);
        Assert.Equal(100, result.MarginPercent);
    }

    [Fact]
    public void Calculate_RevenueAndCosts_ComputesProfitAndMargin()
    {
        var periodId = Guid.NewGuid();
        var channelId = Guid.NewGuid();
        var revenues = new List<RevenueInput>
        {
            new() { ChannelId = channelId, PeriodId = periodId, Amount = 10000 }
        };
        var costs = new List<CostInput>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Type = CostType.Fixed,
                Name = "Rent",
                Amount = 2000,
                PeriodId = periodId,
                Assignments = new List<CostAssignmentInput> { new() { SalesChannelId = channelId, Weight = 1 } }
            }
        };
        var channelNames = new Dictionary<Guid, string> { { channelId, "Shop" } };

        var result = _calculator.Calculate(periodId, revenues, costs, channelNames);

        Assert.Equal(10000, result.TotalRevenue);
        Assert.Equal(2000, result.TotalCosts);
        Assert.Equal(8000, result.OperatingProfit);
        Assert.Equal(80, result.MarginPercent);
        var byChannel = result.ByChannel.Single();
        Assert.Equal(10000, byChannel.Revenue);
        Assert.Equal(2000, byChannel.Costs);
        Assert.Equal(8000, byChannel.Profit);
        Assert.Equal(80, byChannel.MarginPercent);
    }

    [Fact]
    public void Calculate_CostSplitByWeight_AllocatesCorrectly()
    {
        var periodId = Guid.NewGuid();
        var ch1 = Guid.NewGuid();
        var ch2 = Guid.NewGuid();
        var revenues = new List<RevenueInput>
        {
            new() { ChannelId = ch1, PeriodId = periodId, Amount = 6000 },
            new() { ChannelId = ch2, PeriodId = periodId, Amount = 4000 }
        };
        var costs = new List<CostInput>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Type = CostType.Variable,
                Name = "Marketing",
                Amount = 1000,
                PeriodId = periodId,
                Assignments = new List<CostAssignmentInput>
                {
                    new() { SalesChannelId = ch1, Weight = 0.6m },
                    new() { SalesChannelId = ch2, Weight = 0.4m }
                }
            }
        };
        var channelNames = new Dictionary<Guid, string> { { ch1, "A" }, { ch2, "B" } };

        var result = _calculator.Calculate(periodId, revenues, costs, channelNames);

        Assert.Equal(10000, result.TotalRevenue);
        Assert.Equal(1000, result.TotalCosts);
        Assert.Equal(9000, result.OperatingProfit);
        var byCh1 = result.ByChannel.First(c => c.ChannelId == ch1);
        var byCh2 = result.ByChannel.First(c => c.ChannelId == ch2);
        Assert.Equal(600, byCh1.Costs);
        Assert.Equal(400, byCh2.Costs);
    }
}
