using LogGrokX.Data.Monikers;

namespace LogGrokX.Data
{
    public interface ILineParser
    {
        bool TryParse(string input, int beginning, int length, in ParsedLineComponents components, out long timeTicks);

        ParseResult Parse(string input);
    }
}