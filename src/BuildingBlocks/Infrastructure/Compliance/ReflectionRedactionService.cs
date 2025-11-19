using System.Reflection;
using BuildingBlocks.Common.Security;

namespace BuildingBlocks.Infrastructure.Compliance;

public class ReflectionRedactionService : IRedactionService
{
    public object Redact(object instance)
    {
        if (instance is null) return string.Empty;
        var type = instance.GetType();
        var clone = Activator.CreateInstance(type);
        if (clone == null) return instance; // fallback
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            var value = prop.GetValue(instance);
            if (prop.GetCustomAttribute<ContainsPHIAttribute>() != null && value is string)
            {
                prop.SetValue(clone, "***REDACTED***");
            }
            else
            {
                prop.SetValue(clone, value);
            }
        }
        return clone;
    }
}
