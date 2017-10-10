FROM microsoft/aspnetcore-build AS builder
WORKDIR /source

COPY . .
RUN dotnet test test/shoppingcart.Tests/shoppingcart.Tests.csproj
RUN dotnet publish src/shoppingcart/shoppingcart.csproj --output /app/ --configuration Release

FROM microsoft/aspnetcore
WORKDIR /app
COPY --from=builder /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "shoppingcart.dll"]