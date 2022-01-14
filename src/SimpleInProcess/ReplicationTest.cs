using SimpleInProcess.Client;
using SimpleInProcess.Common;
using SimpleInProcess.Server;
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

            var sa = world.SpawnNetworkActor<SubActor>();
            sa.X = 0.5f;
            sa.Y = 2.5f;
            sa.Z = 100.0f;
            actor.RefToSubActor = sa;
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values BEFORE replication:");

            Console.WriteLine("  (Server) MA.CustomId   = " + actor.CustomId.ToString());
            Console.WriteLine("  (Server) MA.SubActor.Y = " + (actor.RefToSubActor?.Y.ToString() ?? "null"));

            var mainActor = dummyReceiver.GetNetworkActor<Actor>(1);
            Console.WriteLine("  (Client) MA.CustomId   = " + mainActor?.CustomId.ToString() ?? "null");
            Console.WriteLine("  (Client) MA.SubActor.Y = " + (mainActor?.RefToSubActor?.Y.ToString() ?? "null"));

            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine("Manually invoking replication");
            world.ReplicateValues();
            Console.WriteLine();

            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values AFTER replication:");
            Console.WriteLine("  (Client) MA.CustomId   = " + mainActor?.CustomId.ToString() ?? "null");
            Console.WriteLine("  (Client) MA.SubActor.Y = " + (mainActor?.RefToSubActor?.Y.ToString() ?? "null"));
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine("Changing value 'CustomId' on server to 23456");
            Console.WriteLine();
            actor.CustomId = 23456;
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values BEFORE replication:");
            Console.WriteLine("  (Server) MA.CustomId = " + actor.CustomId.ToString());
            Console.WriteLine("  (Client) MA.CustomId = " + mainActor?.CustomId.ToString() ?? "null");
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine("Manually invoking replication");
            world.ReplicateValues();
            Console.WriteLine();

            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine(" Values AFTER replication:");
            Console.WriteLine("  (Server) MA.CustomId = " + actor.CustomId.ToString());
            Console.WriteLine("  (Client) MA.CustomId = " + mainActor?.CustomId.ToString() ?? "null");
            Console.WriteLine(" --------------------------------- ");
            Console.WriteLine();

            Console.WriteLine("ReplicationTest END");
        }
    }
}
