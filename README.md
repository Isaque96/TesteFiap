CODE:
dotnet ef dbcontext scaffold Name=ConnectionStrings:Default Microsoft.EntityFrameworkCore.SqlServer \
--project ./AdmSchoolApp.Infrastructure/AdmSchoolApp.Infrastructure.csproj \
--startup-project ./AdmSchoolApp.Api/AdmSchoolApp.Api.csproj \
--context AdmSchoolDbContext \
--context-dir "./Contexts" \
--output-dir "../AdmSchoolApp.Domain/Entities" \
--schema adm \
--force \
--data-annotations