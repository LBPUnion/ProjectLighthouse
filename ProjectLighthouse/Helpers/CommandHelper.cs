using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.CommandLine;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public static class CommandHelper
    {
        public static List<ICommand> Commands { get; }

        static CommandHelper()
        {
            Commands = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(ICommand)) && t.GetConstructor(Type.EmptyTypes) != null)
                .Select(t => Activator.CreateInstance(t) as ICommand)
                .ToList();
        }

        public static async Task RunCommand(string[] args)
        {
            if (args.Length < 1)
            {
                throw new Exception
                (
                    "This should never happen. " +
                    "If it did, its because you tried to run a command before validating that the user actually wants to run one."
                );
            }

            string baseCmd = args[0];
            args = args.Skip(1).ToArray();

            IEnumerable<ICommand> suitableCommands = Commands.Where
                    (command => command.Aliases().Any(a => a.ToLower() == baseCmd.ToLower()))
                .Where(command => args.Length >= command.RequiredArgs());
            foreach (ICommand command in suitableCommands)
            {
                Console.WriteLine("Running command " + command.Name());
                await command.Run(args);
                return;
            }

            Console.WriteLine("Command not found.");
        }
    }
}