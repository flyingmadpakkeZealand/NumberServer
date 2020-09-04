using System;

namespace NumberServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NumberServerWorker server = new NumberServerWorker();
            server.Start();
        }
    }
}
