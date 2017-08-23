#!bin/bash

dotnet restore test/shoppingcart.Tests/shoppingcart.Tests.csproj
dotnet test test/shoppingcart.Tests/shoppingcart.Tests.csproj
dotnet restore src/shoppingcart/shoppingcart.csproj
dotnet publish src/shoppingcart/shoppingcart.csproj -c release -o $(pwd)/publish/
docker stop shoppingcart || true && docker rm shoppingcart || true
docker build -t shoppingcart publish
docker run -d --name shoppingcart -p 8007:80 shoppingcart