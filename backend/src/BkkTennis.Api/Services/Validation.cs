using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace BkkTennis.Api.Services;

/// <summary>At most N bytes when UTF-8 encoded (bcrypt silently ignores everything after byte 72, so longer passwords must be refused).</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class MaxUtf8BytesAttribute(int maxBytes) : ValidationAttribute
{
    public override bool IsValid(object? value) => value is not string s || System.Text.Encoding.UTF8.GetByteCount(s) <= maxBytes;

    public override string FormatErrorMessage(string name) => $"The {name} field must be at most {maxBytes} bytes (UTF-8).";
}

/// <summary>Not null, not an empty Guid / blank string / empty collection.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NotEmptyAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value switch
    {
        null => false,
        Guid g => g != Guid.Empty,
        string s => !string.IsNullOrWhiteSpace(s),
        ICollection c => c.Count > 0,
        _ => true,
    };

    public override string FormatErrorMessage(string name) => $"The {name} field is required.";
}

public static class ValidationExtensions
{
    /// <summary>
    /// Validates the endpoint's request DTO. Add it AFTER any authorization filters on the endpoint (filters run in the order
    /// added, outer group filters first) so that callers who may not use the endpoint get 401/403, not validation details.
    /// </summary>
    public static TBuilder WithValidation<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter<TBuilder, ValidationFilter>();
}

/// <summary>
/// Validates request DTOs with DataAnnotations (recursing into nested objects and lists) and answers
/// 400 <c>{ code: "validation_failed", message, errors: { "slots[0].courtId": ["…"] } }</c>. Domain rules stay in the services.
/// Only arguments whose type carries validation attributes are inspected, so services and HttpContext are never touched.
/// </summary>
public sealed class ValidationFilter : IEndpointFilter
{
    private static readonly ConcurrentDictionary<Type, bool> HasRulesCache = new();

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var errors = new Dictionary<string, List<string>>();
        foreach (var arg in ctx.Arguments)
            if (arg is not null && HasRules(arg.GetType()))
                Validate(arg, "", errors);

        if (errors.Count == 0) return await next(ctx);
        return Results.Json(new { code = "validation_failed", message = "ข้อมูลไม่ครบถ้วนหรือไม่ถูกต้อง", errors }, statusCode: StatusCodes.Status400BadRequest);
    }

    private static void Validate(object target, string prefix, Dictionary<string, List<string>> errors)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(target, new ValidationContext(target), results, validateAllProperties: true);
        foreach (var r in results)
        {
            var members = r.MemberNames.Any() ? r.MemberNames : [""];
            foreach (var m in members)
            {
                var key = prefix + Camel(m);
                if (!errors.TryGetValue(key, out var list)) errors[key] = list = [];
                list.Add(r.ErrorMessage ?? "Invalid value.");
            }
        }

        foreach (var p in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.GetIndexParameters().Length > 0 || p.GetValue(target) is not { } value || value is string) continue;
            var name = prefix + Camel(p.Name);
            if (value is IEnumerable items)
            {
                var i = 0;
                foreach (var item in items)
                {
                    if (item is not null && HasRules(item.GetType())) Validate(item, $"{name}[{i}].", errors);
                    i++;
                }
            }
            else if (HasRules(value.GetType()))
                Validate(value, name + ".", errors);
        }
    }

    private static string Camel(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);

    /// <summary>True when the type (or a type nested in its properties / lists) has at least one validation attribute.</summary>
    private static bool HasRules(Type type) => HasRulesCache.GetOrAdd(type, t => HasRules(t, []));

    private static bool HasRules(Type type, HashSet<Type> seen)
    {
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)) return false;
        if (!seen.Add(type)) return false;
        if (type.IsArray) return HasRules(type.GetElementType()!, seen);
        if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
            return type.GetGenericArguments().Any(a => HasRules(a, seen));
        if (type.Namespace is not { } ns || !ns.StartsWith("BkkTennis")) return false; // Guid, DateOnly, framework types …

        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.GetCustomAttributes<ValidationAttribute>(inherit: true).Any()) return true;
            if (HasRules(p.PropertyType, seen)) return true;
        }
        return false;
    }
}
