using SimpleInProcess;

bool done = false;

Console.WriteLine("UnrealLightweightServer");
Console.WriteLine("Simple InProcess sample project");
Console.WriteLine();

/*Console.WriteLine("Type exit to quit");

while (!done)
{
    string? cmd = Console.ReadLine();
    switch (cmd)
    {
        case "exit":
            done = true;
            continue;
    }
}*/

ReplicationTest.Test();

Console.WriteLine("Bye");