using AdmSchoolApp.Domain.Attributes;

namespace AdmSchoolApp.Domain.Enums;

public enum InternalCodes
{
    [CodeSpecification("US1900", "Requisição mal formada")]
    MalformedRequest,
    
    [CodeSpecification("US9900", "Usuário não autorizado")]
    UnauthorizedRequest,
    
    [CodeSpecification("CE9999", "Erro interno da aplicação")]
    InternalError,
    
    [CodeSpecification("ST1001", "Aluno não encontrado")]
    StudentNotFound,
    
    [CodeSpecification("ST1002", "CPF já cadastrado")]
    CpfAlreadyExists,
    
    [CodeSpecification("ST1003", "Email já cadastrado")]
    EmailAlreadyExists,
    
    [CodeSpecification("CL1001", "Turma não encontrada")]
    ClassNotFound,
    
    [CodeSpecification("EN1001", "Matrícula não encontrada")]
    EnrollmentNotFound,
    
    [CodeSpecification("EN1002", "Aluno já matriculado nesta turma")]
    StudentAlreadyEnrolled,
    
    [CodeSpecification("US1001", "Usuário não encontrado")]
    UserNotFound,
    
    [CodeSpecification("VL1000", "Erro de validação")]
    ValidationError
}
