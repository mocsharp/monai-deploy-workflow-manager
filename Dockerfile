# Copyright 2021 MONAI Consortium
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#     http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal as build

ARG Version=0.0.0
ARG FileVersion=0.0.0.0

# Install the tools
RUN dotnet tool install --tool-path /tools dotnet-trace
RUN dotnet tool install --tool-path /tools dotnet-dump
RUN dotnet tool install --tool-path /tools dotnet-counters
RUN dotnet tool install --tool-path /tools dotnet-stack
WORKDIR /app
COPY . ./

RUN echo "Building MONAI Workflow Manager $Version ($FileVersion)..."
RUN dotnet publish -c Release -o out --nologo /p:Version=$Version /p:FileVersion=$FileVersion src/WorkflowManager/Monai.Deploy.WorkflowManager.csproj

RUN echo "Fetching mc executable for minio..."
RUN wget https://dl.min.io/client/mc/release/linux-amd64/mc
RUN chmod +x mc

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get clean \
 && apt-get update \
 && apt-get install -y --no-install-recommends \
    libssl1.1 \
    openssl \
    sqlite3 \
   && rm -rf /var/lib/apt/lists

WORKDIR /opt/monai/wm
COPY --from=build /app/out .
#COPY docs/compliance/open-source-licenses.md .

COPY --from=build /tools /opt/dotnetcore-tools

COPY --from=build /app/mc /usr/local/bin/mc
# RUN mv mc /usr/local/bin/mc

EXPOSE 104
EXPOSE 5000

RUN ls -lR /opt/monai/wm
ENV PATH="/opt/dotnetcore-tools:${PATH}"

ENTRYPOINT ["/opt/monai/wm/Monai.Deploy.WorkflowManager"]
