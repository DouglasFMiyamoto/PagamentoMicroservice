FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Definir a imagem base para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos do projeto
COPY ["PagamentoMicroservice/PagamentoMicroservice.csproj", "./"]
RUN dotnet restore "PagamentoMicroservice.csproj"

# Copia todos os arquivos para o container
COPY PagamentoMicroservice/. .

WORKDIR "/src"
# Roda o build e publica
RUN dotnet publish "PagamentoMicroservice.csproj" -c Release -o /app/publish

FROM build AS publish
RUN dotnet publish "PagamentoMicroservice.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Definir variável de ambiente para o LocalStack
ENV AWS_SERVICE_URL=http://localstack:4566

# Define o ponto de entrada para o container
ENTRYPOINT ["dotnet", "PagamentoMicroservice.dll"]
