# استخدم صورة .NET 10 SDK للبناء
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

# نسخ csproj أولاً لتسريع restore
COPY *.csproj ./
RUN dotnet restore

# نسخ كل الملفات وبناء المشروع
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# صورة runtime لتشغيل المشروع
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

#Expose port
EXPOSE 8080

# تشغيل التطبيق
ENTRYPOINT ["dotnet", "Yoser_API.dll"]
