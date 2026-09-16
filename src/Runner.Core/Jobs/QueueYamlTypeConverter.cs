using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Runner.Core.Jobs;

public sealed class QueueYamlTypeConverter : IYamlTypeConverter
{
  public bool Accepts(Type type) =>
    type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Queue<>);

  public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
  {
    Type itemType = type.GetGenericArguments()[0];
    Type listType = typeof(List<>).MakeGenericType(itemType);
    object? list = rootDeserializer(listType);

    Type queueType = typeof(Queue<>).MakeGenericType(itemType);
    return Activator.CreateInstance(queueType, list)!;
  }

  public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
  {
    Type itemType = type.GetGenericArguments()[0];
    Type listType = typeof(List<>).MakeGenericType(itemType);
    object? list = value is null ? null : Activator.CreateInstance(listType, value);
    serializer(list, listType);
  }
}
