# OcrWaterMeter

[![linux/amd64](https://github.com/TheR00st3r/OcrWaterMeter/actions/workflows/build-and-push-docker.yml/badge.svg)](https://github.com/TheR00st3r/OcrWaterMeter/actions/workflows/build-and-push-docker.yml)


OcrWaterMeter is a configurable open source software solution built with dotnet 6.0 that takes a picture from your watermeter via a http(s) url and reads the values of it, so you can monitor your water consumption. 

You can find a image on [Docker Hub](https://hub.docker.com/r/hahni91/ocrwatermeter)

## Running OcrWaterMeter

### Prerequirements

- [Docker](https://docs.docker.com/get-docker/)
- [Docker-compose](https://docs.docker.com/compose/install/)
- continous updating photo of your watermeter that is reachable via http(s)

### Installation

- Download or clone this repository (or the docker-compose.yml)
- if you want to use a different port than 5000/tcp, change the first 5000 the docker-compose.yml
- run `docker-compose up -d`
- visit your OcrWaterMeter installation with your browser and configure it 


### Usage

Configuration: https://host:5000/config
Pure Value: https://host:5000/WaterMeter/Value

