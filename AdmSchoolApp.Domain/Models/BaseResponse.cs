using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AdmSchoolApp.Domain.Attributes;
using AdmSchoolApp.Domain.Enums;

namespace AdmSchoolApp.Domain.Models;

public class BaseResponse<T>
{
    public BaseResponse() { }

    public BaseResponse(T data, string message, InternalCodes code, string[] errors)
    {
        Message = message;
        Data = data;
        Error = new BaseError(errors, code);
    }

    public BaseResponse(string message, T? data = default, BaseError? error = null)
    {
        Message = message;
        Data = data;
        Error = error;
    }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "Sucesso!";
    
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BaseError? Error { get; set; }
}

public class BaseError
{
    public BaseError() { }

    public BaseError(string[] errors, InternalCodes code)
    {
        Errors = errors;
        Code = code;
    }
    
    public void AddError(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        Errors = Errors.Append(error).ToArray();
    }
    
    [JsonPropertyName("traceId")]
    public Guid TraceId { get; } = Guid.NewGuid();
    
    [JsonPropertyName("errors")]
    public string[] Errors { get; set; } = [];
    
    [JsonPropertyName("code")]
    public InternalCodes Code { get; set; }
}

public sealed class InternalCodesJsonConverter : JsonConverter<InternalCodes>
{
    private static Dictionary<InternalCodes, string>? _enumToCode;
    private static Dictionary<string, InternalCodes>? _codeToEnum;

    public InternalCodesJsonConverter()
    {
        AssertConfigured();
    }

    private static void AssertConfigured()
    {
        var type = typeof(InternalCodes);
        var enumToCode = new Dictionary<InternalCodes, string>();
        var codeToEnum = new Dictionary<string, InternalCodes>(StringComparer.Ordinal);

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var enumValue = (InternalCodes)field.GetValue(null)!;
            var attr = field.GetCustomAttribute<CodeSpecificationAttribute>()
                ?? throw new InvalidOperationException($"'{type.Name}.{field.Name}' sem CodeSpecificationAttribute.");

            if (codeToEnum.ContainsKey(attr.Code!))
                throw new InvalidOperationException($"Código duplicado '{attr.Code}' em {type.Name}.{field.Name}.");

            enumToCode[enumValue] = attr.Code!;
            codeToEnum[attr.Code!] = enumValue;
        }

        _enumToCode = enumToCode;
        _codeToEnum = codeToEnum;
    }

    private static void EnsureReady()
    {
        if (_enumToCode is null || _codeToEnum is null)
            throw new InvalidOperationException("InternalCodesJsonConverter não foi inicializado. Chame AssertConfigured() no startup.");
    }

    public override InternalCodes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        EnsureReady();
        var code = reader.GetString();
        if (code is null || !_codeToEnum!.TryGetValue(code, out var value))
            throw new JsonException($"Código '{code}' não mapeado para {nameof(InternalCodes)}.");
        return value;
    }

    public override void Write(Utf8JsonWriter writer, InternalCodes value, JsonSerializerOptions options)
    {
        EnsureReady();
        if (!_enumToCode!.TryGetValue(value, out var code))
            throw new JsonException($"O valor '{value}' não possui CodeSpecificationAttribute.");
        writer.WriteStringValue(code);
    }
}