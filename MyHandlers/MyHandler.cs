using System;
using System.Threading.Tasks;
using MyMessages;
using Rebus.Handlers;

namespace MyHandlers
{
    public class MyHandler : IHandleMessages<MyMessage>
    {
        public async Task Handle(MyMessage message)
        {
            Console.WriteLine("Handled!");
        }
    }
}
