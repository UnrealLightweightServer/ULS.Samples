using SimpleInProcess.Client;
using SimpleInProcess.Common;
using SimpleInProcess.Server;
using System.Text;
using ULS.Core;

namespace SimpleInProcess
{
    internal static class ReplicationTest
    {
        internal static void Test()
        {
            Console.WriteLine("ReplicationTest START");

            ClientWorld dummyReceiver = new ClientWorld();

            ServerWorld world = new ServerWorld();
            world.AddCommChannel(dummyReceiver);

            Actor actor = world.SpawnNetworkActor<Actor>();
            actor.CustomId = 4711;

            var sa = world.SpawnNetworkActor<SubActor>();
            sa.X = 0.5f;
            sa.Y = 2.5f;
            sa.Z = 100.0f;
            actor.RefToSubActor = sa;

            Console.WriteLine(" --------------------------------- ");

            Console.WriteLine("(S) MA.CustomId = " + actor.CustomId.ToString());
            Console.WriteLine("(S) MA.SA.Y = " + (actor.RefToSubActor?.Y.ToString() ?? "null"));

            var mainActor = dummyReceiver.GetNetworkActor<Actor>(1);
            Console.WriteLine("(C) MA.CustomId = " + mainActor?.CustomId.ToString() ?? "null");
            Console.WriteLine("(C) MA.SA.Y = " + (mainActor?.RefToSubActor?.Y.ToString() ?? "null"));

            Console.WriteLine(" --------------------------------- ");

            world.ReplicateValues();

            Console.WriteLine(" ================================= ");
            Console.WriteLine(" ================================= ");
            Console.WriteLine("  CLIENT ");
            Console.WriteLine(" ================================= ");
            Console.WriteLine(" ================================= ");
            
            Console.WriteLine("(C) MA.CustomId = " + mainActor?.CustomId.ToString() ?? "null");
            Console.WriteLine("(C) MA.SA.Y = " + (mainActor?.RefToSubActor?.Y.ToString() ?? "null"));

            actor.CustomId = 23456;
            Console.WriteLine("(S) MA.CustomId = " + actor.CustomId.ToString());
            Console.WriteLine("(C) MA.CustomId = " + mainActor?.CustomId.ToString() ?? "null");

            world.ReplicateValues();
            Console.WriteLine("(S) MA.CustomId = " + actor.CustomId.ToString());
            Console.WriteLine("(C) MA.CustomId = " + mainActor?.CustomId.ToString() ?? "null");

            Console.WriteLine("ReplicationTest END");
        }
    }
}
