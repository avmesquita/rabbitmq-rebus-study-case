#!/bin/sh
set -eu

repository_url="${REPOSITORY_URL:-https://github.com/avmesquita/rabbitmq-rebus-study-case.git}"
repository_ref="${REPOSITORY_REF:-main}"

if [ ! -d /workspace/.git ]; then
    if [ "$(find /workspace -mindepth 1 -maxdepth 1 -print -quit)" ]; then
        echo "The workspace volume is not empty and does not contain a Git repository." >&2
        exit 1
    fi

    git clone --branch "$repository_ref" --single-branch "$repository_url" /workspace
fi

exec code-server \
    --bind-addr 0.0.0.0:8080 \
    --auth password \
    /workspace