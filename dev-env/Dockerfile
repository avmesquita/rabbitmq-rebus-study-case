FROM mcr.microsoft.com/dotnet/sdk:10.0

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        curl \
        docker.io \
        git \
        sudo \
        zsh \
    && curl -fsSL https://code-server.dev/install.sh | sh \
    && rm -rf /var/lib/apt/lists/* \
    && echo "ubuntu ALL=(root) NOPASSWD:ALL" > /etc/sudoers.d/ubuntu \
    && chmod 0440 /etc/sudoers.d/ubuntu \
    && mkdir -p /workspace /home/ubuntu/.local/share/code-server \
    && chown -R ubuntu:ubuntu /workspace /home/ubuntu

COPY dev-env/entrypoint.sh /usr/local/bin/dev-env-entrypoint

RUN chmod +x /usr/local/bin/dev-env-entrypoint

USER ubuntu
WORKDIR /workspace

EXPOSE 8080

CMD ["/usr/local/bin/dev-env-entrypoint"]