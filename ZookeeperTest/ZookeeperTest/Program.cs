using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using org.apache.zookeeper;

namespace ZookeeperTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(async builder =>
                {
                    var zooKeeper = new ZooKeeper("localhost:2181", 30000, new ZooKeeperConfigWatcher());
                    var result = await zooKeeper.getDataAsync("/neate", new ZooKeeperConfigWatcher());
                    var json = Encoding.UTF8.GetString(result.Data);
                    var memoryJson = JsonConfigurationParser.Parse(json);

                    var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(memoryJson)
                        .Build();
                    
                    var databaseConfigSection = config.GetSection("database");
                    var databasePassword = databaseConfigSection.GetValue<string>("password");

                    builder.AddInMemoryCollection(memoryJson);
                })
                .UseStartup<Startup>();
    }
}