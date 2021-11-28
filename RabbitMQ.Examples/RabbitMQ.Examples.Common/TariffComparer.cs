namespace TariffXml.Core;

public class TariffComparer : IComparer<Tariff>
{
    public int Compare(Tariff? x, Tariff? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        return string.Compare(x.Id, y.Id, StringComparison.Ordinal);
    }
}