# Ocelot Gateway Solution

Este proyecto implementa un `API Gatewat` utilizando `Ocelot` en **.NET**. Permite la comunicacion hacia los diferentes microservicios, gestionando el enrutamiento y autenticacion del  sistema, y el respectivo llamado de cada endpoint

## Requisitos previos
- .NET 8 SDK

# Instrucciones de ejecucion
Para ejecutar el proyecto solo hay que asegurarse de instalar las dependencias necesarias, y configurar las diferentes variables de entorno requeridad para su ejecucion


# Configuracion de `QoSOptions`(Circuit Breaker)
Para evitar errores tipo `502`,  debido a que se utiliza un servicio gratuito para el despliegue se agrega la opcion `QoSOptions` a los diferentes endpoints que se utiliza para llamar el servicio, asegurando que el API Gateway no retorne inmediatamente un 502 debido a indisponibilidad del servicio, sino que se hagan reintentos 


# Microservicios
En el API Gateway se hace punto de entrada a los siguientes Microservicios:

- [UsersAuthorization](https://github.com/Daniel7R/UsersAuthorization_ms)
- [Transactions](https://github.com/Daniel7R/transactions_ms)
- [Tournament](https://github.com/Daniel7R/tournament_ms)
- [Tickets](https://github.com/Daniel7R/tickets_ms)
- [Streams](https://github.com/Daniel7R/streams_ms)