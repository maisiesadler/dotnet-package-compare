FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

COPY . ./
RUN dotnet publish ./src/PackageLister.Console/PackageLister.Console.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Maisie Sadler <maisie.sadler>"
LABEL repository="https://github.com/maisiesadler/dotnet-package-compare"
LABEL homepage="https://github.com/maisiesadler/dotnet-package-compare"

LABEL com.github.actions.name="Dotnet package comparison"
LABEL com.github.actions.description="Shows packages changes for a PR."
LABEL com.github.actions.icon="activity"
LABEL com.github.actions.color="orange"

ARG BEFORE_FILE_LOCATION 
ENV BEFORE_FILE_LOCATION=$BEFORE_FILE_LOCATION
ARG AFTER_FILE_LOCATION
ENV AFTER_FILE_LOCATION=$AFTER_FILE_LOCATION

FROM mcr.microsoft.com/dotnet/runtime:7.0-bullseye-slim
COPY --from=build-env /out .
COPY --from=build-env ./runindocker.sh /runindocker.sh
ENTRYPOINT [ "/runindocker.sh" ]
