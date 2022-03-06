using LiteDB;
using Microsoft.Extensions.Options;

namespace OcrWaterMeter.Server.Database
{
    public class LiteDbContext
    {
        public readonly LiteDatabase Context;

        public LiteDbContext(IOptions<LiteDbConfig> configs)
        {
            try
            {
                Context = new LiteDatabase(configs.Value.DatabasePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Can find or create LiteDb database.", ex);
            }
        }
    }
}
