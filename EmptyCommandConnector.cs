using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EmptyCommand
{
    public static class EmptyCommandConnector
    {
        private static LinkedList<object> _bindedRecivers = new();

        public static void AddCommandReciver(this ICommandReciver reciver)
        {
            _bindedRecivers.AddLast(reciver);
        }

        public static void RemoveCommandReciver(this ICommandReciver reciver)
        {
            _bindedRecivers.Remove(reciver);
        }

        public static T CreateCommand<T>()
            where T: struct, ICommand
        {
            return CommandContext<T>.CommandImplementation;
        }

        internal static object GetCommandReciver(Type reciverType)
        {
            var reciver = _bindedRecivers.First(x => x.GetType().Equals(reciverType));

            if (reciver == null)
                throw new NullReferenceException($"Не найден слушаетль комманд типа - {reciverType.Name}");

            return reciver;
        }

        private static class CommandContext<T>
            where T : struct, ICommand
        {
            private static object Command;
            private static MethodBase CloneMethod;
            private static int CommandID;

            static CommandContext()
            {
                Command = Activator.CreateInstance(typeof(T));

                var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].FieldType.GetInterface(nameof(ICommandReciver)) != null)
                    {
                        fields[i].SetValue(Command, Convert.ChangeType(GetCommandReciver(fields[i].FieldType), fields[i].FieldType));
                    }
                }

                var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                for (int i = 0; i < properties.Length; i++)
                {
                    if(properties[i].PropertyType.GetInterface(nameof(ICommandReciver)) != null)
                    {
                        properties[i].SetValue(Command, Convert.ChangeType(GetCommandReciver(fields[i].FieldType), fields[i].FieldType));
                    }
                }

                CloneMethod = typeof(T).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
                var random = new Random();
                CommandID = random.Next(-999, 999);
                
            }

            public static T CommandImplementation
            {
                get
                {
                    var command = (T) CloneMethod.Invoke(Command, null);
                    command.ID = CommandID;
                    return command;
                }
            }
        }
    }
}
