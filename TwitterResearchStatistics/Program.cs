using Microsoft.Data.Sqlite;
using Quartz;
using Serilog;
using TwitterResearchStatistics.DAL;
using TwitterResearchStatistics.Twitter;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();


// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;

services.AddSingleton<ITwitterClient>(provider =>
    new TwitterClient(
        configuration["Twitter:ApiKey"],
        configuration["Twitter:ApiKeySecret"],
        configuration["Twitter:BearerToken"],
        provider.GetService<CollectionPersistence>()
    )
);

services.AddSingleton<CollectionPersistence>();

// database store
var connString = builder.Configuration.GetConnectionString("Local");
var connectionStringBuilder = new SqliteConnectionStringBuilder();

//Use DB in project directory.  If it does not exist, create it:
connectionStringBuilder.ConnectionString = connString;
services.AddSingleton<ITwitterContext>(new TwitterContext(connectionStringBuilder));

// configure Quartz scheduler which will run the scheduled pull from Twitter
services.Configure<QuartzOptions>(options =>
{
    options.Scheduling.IgnoreDuplicates = true; // default: false
    options.Scheduling.OverWriteExistingData = true; // default: true
});

services.AddQuartz(q =>
{
    // base quartz scheduler, job and trigger configuration
    q.UseMicrosoftDependencyInjectionJobFactory();
    // overwrite to only have 1 job running, when expanding to greater load possibly have this value come from config settings to be dynamic and can change concurrency on the fly
    q.UseDefaultThreadPool(tp =>
    {
        // can increase this to expand capacity when needed
        tp.MaxConcurrency = 2;
    });

    // create the scheduled job
    // limited to 50 requests per 15 minutes - about every 20 seconds
    q.ScheduleJob<TwitterSampleStreamJob>(trigger => trigger
            .WithIdentity("Twitter Sample Stream Configuration Trigger")
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInSeconds(20).RepeatForever())
            .WithDescription("Scheduled job that will pull available sample tweets from Twitter")
        );

    q.ScheduleJob<TwitterQueueConsumerJob>(trigger => trigger
        .WithIdentity("Twitter Queue Consumer Trigger")
        .StartNow()
        .WithSimpleSchedule(x => x.WithIntervalInSeconds(15).RepeatForever())
        .WithDescription("Scheduled job process the queue of tweets and persist them")
    );
    
    q.UseInMemoryStore();
    // if wanting to use a persistent store could use SQLite or sql server
    //q.UsePersistentStore(s =>
    //{
        
    //    s.UseProperties = true;
    //    s.RetryInterval = TimeSpan.FromSeconds(5);
    //    s.UseSQLite(sqlite =>
    //    {
    //        sqlite.ConnectionString = connectionStringBuilder.ConnectionString;
    //        // this is the default
    //        sqlite.TablePrefix = "qrtz_";
    //    });
    //    s.UseJsonSerializer();
    //});
});

// ASP.NET Core hosting
services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});

services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
