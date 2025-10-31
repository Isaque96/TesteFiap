CODE:
dotnet ef dbcontext scaffold Name=ConnectionStrings:Default Microsoft.EntityFrameworkCore.SqlServer \
--project ./AdmSchoolApp.Infrastructure/AdmSchoolApp.Infrastructure.csproj \
--startup-project ./AdmSchoolApp.Api/AdmSchoolApp.Api.csproj \
--context AdmSchoolDbContext \
--context-dir "." \
--output-dir "TempModels" \
--schema adm \
--force \
--data-annotations