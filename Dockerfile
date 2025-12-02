FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /App
COPY ./api ./
RUN dotnet restore
RUN dotnet publish -o out
WORKDIR /App/out
# COPY ./api/images ./images
ENTRYPOINT ["dotnet", "api.dll"]