FROM mcr.microsoft.com/dotnet/aspnet:7.0
USER root

# we need gettext-base for envsubst to work
RUN \
  apt-get update \
  && apt-get -y install gettext-base \
  && mkdir /files

ENV ASPNETCORE_URLS="http://*:5000"
ENV ASPNETCORE_ENVIRONMENT="Docker"
COPY . /webapi

RUN useradd dotnetuser --create-home --uid 1000 && \
  chown dotnetuser /webapi && \
  chown -R dotnetuser /webapi/wwwroot && \
  chmod +x /webapi/import-meta-env

WORKDIR /webapi
USER dotnetuser
ENTRYPOINT ./import-meta-env -x ./wwwroot/.env -e ./wwwroot/.env -o ./wwwroot/**/* && dotnet MccSoft.TemplateApp.App.dll
EXPOSE 5000
