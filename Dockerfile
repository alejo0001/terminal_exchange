FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY NuGet.config ./
COPY *.props ./
COPY *.targets ./
COPY src/ ./src/
WORKDIR "/src/src/Presentation"
RUN dotnet restore "Presentation.csproj"
RUN dotnet build "Presentation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Presentation.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
ARG AZUREENV
ENV AZENV $AZUREENV
EXPOSE 80
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT dotnet CrmAPI.Presentation.dll --environment=$AZENV
