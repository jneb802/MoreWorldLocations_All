using MoreWorldLocations.TestAdapter;
using Xunit;
public class PortDeliveryStateTests
{
    public class Legacy { public bool m_hasOpenDelivery = true; }
    public class LegacyFalse { public bool m_hasOpenDelivery; }
    public class LegacyUnset { public object? m_hasOpenDelivery; }
    [Fact] public void LegacyFieldDoesNotConsultSavedState() => Assert.True(PortDeliveryState.Read(new Legacy(),typeof(Legacy).GetField("m_hasOpenDelivery"),()=>throw new Exception("Unexpected fallback")));
    [Fact] public void LegacyFalseRemainsFalse() => Assert.False(PortDeliveryState.Read(new LegacyFalse(),typeof(LegacyFalse).GetField("m_hasOpenDelivery"),()=>throw new Exception("Unexpected fallback")));
    [Fact] public void LegacyNullIsUnknownNotFalse() => Assert.Null(PortDeliveryState.Read(new LegacyUnset(),typeof(LegacyUnset).GetField("m_hasOpenDelivery"),()=>throw new Exception("Unexpected fallback")));
    [Theory] [InlineData(true)] [InlineData(false)] [InlineData(null)]
    public void MissingLegacyFieldUsesSavedStateAndPreservesUnknown(bool? saved) => Assert.Equal(saved,PortDeliveryState.Read(new object(),null,()=>saved));
    [Theory] [InlineData(true)] [InlineData(false)]
    public void StoredSavedValueIsReadWhateverTheDefault(bool stored) => Assert.Equal(stored,PortDeliveryState.ReadSaved(_=>stored));
    [Fact] public void AbsentSavedKeyIsUnknownNotFalse() => Assert.Null(PortDeliveryState.ReadSaved(fallback=>fallback));
}
