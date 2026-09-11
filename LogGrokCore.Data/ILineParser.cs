using LogGrokCore.Data.Monikers;

namespace LogGrokCore.Data
{
    public interface ILineParser
    {
        bool TryParse(string input, int beginning, int length, in ParsedLineComponents components, out long timeTicks);

        ParseResult Parse(string input);
    }
}