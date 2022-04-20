using SimpleInProcess.Client;
using SimpleInProcess.Common;
using SimpleInProcess.Server;
using System.Numerics;
using System.Text;
using ULS.Core;

namespace SimpleInProcess
{
    internal static class ReplicationTest
    {
        /// <summary>
        /// This test runs without user interaction and manually invokes the replication process.
        /// </summary>
        internal static void Test()
        {
            Console.WriteLine("ReplicationTest START");

            // Create client and server
            ClientWorld dummyReceiver = new ClientWorld();

            ServerWorld world = new ServerWorld();
            world.AddCommChannel(dummyReceiver);

            // Create test actors
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Spawning actors on Server and Client");
            Actor actor = world.SpawnNetworkActor<Actor>();
            actor.CustomId = 4711;
            actor.Counter = 10;

            var sa = world.SpawnNetworkActor<SubActor>();
            sa.Translation = new Vector3(0.5f, 2.5f, 100.0f);
            actor.RefToSubActor = sa;
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            var mainActor = dummyReceiver.GetNetworkObject<Actor>(actor.UniqueId);

            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values BEFORE replication:");
            Console.WriteLine("  (Server) MA.Counter    = " + actor.Counter.ToString());
            Console.WriteLine("  (Server) MA.SubActor.Y = " + (actor.RefToSubActor?.Translation.Y.ToString() ?? "null"));
            Console.WriteLine("  (Client) MA.Counter    = " + mainActor?.Counter.ToString() ?? "null");
            Console.WriteLine("  (Client) MA.SubActor.Y = " + (mainActor?.RefToSubActor?.Translation.Y.ToString() ?? "null"));

            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine("Manually invoking replication");
            world.ReplicateValues();
            Console.WriteLine();

            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values AFTER replication:");
            Console.WriteLine("  (Server) MA.Counter    = " + actor.Counter.ToString());
            Console.WriteLine("  (Server) MA.SubActor.Y = " + (actor.RefToSubActor?.Translation.Y.ToString() ?? "null"));
            Console.WriteLine("  (Client) MA.Counter    = " + mainActor?.Counter.ToString() ?? "null");
            Console.WriteLine("  (Client) MA.SubActor.Y = " + (mainActor?.RefToSubActor?.Translation.Y.ToString() ?? "null"));
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values BEFORE setting immediate member:");
            Console.WriteLine("  (Server) MA.CustomId = " + actor.CustomId.ToString());
            Console.WriteLine("  (Client) MA.CustomId = " + mainActor?.CustomId.ToString() ?? "null");
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();
            Console.WriteLine("Changing immeditate value 'CustomId' on server to 23456");
            actor.CustomId = 23456;
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values AFTER immediate replication:");
            Console.WriteLine("  (Server) MA.CustomId = " + actor.CustomId.ToString());
            Console.WriteLine("  (Client) MA.CustomId = " + mainActor?.CustomId.ToString() ?? "null");
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine("Changing non-immediate value 'Counter' on server to 1337");
            Console.WriteLine();
            actor.Counter = 1337;
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values BEFORE replication:");
            Console.WriteLine("  (Server) MA.Counter = " + actor.Counter.ToString());
            Console.WriteLine("  (Client) MA.Counter = " + mainActor?.Counter.ToString() ?? "null");
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine("Manually invoking replication");
            world.ReplicateValues();
            Console.WriteLine();

            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values AFTER replication:");
            Console.WriteLine("  (Server) MA.Counter = " + actor.Counter.ToString());
            Console.WriteLine("  (Client) MA.Counter = " + mainActor?.Counter.ToString() ?? "null");
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine("ReplicationTest END");
        }
    }
}
