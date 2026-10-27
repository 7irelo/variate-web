FROM python:3.13-slim AS build
WORKDIR /app
ENV PIP_NO_CACHE_DIR=1 PIP_DISABLE_PIP_VERSION_CHECK=1
COPY requirements.txt .
RUN pip install --prefix=/install -r requirements.txt
COPY . .
# Train the distilled router from the gold split so the image starts with a
# model. Replace it with a teacher-distilled artefact via `python -m distill.train`.
RUN PYTHONPATH=/install/lib/python3.13/site-packages python -m distill.train --labels gold

FROM python:3.13-slim AS final
WORKDIR /app
ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1 ASSISTANT_ENVIRONMENT=production
COPY --from=build /install /usr/local
COPY --from=build /app/assistant ./assistant
COPY --from=build /app/data ./data
COPY --from=build /app/evals/datasets ./evals/datasets
COPY --from=build /app/models ./models
RUN useradd --uid 10001 --no-create-home --shell /usr/sbin/nologin assistant && chown -R assistant /app/models
USER 10001
EXPOSE 8080
ENTRYPOINT ["uvicorn", "assistant.main:app", "--host", "0.0.0.0", "--port", "8080", "--no-server-header"]
