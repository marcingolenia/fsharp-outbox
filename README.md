![Build & Test](https://github.com/marcingolenia/fsharp-outbox/workflows/Build%20&%20Test/badge.svg) ![F#](https://img.shields.io/badge/Made%20with-F%23-blue)
# fsharp-outbox

This is a sample implementation of the transactional outbox pattern and polling publisher pattern in F# language. Infrastructure pieces are:
* PostgreSQL 
* RabbitMq with Rebus

The related blog-post can be found here: https://mcode.it/blog/2021-02-18-fsharp_outbox/ 

# Instructions
* You will need [.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0) to build, test, run the app.
* Use `docker-compose up -d` to setup postgres database and rabbitmq on alpine with management plugin for development/testing purposes.

