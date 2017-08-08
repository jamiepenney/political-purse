FROM microsoft/aspnetcore-build:1.1.2

RUN mkdir /app

COPY ./CHECKS /app/CHECKS
COPY ./PoliticalPurse.Web /app/PoliticalPurse.Web

WORKDIR /app/PoliticalPurse.Web

RUN /usr/local/bin/npm install --production && /usr/local/bin/npm run build && rm -rf node_modules

RUN dotnet restore
RUN dotnet build --configuration Release

EXPOSE 5000
CMD ["dotnet", "run", "--configuration", "Release"]
