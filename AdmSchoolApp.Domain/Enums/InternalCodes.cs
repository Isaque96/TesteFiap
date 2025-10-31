using AdmSchoolApp.Domain.Attributes;

namespace AdmSchoolApp.Domain.Enums;

public enum InternalCodes
{
    [CodeSpecification("US1900", "Requisição mal formada")]
    MalformedRequest,
    [CodeSpecification("US9900", "Usuário não autorizado")]
    UnauthorizedRequest,
    [CodeSpecification("CE9999", "Erro interno da aplicação")]
    InternalError
}
