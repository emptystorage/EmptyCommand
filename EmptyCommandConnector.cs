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
            static CommandContext()
            {
                object command = Activator.CreateInstance(typeof(T));

                var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].FieldType.GetInterface(nameof(ICommandReciver)) != null)
                    {
                        fields[i].SetValue(command, Convert.ChangeType(GetCommandReciver(fields[i].FieldType), fields[i].FieldType));
                    }
                }

                var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                for (int i = 0; i < properties.Length; i++)
                {
                    if(properties[i].PropertyType.GetInterface(nameof(ICommandReciver)) != null)
                    {
                        properties[i].SetValue(command, Convert.ChangeType(GetCommandReciver(fields[i].FieldType), fields[i].FieldType));
                    }
                }

                CommandImplementation = (T)command;
            }

            public static T CommandImplementation { get; }
        }
    }
}
