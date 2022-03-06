namespace OcrWaterMeter.Server.Database
{
    public static class LiteDbServiceExtention
    {
        public static void AddLiteDb(this IServiceCollection services, string databasePath)
        {
            //services.AddTransient<LiteDbContext, LiteDbContext>();
            services.AddSingleton<LiteDbContext, LiteDbContext>();
            services.Configure<LiteDbConfig>(options => options.DatabasePath = databasePath);
        }
    }

}
