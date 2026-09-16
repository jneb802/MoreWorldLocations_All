using System;
using System.Linq;
using More_World_Locations_AIO.ServerOnly;
using More_World_Locations_AIO.ServerOnly.Verification;
using Xunit;

namespace More_World_Locations_AIO.Tests.Verification;

public sealed class ValidatorSelectionTests
{
    [Fact]
    public void APassingTemplateNeedsNoGrandfatheredNameOrShippedApproval()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        LocationDB.RegisterAll();
        Assert.True(world.IsInWorld("Review_NewBuild"));
        Assert.Equal(new[] { "MWL_MeadowsTomb4", "MWL_Ruins1", "MWL_RuinsWell1", "MWL_WoodTower2", "Review_NewBuild" },
            ServerOnlySelection.Registered.OrderBy(n => n, StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("MWL_MeadowsTomb4")]
    [InlineData("MWL_Ruins1")]
    [InlineData("MWL_RuinsWell1")]
    [InlineData("MWL_WoodTower2")]
    public void AFormerSpecialCaseHasNoExceptionToTheCurrentValidator(string name)
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        world.Asset(name).AddComponent<TestOnlyClientBehaviour>();
        LocationDB.RegisterAll();
        Assert.False(world.IsInWorld(name));
        Assert.False(CatalogueAudit.Report!.Find(name)!.Evaluation.Approved);
        Assert.True(world.IsInWorld("Review_NewBuild"));
    }

    [Fact]
    public void AValidationRequestCannotOverrideAFailedVerdict()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        world.Asset("Review_NewBuild").AddComponent<TestOnlyClientBehaviour>();
        string? before = Environment.GetEnvironmentVariable(ValidationSwitches.ApproveVariable);
        try
        {
            Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, "Review_NewBuild");
            LocationDB.RegisterAll();
            Assert.False(world.IsInWorld("Review_NewBuild"));
            Assert.True(CatalogueSweep.Enforce());
            Assert.False(ServerOnlySelection.IsOurs("Review_NewBuild"));
        }
        finally { Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, before); }
    }

    [Fact]
    public void FullModeRegistrationDoesNotUseServerOnlyVerdictsOrSubsets()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        world.Asset("Review_NewBuild").AddComponent<TestOnlyClientBehaviour>();
        string? before = Environment.GetEnvironmentVariable(ValidationSwitches.ApproveVariable);
        try
        {
            ServerOnlyMode.Set(false);
            Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, "MWL_Ruins1");
            LocationDB.RegisterAll();
            Assert.True(world.IsInWorld("Review_NewBuild"));
            Assert.Null(CatalogueAudit.Report);
        }
        finally { Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, before); }
    }

    [Fact]
    public void AValidationRequestOnlyNarrowsThePassingSet()
    {
        using var world = new TemplateWorld().WithPlainAssetsForEveryDefinition();
        string? before = Environment.GetEnvironmentVariable(ValidationSwitches.ApproveVariable);
        try
        {
            Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, "Review_NewBuild,NoSuchTemplate");
            LocationDB.RegisterAll();
            Assert.Equal(new[] { "Review_NewBuild" }, ServerOnlySelection.Registered);
            Assert.False(world.IsInWorld("MWL_Ruins1"));
        }
        finally { Environment.SetEnvironmentVariable(ValidationSwitches.ApproveVariable, before); }
    }
}
