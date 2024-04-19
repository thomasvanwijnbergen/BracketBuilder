namespace BracketBuilder.Shared
{
    public static class ParsingExtention
    {
        public static int? IntTryParse(string? intStr)
        {
            if (intStr == null)
                return null;
            var success = int.TryParse(intStr, out int num);
            if (success) return num;
            return null;
        }
        public static Guid? GuidTryParse(string? guidStr)
        {
            if (guidStr == null)
                return null;
            var success = Guid.TryParse(guidStr,out Guid guid);
            if (success) return guid;
            return null;
        }
    }
}
