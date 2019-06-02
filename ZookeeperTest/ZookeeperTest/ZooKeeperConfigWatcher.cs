using System;
using System.Threading.Tasks;
using org.apache.zookeeper;

namespace ZookeeperTest
{
    public class ZooKeeperConfigWatcher: Watcher 
    {
        // This method will get invoked every time the ZooKeeper status or configuration values change.
        // This way you can dinamically react to changes in configuration.
        public override Task process(WatchedEvent @event)
        {
            Console.WriteLine("new event");
            return new Task( () => {} );
        }
    }
}