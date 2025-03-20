FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# config listen port
ENV ASPNETCORE_URLS=http://+:8086

ARG APISettings__JwtOptions__Secret
ARG APISettings__JwtOptions__Issuer
ARG APISettings__JwtOptions__Audience
ARG OCELOT_BASE_URL
ARG OCELOT_ROUTES


ENV APISettings__JwtOptions__Secret=${APISettings__JwtOptions__Secret}
ENV APISettings__JwtOptions__Issuer=${APISettings__JwtOptions__Issuer}
ENV APISettings__JwtOptions__Audience=${APISettings__JwtOptions__Audience}
ENV OCELOT_BASE_URL=${OCELOT_BASE_URL}
ENV OCELOT_ROUTES=${OCELOT_ROUTES}


RUN echo "Variables cargadas: $RabbitMQ__Host"

EXPOSE 8086

ENTRYPOINT ["dotnet", "GatewaySolution.dll"]