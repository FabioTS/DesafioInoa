# Desafio Inoa C#

## Requisitos

O objetivo do sistema é avisar, via e-mail, caso a cotação de um ativo da B3 caia mais do que certo nível, ou suba acima de outro.

O programa deve ser uma aplicação de console (não há necessidade de interface gráfica).

Ele deve ser chamado via linha de comando com 3 parâmetros.

1. O ativo a ser monitorado
2. O preço de referência para venda
3. O preço de referência para compra

Ex:

    > stock-quote-alert.exe PETR4 22.67 22.59

Ele deve ler de um arquivo de configuração com:

1. O e-mail de destino dos alertas
2. As configurações de acesso ao servidor de SMTP que irá enviar o e-mail

A escolha da API de cotação é livre.

O programa deve ficar continuamente monitorando a cotação do ativo enquanto estiver rodando.

Em outras palavras, dada a cotação de PETR4 abaixo.

![Image](./image.png)

Toda vez que o preço for maior que linha-azul, um e-mail deve ser disparado aconselhando a venda.

Toda vez que o preço for menor que linha-vermelha, um e-mail deve ser disparado aconselhando a compra.

Features extras são bem vindas, boas práticas e patterns também ;)

## Pré requisitos

Faça o download da última versão .NET [.NET 5.0 SDK](https://dotnet.microsoft.com/download)

[Saiba mais sobre .NET](https://docs.microsoft.com/dotnet/fundamentals/)

Verifique a versão do .NET executando:

    > dotnet --version
## Instalando e usando

Para fazer uso correto da aplicação, é possível executar principalmente de duas formas:

Na pasta raiz, execute:

    > cd DesafioInoa.App
    > dotnet run PETR4 22.67 22.59

ou,

    > dotnet publish DesafioInoa.App\ --output .\app\ --runtime win-x64 --configuration Release -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true
    > cd .\app\
    > .\DesafioInoa.App.exe PETR4 22.67 22.59

## API

Foi desenvolvida uma API simples que pode ser executada da seguinte forma:

    > cd DesafioInoa.Api
    > dotnet run

Enquanto a API estiver em execução, é possível acessar sua documentação pelo Swagger.

> <http://localhost:5000/swagger>

## Autores

* **Fábio Trevizolo** - *Desenvolvedor* - [FabioTS](https://github.com/FabioTS)
