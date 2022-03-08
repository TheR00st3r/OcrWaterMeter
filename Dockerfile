# -----------
# Build stage
# -----------

FROM debian:11 AS build
LABEL stage=builder
LABEL org.opencontainers.image.authors="Alexander@volzit.de"

#install prereqs
WORKDIR /
RUN apt-get update -qqy && DEBIAN_FRONTEND=noninteractive apt-get install -y \
    wget bash apt-transport-https
RUN wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt-get update -qqy && apt-get install -y dotnet-sdk-6.0
RUN eval apt-get clean && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

#get OcrWaterMeter and publish it
COPY OcrWaterMeter /OcrWaterMeter
WORKDIR /OcrWaterMeter
RUN dotnet publish ./Server/OcrWaterMeter.Server.csproj -c Release -p:PublishProfile=DefaultPublish

# -----------
# Final stage
# -----------

FROM debian:11 
LABEL org.opencontainers.image.authors="Alexander@volzit.de"

#dotnet config
ENV ASPNETCORE_URLS="http://+:5000"

#install prereqs
RUN apt-get update -qqy && DEBIAN_FRONTEND=noninteractive apt-get install -y \
    wget apt-transport-https
RUN wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt-get update -qqy && apt-get install -y aspnetcore-runtime-6.0
RUN eval apt-get clean && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

#copy OcrWaterMeter files
WORKDIR /OcrWaterMeter
COPY --from=build /OcrWaterMeter/Server/bin/Release/net6.0/publish /OcrWaterMeter
RUN chmod +x OcrWaterMeter.Server

#Expose the port used
EXPOSE 5000/tcp

#User 
RUN groupadd -g 1010 -r ocrwatermeter && useradd --no-log-init -u 1010 -r -g ocrwatermeter ocrwatermeter
USER ocrwatermeter

# run
CMD [ "/bin/sh", "-c", "/OcrWaterMeter/OcrWaterMeter.Server" ]